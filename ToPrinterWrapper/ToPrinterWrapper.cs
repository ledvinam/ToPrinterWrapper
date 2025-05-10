using System.Diagnostics;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ToPrinterWrapper
{
    /// <summary>
    /// Provides automated printing functionality using 2Printer and PDF printers with async, concurrency, and cancellation support.
    /// </summary>
    public class ToPrinter : IAsyncDisposable
    {
        #region Public Properties
        
        /// <summary>
        /// Print path for temporary files.
        /// </summary>
        public string PrintPath { get; } = @"C:\ToPrinter\";

        /// <summary>
        /// Maximum number of concurrent printing jobs.
        /// </summary>
        public int MaxConcurrentPrintingJobs { get; } = 10;

        /// <summary>
        /// Command-line executable for 2Printer.
        /// </summary>
        public const string ToPrinterCommand = "2Printer.exe";

        /// <summary>
        /// Gets or sets a value indicating whether to log output and errors to the console.
        /// </summary>
        public bool Log { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to write output and errors to the console.
        /// </summary>
        public bool Silent { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to check the printer status before printing.
        /// </summary>
        public bool CheckPrinterStatus { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to throw exceptions on errors.
        /// </summary>
        public bool ThrowExceptions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable memory pressure handling.
        /// </summary>
        public bool MemoryPressureEnabled { get; set; } = false;

        /// <summary>
        /// Maximum allowed system memory usage ratio (0.0 - 1.0). If exceeded, new print jobs will wait until memory pressure is relieved. Default: 0.9 (90%).
        /// </summary>
        public double MaxSystemMemoryUsageRatio { get; set; } = 0.9;

        /// <summary>
        /// How often (ms) to poll for memory pressure relief when waiting. Default: 1000ms.
        /// </summary>
        public int MemoryPressurePollIntervalMs { get; set; } = 1000;

        #endregion

        private readonly SemaphoreSlim _concurrentPrintingSemaphore;       
        private readonly CancellationTokenSource _shutdownCts = new();
        private bool _shutdown = false;
        private static readonly FileDeleteQueue _fileDeleteQueue = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ToPrinter"/> class.
        /// </summary>
        /// <param name="printPath">The directory to use for print temp files.</param>
        /// <param name="maxConcurrentPrintingJobs">The maximum number of concurrent printing jobs.</param>
        public ToPrinter(string printPath = @"C:\ToPrinter\", int maxConcurrentPrintingJobs = 10)
        {
            // Validate RAM disk or fallback to default temp path
            if (!Directory.Exists(printPath))
            {
                try
                {
                    Directory.CreateDirectory(printPath);
                }
                catch
                {
                    // Fallback to system temp path if RAM disk or custom path is not available
                    printPath = Path.GetTempPath();
                    if (!Directory.Exists(printPath))
                    {
                        Directory.CreateDirectory(printPath);
                    }
                }
            }

            PrintPath = printPath;
            MaxConcurrentPrintingJobs = maxConcurrentPrintingJobs;
            _concurrentPrintingSemaphore = new SemaphoreSlim(MaxConcurrentPrintingJobs);
        }
          
        /// <summary>
        /// Initiates shutdown: cancels all current and pending print jobs and disposes the semaphore.
        /// </summary>
        public async Task ShutdownAsync()
        {
            if (_shutdown) return;
            if(Log) Console.WriteLine("Shutting down ToPrinter...");
            _shutdown = true;
            _shutdownCts.Cancel();

            for (int i = 0; i < MaxConcurrentPrintingJobs; i++)
            {
                await _concurrentPrintingSemaphore.WaitAsync();
            }

            _concurrentPrintingSemaphore?.Dispose();
            _fileDeleteQueue?.Dispose();
            if(Log) Console.WriteLine("ToPrinter shutdown complete.");
        }

        public async ValueTask DisposeAsync() => await ShutdownAsync();
        
        /// <summary>
        /// Prints a document using the specified 2Printer arguments, with optional timeout.
        /// </summary>
        /// <param name="filePath">The path to the file to print.</param>
        /// <param name="printerName">The name of the printer.</param>
        /// <param name="arguments">The command-line arguments for 2Printer.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <param name="timeout">Optional timeout for the print process.</param>
        /// <returns>The exit code from 2Printer.</returns>
        public async Task<int> PrintDocumentAsync(string filePath, string printerName, string arguments, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                if (ThrowExceptions) throw new ArgumentException("Arguments cannot be null or empty.", nameof(arguments));
                return ErrorCodes.InvalidParameter;
            }

            if(CheckPrinterStatus)
            {
                var printerError = await IsPrinterOnlineAsync(printerName);
                if (printerError != 0)
                {
                    if (Log) Console.WriteLine($"Printer '{printerName}' is not online. Error code: {printerError}");
                    if (ThrowExceptions) throw new InvalidOperationException($"Printer '{printerName}' is not online. Error code: {printerError}");
                    return printerError;
                }
            }

            var args = $"-src \"{filePath}\" -prn \"{printerName}\" {arguments}";
            if (Log) Console.WriteLine($"Executing: {ToPrinterCommand} {args}");

            using var timeoutCts = timeout.HasValue ? new CancellationTokenSource(timeout.Value) : null;
            using var linkedCts = timeoutCts != null
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownCts.Token, timeoutCts.Token)
                : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownCts.Token);

            // Wait for system memory pressure to be relieved before starting a new print job
            while (MemoryPressureEnabled && IsSystemMemoryPressureHigh(MaxSystemMemoryUsageRatio))
            {
                if (Log) Console.WriteLine($"[ToPrinter] System memory pressure high (>{MaxSystemMemoryUsageRatio:P0}). Waiting before starting new print job...");
                await Task.Delay(MemoryPressurePollIntervalMs, linkedCts.Token);
            }

            await _concurrentPrintingSemaphore.WaitAsync(linkedCts.Token);
            var semaphoreReleased = false;
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = ToPrinterCommand,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = false };

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    if (ThrowExceptions) throw new InvalidOperationException($"Failed to start process '{ToPrinterCommand}'.", ex);
                    if (Log) Console.WriteLine($"Failed to start process: {ex.Message}");
                    return ErrorCodes.ExeLaunchError;
                }

#if NET6_0
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
#else
                var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
#endif

                using (linkedCts.Token.Register(() => { try { if (!process.HasExited) process.Kill(); } catch { } }))
                {
                    try
                    {
                        await process.WaitForExitAsync(linkedCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Ensure output and error are read even if cancelled
                        await Task.WhenAll(outputTask, errorTask);
                        if (ThrowExceptions) throw;
                        return ErrorCodes.CanceledByUser;
                    }
                }

                var output = await outputTask;
                var error = await errorTask;

                if (Log)
                {
                    Console.WriteLine($"Output: {output}");
                    Console.WriteLine($"Error: {error}");
                    Console.WriteLine($"ExitCode : {process.ExitCode} - {process.ExitCode.ToDescription()}");
                }

                if (process.ExitCode != 0 && ThrowExceptions)
                {
                    throw new InvalidOperationException($"Printer exited with code {process.ExitCode}: {error}");
                }

                return process.ExitCode;
            }
            finally
            {
                if (!semaphoreReleased)
                {
                    _concurrentPrintingSemaphore.Release();
                    semaphoreReleased = true;
                }
            }
        }

        private async Task<int> PrintDocumentInternalAsync(string fileName, string printerName, PrintOptions printOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            string args = $"-options alerts:no silent:{(Silent ? "yes" : "no")} log:no -props spjob:yes {printOptions.BuildArguments()}";
            var delete = printOptions.DeleteFile;
            printOptions.DeleteFile = null;
            var exitCode =  await PrintDocumentAsync(fileName, printerName, args, timeout, cancellationToken);
            if(delete == true) DeleteFile(fileName);
            return exitCode;
        }

        /// <summary>
        /// Prints a document from a file using the specified printer and options.
        /// </summary>
        /// <param name="fileName">The path to the file to print.</param>
        /// <param name="printerName">The name of the printer.</param>
        /// <param name="printOptions">The print options.</param>
        /// <param name="timeout">Optional timeout for the print process.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The exit code from 2Printer.</returns>
        public async Task<int> PrintDocumentAsync(string fileName, string printerName, PrintOptions printOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            string tempFile = Path.Combine(PrintPath, $"{Guid.NewGuid().ToString()}.tmp");
            var delete = printOptions.DeleteFile;
            printOptions.DeleteFile = null;
            
            await using (var fileStream = File.OpenRead(fileName))
            {
                await using (var tempStream = File.OpenWrite(tempFile))
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    await fileStream.CopyToAsync(tempStream, 81920, cancellationToken);
                }
            }

            var exitCode =  await PrintDocumentInternalAsync(tempFile, printerName, printOptions, timeout, cancellationToken);
            DeleteFile(tempFile);
            if(delete == true) DeleteFile(fileName);
            return exitCode;
        }

        /// <summary>
        /// Prints a document from a stream using the specified printer and options.
        /// </summary>
        /// <param name="stream">The stream containing the document to print.</param>
        /// <param name="printerName">The name of the printer.</param>
        /// <param name="printOptions">The print options.</param>
        /// <param name="timeout">Optional timeout for the print process.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The exit code from 2Printer.</returns>
        public async Task<int> PrintDocumentAsync(Stream stream, string printerName, PrintOptions printOptions, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            string tempFile = Path.Combine(PrintPath, $"{Guid.NewGuid().ToString()}.tmp");
            printOptions.DeleteFile = null;
           
            await using (var fileStream = File.OpenWrite(tempFile))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(fileStream, 81920, cancellationToken);
            }

            var exitCode = await PrintDocumentInternalAsync(tempFile, printerName, printOptions, timeout, cancellationToken);
            DeleteFile(tempFile);
            return exitCode;
        }

        private void DeleteFile(string fileName)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
            }
            catch (IOException)
            {
                _fileDeleteQueue.Enqueue(fileName);
            }
            catch (UnauthorizedAccessException)
            {
                _fileDeleteQueue.Enqueue(fileName);
            }
            // Other exceptions are ignored for now
        }

        /// <summary>
        /// Checks if the specified printer is online using 2Printer's -prnlist command.
        /// </summary>
        /// <param name="printerName">The name of the printer to check.</param>
        /// <returns>True if the printer is online, false otherwise.</returns>
        public async Task<int> IsPrinterOnlineAsync(string printerName)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ToPrinterCommand,
                Arguments = $"-prn \"{printerName}\" -options getprinterstatus:yes alerts:no silent:no log:no",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                if (ThrowExceptions) throw new InvalidOperationException($"Failed to start process '{ToPrinterCommand}'.", ex);
                if (Log) Console.WriteLine($"Failed to start process: {ex.Message}");
                return ErrorCodes.ExeLaunchError;
            }

            await process.WaitForExitAsync();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (Log)
            {
                Console.WriteLine($"Output: {output}");
                Console.WriteLine($"Error: {error}");
                Console.WriteLine($"ExitCode : {process.ExitCode} - {process.ExitCode.ToDescription()}");
            }

            if(process.ExitCode == 0 && output.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                if (ThrowExceptions) throw new InvalidOperationException($"Printer '{printerName}' not found.");
                return ErrorCodes.PrinterNotFound;
            }
            else if (process.ExitCode == 0 && output.Contains("offline", StringComparison.OrdinalIgnoreCase))
            {
                if (ThrowExceptions) throw new InvalidOperationException($"Printer '{printerName}' is offline.");
                return ErrorCodes.PrinterOffline;
            }
            else if (process.ExitCode == 0 && output.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                if (ThrowExceptions) throw new InvalidOperationException($"Printer '{printerName}' error: {output}");
                return ErrorCodes.PrinterError;
            }
            else if (process.ExitCode == 3)
            {
                return 0;
            }

            if (process.ExitCode != 0 && ThrowExceptions)
            {
                throw new InvalidOperationException($"Printer exited with code {process.ExitCode}: {error}");
            }

            return process.ExitCode;
        }

#if NET6_0_OR_GREATER
        private static bool IsSystemMemoryPressureHigh(double maxUsageRatio)
        {
            // Windows only: use GlobalMemoryStatusEx via P/Invoke for system-wide memory
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                ulong total = memStatus.ullTotalPhys;
                ulong avail = memStatus.ullAvailPhys;
                double usedRatio = (double)(total - avail) / total;
                return usedRatio >= maxUsageRatio;
            }
            // If unable to determine, be conservative and do not block
            return false;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] MEMORYSTATUSEX lpBuffer);
#else
        private static bool IsSystemMemoryPressureHigh(double maxUsageRatio)
        {
            // Not supported on this platform, do not block
            return false;
        }
#endif
    }
}
