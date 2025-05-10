using System.Threading.Tasks;
using Xunit;

namespace ToPrinterWrapper.Tests
{
    public class ToPrinterStatusTests
    {
        
    
        [Theory]
        [InlineData(Tests.TestNetworkPrinterName)]
        [InlineData(Tests.TestPrinterName)]
        public async Task IsPrinterOnlineAsync_ShouldReturnTrueForOnlinePrinters(string printerName)
        {
            var printer = new ToPrinter(Tests.PrintPath);
            printer.Silent = false;
            bool isOnline = await printer.IsPrinterOnlineAsync(printerName);
            // This will only pass if the printer is actually online and accessible from the test machine
            Assert.True(isOnline, $"Printer '{printerName}' should be online.");
        }

        [Fact]
        public async Task IsPrinterOnlineAsync_ShouldReturnFalseForNonExistentPrinter()
        {
            var printer = new ToPrinter(Tests.PrintPath);
            printer.Silent = false;
            string fakePrinter = "\\\\NONEXISTENT-SERVER\\FakePrinter";
            bool isOnline = await printer.IsPrinterOnlineAsync(fakePrinter);
            Assert.False(isOnline, $"Non-existent printer '{fakePrinter}' should not be online.");
        }
    }
}
