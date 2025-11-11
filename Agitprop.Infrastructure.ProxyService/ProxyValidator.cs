using System.Net;

namespace Agitprop.Infrastructure.ProxyService
{
    public class ProxyValidator : IProxyValidator
    {
        private readonly TimeSpan _timeout;
        private readonly Uri _testUri;

        public ProxyValidator(Uri testUri, TimeSpan timeout)
        {
            _testUri = testUri;
            _timeout = timeout;
        }

        public async Task<bool> ValidateAsync(string proxyAddress, CancellationToken ct = default)
        {
            // simple parse: support http(s) and socks? (for socks you need a socks handler lib)
            using var handler = new HttpClientHandler
            {
                Proxy = new WebProxy(proxyAddress),
                UseProxy = true
            };

            // disable automatic decompression or big downloads
            handler.AutomaticDecompression = DecompressionMethods.None;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_timeout);

            using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan }; // we use cts
            try
            {
                using var resp = await client.GetAsync(_testUri, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                // Consider 200..399 success statuses
                return resp.IsSuccessStatusCode;
            }
            catch (TaskCanceledException) { return false; }
            catch (HttpRequestException) { return false; }
            catch (Exception)
            {
                // log and false
                return false;
            }
        }
    }
}