using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ToPrinterWrapper.Tests
{
    public class ToPrinterParallelTests
    {      
        [Fact]
        public async Task PrintDocumentAsync_Concurrent1_ShouldNotExceedMaxConcurrency()
        {
            await Tests.CleanUpAsync();

            short jobs = 20;
            var printer = new ToPrinter(Tests.PrintPath, jobs);
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
            for (int i = 0; i < jobs; i++)
            {
                var fileStream = File.OpenRead(Tests.TestFilePath);
                tasks.Add(printer.PrintDocumentAsync(fileStream, Tests.TestPrinterName, printOptions));
            }

            await Task.WhenAll(tasks);

            await Task.Delay(jobs * 1000); // Wait for all jobs to finish

            Assert.True(await Tests.ExistsAsync(jobs)); 
        }

         [Fact]
        public async Task PrintDocumentAsync_Concurrent2_ShouldNotExceedMaxConcurrency()
        {
            await Tests.CleanUpAsync();

            short jobs = 20;
            var printer = new ToPrinter(Tests.PrintPath, jobs);
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
            for (int i = 0; i < jobs; i++)
            {
                tasks.Add(printer.PrintDocumentAsync(Tests.TestFilePath, Tests.TestPrinterName, printOptions));
            }
            
            await Task.WhenAll(tasks);

            await Task.Delay(jobs * 1000); // Wait for all jobs to finish

            Assert.True(await Tests.ExistsAsync(jobs)); 
        }
    }
}
