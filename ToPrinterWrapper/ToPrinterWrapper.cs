using System.Diagnostics;
using System.Text;
using System.Collections.Concurrent;

namespace ToPrinterWrapper
{
    /// <summary>
    /// Provides automated printing functionality using 2Printer and PDF printers with async, concurrency, and cancellation support.
    /// </summary>
    public class ToPrinter
    {
        public string PrintPath { get; } = @"C:\ToPrinter\";
        public int MaxConcurrentPrintingJobs { get; } = 10;

        private const string ToPrinterCommand = "2Printer.exe";

        private readonly SemaphoreSlim _concurrentPrintingSemaphore;       
        private readonly CancellationTokenSource _shutdownCts = new();
        private bool _shutdown = false;
        private static readonly FileDeleteQueue _fileDeleteQueue = new();
        /// <summary>
        /// Gets or sets a value indicating whether to log output and errors to the console.
        /// </summary>
        public bool Log { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToPrinter"/> class.
        /// </summary>
        /// <param name="printPath">The directory to use for print temp files.</param>
        /// <param name="maxConcurrentPrintingJobs">The maximum number of concurrent printing jobs.</param>
        public ToPrinter(string printPath = @"C:\ToPrinter\", int maxConcurrentPrintingJobs = 10)
        {
            PrintPath = printPath;
            MaxConcurrentPrintingJobs = maxConcurrentPrintingJobs;

            _concurrentPrintingSemaphore = new SemaphoreSlim(MaxConcurrentPrintingJobs);
            
            if (!Directory.Exists(PrintPath))
            {
                Directory.CreateDirectory(PrintPath);
            }
        }
          
        /// <summary>
        /// Initiates shutdown: cancels all current and pending print jobs and disposes the semaphore.
        /// </summary>
        public async Task ShutdownAsync()
        {
            if (_shutdown) return;
            _shutdown = true;
            _shutdownCts.Cancel();

            for (int i = 0; i < MaxConcurrentPrintingJobs; i++)
            {
                await _concurrentPrintingSemaphore.WaitAsync();
            }

            _concurrentPrintingSemaphore?.Dispose();
            _fileDeleteQueue?.Dispose();
        }

        /// <summary>
        /// Prints a document using the specified 2Printer arguments.
        /// </summary>
        /// <param name="arguments">The command-line arguments for 2Printer.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The exit code from 2Printer.</returns>
        public async Task<int> PrintDocumentAsync(string arguments, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                throw new ArgumentException("Arguments cannot be null or empty.", nameof(arguments));
            }
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownCts.Token);
            await _concurrentPrintingSemaphore.WaitAsync(linkedCts.Token);
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = ToPrinterCommand,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                
                using (var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = false })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            outputBuilder.AppendLine(e.Data);
                            if (Log) Console.WriteLine($"Output: {e.Data}");
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            errorBuilder.AppendLine(e.Data);
                            if (Log) Console.WriteLine($"Error: {e.Data}");
                        }
                    };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    using (linkedCts.Token.Register(() => { try { if (!process.HasExited) process.Kill(); } catch { } }))
                    {
                        await process.WaitForExitAsync(linkedCts.Token);
                    }
                    if (Log) Console.WriteLine($"ExitCode : {process.ExitCode} - {process.ExitCode.ToDescription()}");
                    return process.ExitCode;
                }
            }
            finally
            {
                _concurrentPrintingSemaphore.Release();
            }
        }

        /// <summary>
        /// Prints a document from a file using the specified printer and options.
        /// </summary>
        /// <param name="fileName">The path to the file to print.</param>
        /// <param name="printerName">The name of the printer.</param>
        /// <param name="printOptions">The print options.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The exit code from 2Printer.</returns>
        public async Task<int> PrintDocumentAsync(string fileName, string printerName, PrintOptions printOptions, CancellationToken cancellationToken = default)
        {
            string arguments = $"-src \"{fileName}\" -prn \"{printerName}\" -options alerts:no silent:yes log:no -props spjob:yes {printOptions.BuildArguments()}";
            if (Log) Console.WriteLine($"Executing: {ToPrinterCommand} {arguments}");

            var delete = printOptions.DeleteFile;
            printOptions.DeleteFile = null;
            var exitCode =  await PrintDocumentAsync(arguments, cancellationToken);
            if(delete == true) _fileDeleteQueue.Enqueue(fileName);
            return exitCode;
        }

        /// <summary>
        /// Prints a document from a stream using the specified printer and options.
        /// </summary>
        /// <param name="stream">The stream containing the document to print.</param>
        /// <param name="printerName">The name of the printer.</param>
        /// <param name="printOptions">The print options.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The exit code from 2Printer.</returns>
        public async Task<int> PrintDocumentAsync(Stream stream, string printerName, PrintOptions printOptions, CancellationToken cancellationToken = default)
        {
            string tempFile = Path.Combine(PrintPath, $"{Guid.NewGuid().ToString()}.tmp");
            printOptions.DeleteFile = null;
           
            using (var fileStream = File.OpenWrite(tempFile))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream, 81920, cancellationToken);
            }
            var exitCode = await PrintDocumentAsync(tempFile, printerName, printOptions, cancellationToken);
            _fileDeleteQueue.Enqueue(tempFile);
            return exitCode;
        }

        /// <summary>
        /// Checks if the specified printer is online using 2Printer's -prnlist command.
        /// </summary>
        /// <param name="printerName">The name of the printer to check.</param>
        /// <returns>True if the printer is online, false otherwise.</returns>
        public async Task<bool> IsPrinterOnlineAsync(string printerName)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ToPrinterCommand,
                Arguments = $"-prn \"{printerName}\" -options getprinterstatus:yes alerts:no silent:yes log:no",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                await process.WaitForExitAsync();
                if (Log) Console.WriteLine($"ExitCode : {process.ExitCode} - {process.ExitCode.ToDescription()}");
                return process.ExitCode == 0;
            }
        }
    }

    /// <summary>
    /// Background file deleter that safely queues files for deletion and retries if locked.
    /// </summary>
    public class FileDeleteQueue : IDisposable
    {
        private readonly ConcurrentDictionary<string, byte> _files = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;
        private readonly int _maxRetries = 5;

        public FileDeleteQueue()
        {
            _worker = Task.Run(ProcessSetAsync);
        }

        public void Enqueue(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _files.TryAdd(filePath, 0);
            }
        }

        private async Task ProcessSetAsync()
        {
            var retryCounts = new ConcurrentDictionary<string, int>();
            while (!_cts.IsCancellationRequested)
            {
                foreach (var file in _files.Keys)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                        _files.TryRemove(file, out _);
                        retryCounts.TryRemove(file, out _);
                    }
                    catch (IOException)
                    {
                        int count = retryCounts.AddOrUpdate(file, 1, (_, c) => c + 1);
                        if (count >= _maxRetries)
                        {
                            _files.TryRemove(file, out _);
                            retryCounts.TryRemove(file, out _);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        int count = retryCounts.AddOrUpdate(file, 1, (_, c) => c + 1);
                        if (count >= _maxRetries)
                        {
                            _files.TryRemove(file, out _);
                            retryCounts.TryRemove(file, out _);
                        }
                    }
                    catch
                    {
                        _files.TryRemove(file, out _);
                        retryCounts.TryRemove(file, out _);
                    }
                }
                await Task.Delay(1000, _cts.Token);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _worker.Wait(); } catch { }
            _cts.Dispose();
        }
    }
}
