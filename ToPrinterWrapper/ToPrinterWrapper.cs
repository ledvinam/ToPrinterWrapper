using System.Diagnostics;
using System.Text;

namespace ToPrinterWrapper
{
    /// <summary>
    /// Provides automated printing functionality using 2Printer and PDF printers with async, concurrency, and cancellation support.
    /// </summary>
    public class ToPrinter
    {
        private const string ToPrinterCommand = "2Printer.exe";
        private readonly SemaphoreSlim _concurrentPrintingSemaphore;
        private int _maxConcurrentPrintingJobs;
        private string _printPath;
        private readonly CancellationTokenSource _shutdownCts = new();
        private bool _shutdown = false;
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
            _maxConcurrentPrintingJobs = maxConcurrentPrintingJobs;
            _printPath = printPath;
            _concurrentPrintingSemaphore = new SemaphoreSlim(_maxConcurrentPrintingJobs);
            if (!Directory.Exists(_printPath))
            {
                Directory.CreateDirectory(_printPath);
            }
        }

        /// <summary>
        /// Sets the maximum number of concurrent printing jobs allowed.
        /// </summary>
        /// <param name="maxJobs">The maximum number of concurrent jobs.</param>
        public void SetMaxConcurrentPrintingJobs(int maxJobs)
        {
            if (maxJobs < 1) throw new ArgumentOutOfRangeException(nameof(maxJobs), "Must be at least 1.");
            if (maxJobs == _maxConcurrentPrintingJobs) return;
            var oldSemaphore = _concurrentPrintingSemaphore;
            var newSemaphore = new SemaphoreSlim(maxJobs);
            _maxConcurrentPrintingJobs = maxJobs;
            oldSemaphore.Dispose();
        }

        /// <summary>
        /// Sets the print path for temporary files. Ensures the directory exists.
        /// </summary>
        /// <param name="printPath">The directory to use for print temp files.</param>
        public void SetPrintPath(string printPath)
        {
            if (string.IsNullOrWhiteSpace(printPath))
                throw new ArgumentException("Print path cannot be null or empty.", nameof(printPath));
            if (!Directory.Exists(printPath))
                Directory.CreateDirectory(printPath);
            _printPath = printPath;
        }

        /// <summary>
        /// Initiates shutdown: cancels all current and pending print jobs and disposes the semaphore.
        /// </summary>
        public async Task ShutdownAsync()
        {
            if (_shutdown) return;
            _shutdown = true;
            _shutdownCts.Cancel();
            for (int i = 0; i < _maxConcurrentPrintingJobs; i++)
            {
                await _concurrentPrintingSemaphore.WaitAsync();
            }
            _concurrentPrintingSemaphore.Dispose();
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
            return await PrintDocumentAsync(arguments, cancellationToken);
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
            string tempFile = Path.Combine(_printPath, $"{Guid.NewGuid()}.tmp");
            printOptions.DeleteFile ??= true;
            try
            {
                using (var fileStream = File.OpenWrite(tempFile))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(fileStream, 81920, cancellationToken);
                }
                return await PrintDocumentAsync(tempFile, printerName, printOptions, cancellationToken);
            }
            finally
            {
                if (printOptions.DeleteFile == true && File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { /* log or ignore */ }
                }
            }
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
}
