using System.Threading.Tasks;

namespace ToPrinterWrapper.Tests
{
    public class ToPrinterTests
    {
        private const string PrintPath = @"R:\";
        private const string TestFilePath = "test.pdf";
        private const string TestPrinterName = "Bullzip PdDF Printer";    
        private const string TestOuputFilePath = @"R:\test_printed.pdf"; 

        [Fact]
        public async Task PrintDocumentAsync_ValidOptions_ShouldReturnExitCodeZero()
        {
            // Arrange
            var printer = new ToPrinter(PrintPath);
            var printOptions = new PrintOptions
            {
                Copies = 1,
                Duplex = DuplexMode.Simplex,
                Color = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PageSize.A4
            };

            // Act
            int exitCode = await printer.PrintDocumentAsync(TestFilePath, TestPrinterName, printOptions);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(TestOuputFilePath));

            if (File.Exists(TestOuputFilePath))
            {
                File.Delete(TestOuputFilePath);
            }
        }

        [Fact]
        public async Task PrintDocumentAsync_Stream_ValidOptions_ShouldReturnExitCodeZero()
        {
            // Arrange
            var printer = new ToPrinter(PrintPath);
            var printOptions = new PrintOptions
            {
                Copies = 1,
                Duplex = DuplexMode.Simplex,
                Color = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PageSize.A4
            };

            using (var fileStream = File.OpenRead(TestFilePath))
            {
                // Act
                int exitCode = await printer.PrintDocumentAsync(fileStream, TestPrinterName, printOptions);
                // Assert
                Assert.Equal(0, exitCode);
                Assert.True(File.Exists(TestOuputFilePath));

                if (File.Exists(TestOuputFilePath))
                {
                    File.Delete(TestOuputFilePath);
                }
            }
        }

        [Fact]
        public async Task PrintDocumentAsync_Concurrent1_ShouldNotExceedMaxConcurrency()
        {
            // Arrange
            var printer = new ToPrinter(PrintPath, 20);
            var printOptions = new PrintOptions
            {
                Copies = 1,
                Duplex = DuplexMode.Simplex,
                Color = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PageSize.A4
            };
            var tasks = new List<Task<int>>();

            // Act
            for (int i = 0; i < 20; i++)
            {
                var fileStream = File.OpenRead(TestFilePath);
                tasks.Add(printer.PrintDocumentAsync(fileStream, TestPrinterName, printOptions));
            }

            await Task.WhenAll(tasks);
        }

         [Fact]
        public async Task PrintDocumentAsync_Concurrent2_ShouldNotExceedMaxConcurrency()
        {
            // Arrange
            var printer = new ToPrinter(PrintPath, 20);
            var printOptions = new PrintOptions
            {
                Copies = 1,
                Duplex = DuplexMode.Simplex,
                Color = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PageSize.A4
            };
            var tasks = new List<Task<int>>();

            // Act
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(printer.PrintDocumentAsync("test.pdf", "Microsoft Print to PDF", printOptions));
            }
            await Task.WhenAll(tasks);
        }
    }
}
