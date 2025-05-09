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
}