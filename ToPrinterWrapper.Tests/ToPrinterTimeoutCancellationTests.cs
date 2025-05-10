namespace ToPrinterWrapper.Tests
{
    public class ToPrinterTimeoutCancellationTests
    {
       [Fact]
        public async Task PrintDocumentAsync_ShouldRespectTimeout()
        {
            await Tests.CleanUpAsync();
            var printer = new ToPrinter(Tests.PrintPath);
            string arguments = $"-src \"{Tests.TestFilePath}\" -prn \"{Tests.TestPrinterName}\" -options alerts:no silent:yes log:no -props spjob:yes";
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var timeout = TimeSpan.FromSeconds(1);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await printer.PrintDocumentAsync(arguments, timeout, CancellationToken.None);
            });
            sw.Stop();
            Assert.InRange(sw.Elapsed.TotalSeconds, 0.5, 3); // Should cancel close to 1s
        }

        [Fact]
        public async Task PrintDocumentAsync_ShouldRespectCancellationToken()
        {
            await Tests.CleanUpAsync();
            var printer = new ToPrinter(Tests.PrintPath);
            string arguments = $"-src \"{Tests.TestFilePath}\" -prn \"{Tests.TestPrinterName}\" -options alerts:no silent:yes log:no -props spjob:yes";
            using var cts = new System.Threading.CancellationTokenSource(1000); // Cancel after 1s
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await printer.PrintDocumentAsync(arguments, cancellationToken: cts.Token);
            });
            sw.Stop();
            Assert.InRange(sw.Elapsed.TotalSeconds, 0.5, 3); // Should cancel close to 1s
        }
    }
}
