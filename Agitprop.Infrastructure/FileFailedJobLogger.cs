using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Agitprop.Infrastructure
{
    public class FileFailedJobLogger : IFailedJobLogger
    {
        private readonly BlockingCollection<string> _failedUrls = new BlockingCollection<string>();
        private readonly Task _loggingTask;
        private readonly string _filePath;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public FileFailedJobLogger(IConfiguration configuration)
        {
            _filePath = configuration.GetValue<string>("FailedJobsLog") ?? throw new MissingConfigurationValueException("Missing FailedJobsLog");

            // Start background task
            _loggingTask = Task.Run(ProcessFailedUrlsAsync);
        }

        public Task LogFailedJobUrlAsync(string url)
        {
            _failedUrls.Add(url);
            return Task.CompletedTask;
        }

        private async Task ProcessFailedUrlsAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_failedUrls.TryTake(out var url, Timeout.Infinite, _cts.Token))
                    {
                        await AppendUrlToFileAsync(url);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when the token is canceled
            }
        }

        private async Task AppendUrlToFileAsync(string url)
        {
            try
            {
                using (var writer = new StreamWriter(_filePath, append: true))
                {
                    await writer.WriteLineAsync(url);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log failed job URL: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _loggingTask.Wait();

            // Flush any remaining URLs
            while (_failedUrls.TryTake(out var url))
            {
                AppendUrlToFileAsync(url).GetAwaiter().GetResult();
            }

            _cts.Dispose();
            _failedUrls.Dispose();
        }
    }
}
