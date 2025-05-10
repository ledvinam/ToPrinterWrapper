using System.Text;

namespace ToPrinterWrapper
{
    /// <summary>
    /// Specifies the duplex printing mode.
    /// </summary>
    public enum DuplexMode
    {
        /// <summary>Use the printer's default duplex mode.</summary>
        Default,
        /// <summary>Single-sided printing.</summary>
        Simplex,
        /// <summary>Double-sided printing, vertical flip.</summary>
        Vertical,
        /// <summary>Double-sided printing, horizontal flip.</summary>
        Horizontal
    }

    /// <summary>
    /// Specifies the color mode for printing.
    /// </summary>
    public enum ColorMode
    {
        /// <summary>Use the printer's default color mode.</summary>
        Default,
        /// <summary>Print in color.</summary>
        Color,
        /// <summary>Print in grayscale.</summary>
        Grayscale,
        /// <summary>Use the color mode as in the document.</summary>
        AsInDocument
    }

    /// <summary>
    /// Specifies the orientation for printing.
    /// </summary>
    public enum Orientation
    {
        /// <summary>Portrait orientation.</summary>
        Portrait,
        /// <summary>Landscape orientation.</summary>
        Landscape,
        /// <summary>Use the orientation as in the document.</summary>
        AsInDocument,
        /// <summary>Use the orientation as in the printer.</summary>
        AsInPrinter
    }

    /// <summary>
    /// Specifies the paper size for printing.
    /// </summary>
    public enum PageSize
    {
        /// <summary>Use the printer's default paper size.</summary>
        Default = -1,
        /// <summary>Use the page size based on the document.</summary>
        PageSizeBased = -2,
        /// <summary>A4 paper size.</summary>
        A4,
        /// <summary>A3 paper size.</summary>
        A3,
        /// <summary>Letter paper size.</summary>
        Letter,
        /// <summary>Legal paper size.</summary>
        Legal
    }

    /// <summary>
    /// Specifies the scaling mode for printing.
    /// </summary>
    public enum ScaleMode
    {
        /// <summary>Shrink to fit.</summary>
        Shrink,
        /// <summary>Fit to page.</summary>
        Fit,
        /// <summary>Original size.</summary>
        Original,
        /// <summary>Fill page.</summary>
        Fill,
        /// <summary>Booklet mode.</summary>
        Booklet,
        /// <summary>Zoom mode.</summary>
        Zoom
    }

    /// <summary>
    /// Represents a page range for printing (e.g., "1,3,5-12").
    /// </summary>
    public class PageRange
    {
        /// <summary>
        /// Gets the page range string.
        /// </summary>
        public string Range { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageRange"/> class.
        /// </summary>
        /// <param name="range">The page range string (e.g., "1,3,5-12").</param>
        /// <exception cref="ArgumentException">Thrown if the range format is invalid.</exception>
        public PageRange(string range)
        {
            if (string.IsNullOrWhiteSpace(range) || !System.Text.RegularExpressions.Regex.IsMatch(range, @"^\d+(-\d+)?(,\d+(-\d+)?)*$"))
            {
                throw new ArgumentException("Invalid page range format. Use formats like '1,3,5-12'.", nameof(range));
            }

            Range = range;
        }

        /// <inheritdoc/>
        public override string ToString() => Range;
    }

    /// <summary>
    /// Represents a zoom level for printing.
    /// </summary>
    public class ZoomLevel
    {
        /// <summary>
        /// Gets the zoom percentage (1-400).
        /// </summary>
        public int Percent { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomLevel"/> class.
        /// </summary>
        /// <param name="percent">The zoom percentage (1-400).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if percent is not between 1 and 400.</exception>
        public ZoomLevel(int percent)
        {
            if (percent < 1 || percent > 400)
            {
                throw new ArgumentOutOfRangeException(nameof(percent), "Zoom level must be between 1 and 400.");
            }

            Percent = percent;
        }

        /// <inheritdoc/>
        public override string ToString() => Percent.ToString();
    }

    /// <summary>
    /// Specifies the printer tray to use.
    /// </summary>
    public enum PrinterTray
    {
        /// <summary>Automatic bin selection.</summary>
        AutoBin,
        /// <summary>Tray 1.</summary>
        Tray1,
        /// <summary>Tray 2.</summary>
        Tray2,
        /// <summary>Tray 3.</summary>
        Tray3
    }

    /// <summary>
    /// Options for customizing print jobs.
    /// </summary>
    public class PrintOptions
    {
        /// <summary>Number of copies to print.</summary>
        public int? Copies { get; set; }
        /// <summary>Duplex printing mode.</summary>
        public DuplexMode? Duplex { get; set; }
        /// <summary>Color mode for printing.</summary>
        public ColorMode? Color { get; set; }
        /// <summary>Page orientation.</summary>
        public Orientation? Orientation { get; set; }
        /// <summary>Paper size.</summary>
        public PageSize? PaperSize { get; set; }
        /// <summary>Page range to print.</summary>
        public PageRange? Pages { get; set; }
        /// <summary>Scaling mode.</summary>
        public ScaleMode? Scale { get; set; }
        /// <summary>Zoom level.</summary>
        public ZoomLevel? ZoomPercent { get; set; }
        /// <summary>Printer tray selection.</summary>
        public PrinterTray? Tray { get; set; }
        /// <summary>Whether to delete the file after printing (for temp files).</summary>
        public bool? DeleteFile { get; set; }

        /// <summary>
        /// Builds the command-line arguments for 2Printer based on the options set.
        /// </summary>
        /// <returns>The command-line arguments string.</returns>
        public string BuildArguments()
        {
            var arguments = new StringBuilder();
            
            if (this.Copies.HasValue)
                arguments.Append($" copies:{this.Copies.Value}");

            if (this.Duplex.HasValue)
                arguments.Append($" duplex:{this.Duplex.Value.ToString().ToLower()}");

            if (this.Color.HasValue)
                arguments.Append($" color:{this.Color.Value.ToString().ToLower()}");

            if (this.Orientation.HasValue)
                arguments.Append($" orient:{this.Orientation.Value.ToString().ToLower()}");

            if (this.PaperSize.HasValue)
                arguments.Append($" papersize:{(int)this.PaperSize.Value}");

            if (this.Pages != null)
                arguments.Append($" pages:{this.Pages}");

            if (this.Scale.HasValue)
                arguments.Append($" scale:{this.Scale.Value.ToString().ToLower()}");

            if (this.ZoomPercent != null)
                arguments.Append($" zoom_percent:{this.ZoomPercent}");

            if (this.Tray.HasValue)
                arguments.Append($" tray:{this.Tray.Value.ToString().Replace("_", " ")}");

            if (this.DeleteFile.HasValue)
                arguments.Append($" -postproc passed:delete");

            return arguments.ToString().Trim();
        }
    }

    /// <summary>
    /// Fluent builder for PrintOptions.
    /// </summary>
    /// <remarks>
    /// All methods throw <see cref="ArgumentNullException"/> if required arguments are null.
    /// </remarks>
    public class PrintOptionsBuilder
    {
        private readonly PrintOptions _options = new();

        /// <summary>
        /// Sets the number of copies to print.
        /// </summary>
        /// <param name="copies">Number of copies.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetCopies(int copies)
        {
            _options.Copies = copies;
            return this;
        }
        /// <summary>
        /// Sets the duplex printing mode.
        /// </summary>
        /// <param name="duplex">Duplex mode.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetDuplex(DuplexMode duplex)
        {
            _options.Duplex = duplex;
            return this;
        }
        /// <summary>
        /// Sets the color mode for printing.
        /// </summary>
        /// <param name="color">Color mode.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetColor(ColorMode color)
        {
            _options.Color = color;
            return this;
        }
        /// <summary>
        /// Sets the page orientation.
        /// </summary>
        /// <param name="orientation">Orientation.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetOrientation(Orientation orientation)
        {
            _options.Orientation = orientation;
            return this;
        }
        /// <summary>
        /// Sets the paper size.
        /// </summary>
        /// <param name="paperSize">Paper size.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetPaperSize(PageSize paperSize)
        {
            _options.PaperSize = paperSize;
            return this;
        }
        /// <summary>
        /// Sets the page range to print.
        /// </summary>
        /// <param name="pages">Page range.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pages"/> is null.</exception>
        public PrintOptionsBuilder SetPages(PageRange pages)
        {
            if (pages == null) throw new ArgumentNullException(nameof(pages));
            _options.Pages = pages;
            return this;
        }
        /// <summary>
        /// Sets the scaling mode.
        /// </summary>
        /// <param name="scale">Scaling mode.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetScale(ScaleMode scale)
        {
            _options.Scale = scale;
            return this;
        }
        /// <summary>
        /// Sets the zoom level.
        /// </summary>
        /// <param name="zoom">Zoom level.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="zoom"/> is null.</exception>
        public PrintOptionsBuilder SetZoomPercent(ZoomLevel zoom)
        {
            if (zoom == null) throw new ArgumentNullException(nameof(zoom));
            _options.ZoomPercent = zoom;
            return this;
        }
        /// <summary>
        /// Sets the printer tray.
        /// </summary>
        /// <param name="tray">Printer tray.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetTray(PrinterTray tray)
        {
            _options.Tray = tray;
            return this;
        }
        /// <summary>
        /// Sets whether to delete the file after printing.
        /// </summary>
        /// <param name="deleteFile">Whether to delete the file.</param>
        /// <returns>The builder instance.</returns>
        public PrintOptionsBuilder SetDeleteFile(bool deleteFile)
        {
            _options.DeleteFile = deleteFile;
            return this;
        }
        /// <summary>
        /// Builds the PrintOptions instance.
        /// </summary>
        /// <returns>The constructed <see cref="PrintOptions"/>.</returns>
        public PrintOptions Build() => _options;
    }
}