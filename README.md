# ToPrinterWrapper

A .NET static wrapper for 2Printer command-line printing automation, supporting synchronous and asynchronous printing, concurrency control, and robust testability.

## Features
- Static API for printing documents via 2Printer
- Synchronous and async (Task-based) methods
- Print from file path, stream, or argument string
- Print options: copies, duplex, color, orientation, paper size, page range, scale, zoom, tray
- Concurrency control: limit the number of simultaneous print jobs (default 10, configurable)
- Unit-testable and automation-friendly
- Example integration with Bullzip PDF Printer for automated PDF output

## Usage

### Setting Max Concurrent Jobs
```csharp
ToPrinter.SetMaxConcurrentPrintingJobs(5); // Set max concurrent jobs to 5
```

### Printing a Document
```csharp
var options = new PrintOptions { Copies = 1, Duplex = DuplexMode.Simplex };
int exitCode = ToPrinter.PrintDocument("file.pdf", "Bullzip PDF Printer", options);
```

### Async Printing
```csharp
await ToPrinter.PrintDocumentAsync("file.pdf", "Bullzip PDF Printer", options);
```

### Print from Stream
```csharp
using (var stream = File.OpenRead("file.pdf"))
{
    ToPrinter.PrintDocument(stream, "Bullzip PDF Printer", options);
}
```

## Speeding Up Temporary File Operations with a RAM Disk

For even faster print jobs and to reduce SSD wear, you can use a RAM disk for temporary files. This is especially useful when printing from streams, as the library writes the stream to a temporary file before printing.

### Using OSFMount to Create a RAM Disk

You can use [OSFMount](https://www.osforensics.com/tools/mount-disk-images.html) to create a RAM disk. Example command to create a 1GB NTFS RAM disk mounted as drive `R:` and labeled "PRINT":

```powershell
& MOUNT     "C:\Program Files\OSFMount\osfmount.com" -a -t vm -m R: -o format:ntfs:"PRINT" -s 1G
  UNMOUNT   "C:\Program Files\OSFMount\osfmount.com" -l -m R:
```
- `-a -t vm` : Add a virtual memory (RAM) disk
- `-m R:` : Mount as drive R:
- `-o format:ntfs:"PRINT"` : Format as NTFS and label as "PRINT"
- `-s 1G` : Size 1GB

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
- .NET 6.0 or later
- 2Printer.exe in PATH or working directory
- (Optional) Bullzip PDF Printer for automated PDF output

## License
MIT License
