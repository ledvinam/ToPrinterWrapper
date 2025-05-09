# ToPrinterWrapper

A modern, dependency-injection-friendly .NET wrapper for [2Printer](https://www.cmd2printer.com/) command-line printing automation, supporting asynchronous printing, concurrency control, robust testability, and ASP.NET Core integration.

> **Note:** This wrapper requires [2Printer](https://www.cmd2printer.com/) (by fCoder Group, Inc.), a commercial command-line printing tool. You must purchase a valid 2Printer license from [cmd2printer.com](https://www.cmd2printer.com/) to use this library in production.

## Features
- Instance-based API for printing documents via 2Printer (DI/ASP.NET Core ready)
- Async (Task-based) methods with cancellation and concurrency support
- Print from file path, stream, or argument string
- Print options: copies, duplex, color, orientation, paper size, page range, scale, zoom, tray
- Concurrency control: limit the number of simultaneous print jobs (default 10, configurable)
- Robust temp file management and error handling
- Unit-testable and automation-friendly
- ASP.NET Core integration via `ToPrinterWrapperService` and `ToPrinterOptions`
- Multi-targeting: .NET 6, 7, 8, 9
- Full XML documentation for all public APIs
- Example integration with Bullzip PDF Printer for automated PDF output

## Usage

### Dependency Injection (ASP.NET Core)
```csharp
// In Program.cs or Startup.cs
services.Configure<ToPrinterOptions>(config =>
{
    config.PrintPath = @"C:\\ToPrinter\\";
    config.MaxConcurrentPrintingJobs = 5;
});
services.AddHostedService<ToPrinterWrapperService>();
```

### Using ToPrinter in Your Code
```csharp
public class MyService
{
    private readonly ToPrinter _printer;
    public MyService(ToPrinterWrapperService wrapper)
    {
        _printer = wrapper.Printer;
    }
    public async Task PrintAsync()
    {
        var options = new PrintOptions { Copies = 1, Duplex = DuplexMode.Simplex };
        int exitCode = await _printer.PrintDocumentAsync("file.pdf", "Bullzip PDF Printer", options);
    }
}
```

### Print from Stream
```csharp
using (var stream = File.OpenRead("file.pdf"))
{
    await _printer.PrintDocumentAsync(stream, "Bullzip PDF Printer", options);
}
```

### Customizing Print Options
```csharp
var options = new PrintOptions
{
    Copies = 2,
    Duplex = DuplexMode.Vertical,
    Color = ColorMode.Color,
    Orientation = Orientation.Landscape,
    PaperSize = PageSize.A4,
    Pages = new PageRange("1-3,5"),
    Scale = ScaleMode.Fit,
    ZoomPercent = new ZoomLevel(120),
    Tray = PrinterTray.Tray1
};
```

## Speeding Up Temporary File Operations with a RAM Disk

For even faster print jobs and to reduce SSD wear, you can use a RAM disk for temporary files. This is especially useful when printing from streams, as the library writes the stream to a temporary file before printing.

### Using OSFMount to Create a RAM Disk

You can use [OSFMount](https://www.osforensics.com/tools/mount-disk-images.html) to create a RAM disk. Example command to create a 1GB NTFS RAM disk mounted as drive `R:` and labeled "PRINT":

```powershell
& "C:\Program Files\OSFMount\osfmount.com" -a -t vm -m R: -o format:ntfs:"PRINT" -s 1G
# To unmount:
& "C:\Program Files\OSFMount\osfmount.com" -l -m R:
```

### How to Use in Your Code

Set your temp file path to the RAM disk, for example:

```csharp
string tempFile = Path.Combine(@"R:\", Path.GetRandomFileName() + ".pdf");
// Use tempFile for printing...
```

You can make the RAM disk path configurable in your application for flexibility.

**Benefits:**
- Much faster read/write speeds for temp files
- No SSD wear from temp file operations
- RAM disk contents are cleared on reboot

## Concurrency Testing
- The library can be tested for concurrency by launching multiple print jobs in parallel and verifying the maximum concurrency does not exceed the configured limit.

## Requirements
- .NET 6.0 or later (multi-targets .NET 6, 7, 8, 9)
- [2Printer](https://www.cmd2printer.com/) (must be purchased separately)
- 2Printer.exe in PATH or working directory
- (Optional) Bullzip PDF Printer for automated PDF output

## License
MIT License
