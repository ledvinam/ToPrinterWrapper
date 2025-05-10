using System.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ToPrinterWrapper
{
    /// <summary>
    /// Hosted service for managing ToPrinter lifecycle and dependency injection in ASP.NET Core.
    /// </summary>
    public class ToPrinterWrapperService : IHostedService, IAsyncDisposable
    {
        private readonly ToPrinter _printer;
        /// <summary>
        /// Gets the ToPrinter instance used by this service.
        /// </summary>
        public ToPrinter Printer => _printer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToPrinterWrapperService"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options for configuring the ToPrinter instance.</param>
        public ToPrinterWrapperService(IOptions<ToPrinterOptions> options)
        {
            _printer = new ToPrinter(options.Value.PrintPath, options.Value.MaxConcurrentPrintingJobs)
            {
                Log = options.Value.Log,
                Silent = options.Value.Silent,
                CheckPrinterStatus = options.Value.CheckPrinterStatus,
                ThrowExceptions = options.Value.ThrowExceptions
            };
        }

        /// <summary>
        /// Starts the hosted service. No-op for ToPrinter.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Stops the hosted service and gracefully shuts down the ToPrinter instance.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _printer.ShutdownAsync();
        }

        /// <summary>
        /// Disposes the ToPrinter instance asynchronously.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            return new ValueTask(_printer.ShutdownAsync());
        }
    }

    /// <summary>
    /// Options for configuring the ToPrinter instance.
    /// </summary>
    public class ToPrinterOptions
    {
        /// <summary>
        /// Gets or sets the directory path for print temp files.
        /// </summary>
        public string PrintPath { get; set; } = @"C:\ToPrinter\";
        /// <summary>
        /// Gets or sets the maximum number of concurrent printing jobs.
        /// </summary>
        public int MaxConcurrentPrintingJobs { get; set; } = 10;
        /// <summary>
        /// Gets or sets a value indicating whether to log the printing process.
        /// </summary>
        public bool Log { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether to show alerts during the printing process.
        /// </summary>
        public bool Silent { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether to check the printer status before printing.
        /// </summary>
        public bool CheckPrinterStatus { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether to throw exceptions on errors.
        /// </summary>
        public bool ThrowExceptions { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether to enable memory pressure handling.
        /// </summary>
        public bool MemoryPressureEnabled { get; set; } = false;
        /// <summary>
        /// Gets or sets the maximum system memory usage ratio for the ToPrinter instance.
        /// </summary>
        public double MaxSystemMemoryUsageRatio { get; set; } = 0.9;
        /// <summary>
        /// Gets or sets the maximum memory usage ratio for the ToPrinter instance.
        /// </summary>
        public int MemoryPressurePollIntervalMs { get; set; } = 1000;
    }
}
