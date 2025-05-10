using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
namespace ToPrinterWrapper.Tests
{
    public static class Tests{
        
        public const string PrintPath = @"R:\";
        public const string TestFilePath = "test.pdf";
        public const string TestPrinterName = "Bullzip PDF Printer";   
        public const string PrintMask = "test_printed_*.pdf"; 

        public static async Task CleanUpAsync()
        {
            if (!Directory.Exists(PrintPath)) return;
           
            await Task.Delay(1000);

            foreach (var file in Directory.GetFiles(PrintPath, PrintMask))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(PrintPath, "*.tmp"))
            {
                File.Delete(file);
            }
        }

        public static async Task<bool> ExistsAsync(short number)
        {
            if (!Directory.Exists(PrintPath)) return false;

            await Task.Delay(1000);

            return Directory.GetFiles(PrintPath, PrintMask).Length == number;
        }
    }

    public class ToPrinterTests
    {
        [Fact]
        public async Task PrintDocumentAsync_ValidOptions_ShouldReturnExitCodeZero()
        {
            await Tests.CleanUpAsync();
            // Arrange
            var printer = new ToPrinter(Tests.PrintPath);
            var printOptions = new PrintOptions
            {
                Copies = 1,
                Duplex = DuplexMode.Simplex,
                Color = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PageSize.A4
            };

            // Act
            int exitCode = await printer.PrintDocumentAsync(Tests.TestFilePath, Tests.TestPrinterName, printOptions);

            await Task.Delay(1000); // Wait for the file to be created

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(await Tests.ExistsAsync(1));          
        }

        [Fact]
        public async Task PrintDocumentAsync_Stream_ValidOptions_ShouldReturnExitCodeZero()
        {
            await Tests.CleanUpAsync();
            // Arrange
            var printer = new ToPrinter(Tests.PrintPath);
            var printOptions = new PrintOptions
            {
                Copies = 1,
                Duplex = DuplexMode.Simplex,
                Color = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PageSize.A4
            };

            using (var fileStream = File.OpenRead(Tests.TestFilePath))
            {
                // Act
                int exitCode = await printer.PrintDocumentAsync(fileStream, Tests.TestPrinterName, printOptions);
                
                await Task.Delay(1000); // Wait for the file to be created
                // Assert
                Assert.Equal(0, exitCode);
                Assert.True(await Tests.ExistsAsync(1)); 
            }
        }
    }
}
