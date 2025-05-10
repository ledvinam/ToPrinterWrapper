using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ToPrinterWrapper
{
    /// <summary>
    /// Background file deleter that safely queues files for deletion and retries if locked.
    /// </summary>
    public class FileDeleteQueue : IDisposable
    {
        private readonly BlockingCollection<string> _fileQueue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;
        private readonly int _maxRetries = 5;

        public FileDeleteQueue()
        {
            _worker = Task.Run(ProcessQueueAsync);
        }

        public void Enqueue(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _fileQueue.Add(filePath);
            }
        }

        private async Task ProcessQueueAsync()
        {
            var retryCounts = new ConcurrentDictionary<string, int>();
            while (!_cts.IsCancellationRequested)
            {
                string file;
                try
                {
                    file = _fileQueue.Take(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                // file is guaranteed non-null by BlockingCollection
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    retryCounts.TryRemove(file, out _);
                }
                catch (IOException)
                {
                    int count = retryCounts.AddOrUpdate(file, 1, (_, c) => c + 1);
                    if (count < _maxRetries)
                    {
                        await Task.Delay(1000, _cts.Token);
                        _fileQueue.Add(file);
                    }
                    else
                    {
                        retryCounts.TryRemove(file, out _);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    int count = retryCounts.AddOrUpdate(file, 1, (_, c) => c + 1);
                    if (count < _maxRetries)
                    {
                        await Task.Delay(1000, _cts.Token);
                        _fileQueue.Add(file);
                    }
                    else
                    {
                        retryCounts.TryRemove(file, out _);
                    }
                }
                catch
                {
                    retryCounts.TryRemove(file, out _);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _fileQueue.CompleteAdding();
            try { _worker.Wait(); } catch { }
            _cts.Dispose();
            _fileQueue.Dispose();
        }
    }
}
