using System.Text;
using System.Text.Json;
using Agitprop.Infrastructure.Interfaces;

namespace Agitprop.NERClient
{
    class RestClient:ISink
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public RestClient(string baseUrl)
        {
            _client = new HttpClient();
            _baseUrl = baseUrl;
        }

        public async Task<string> PingAsync()
        {
            var response = await _client.GetAsync(_baseUrl + "/ping");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> AnalyzeSingleAsync(object corpus)
        {
            var json = JsonSerializer.Serialize(corpus);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_baseUrl + "/analyzeSingle", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> AnalyzeBatchAsync(object[] corpora)
        {
            var json = JsonSerializer.Serialize(corpora);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(_baseUrl + "/analyzeBatch", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public Task EmitAsync(string url, Dictionary<string, object> data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
