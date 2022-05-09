using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LindormStudio
{
    public class LindormClient : IDisposable
    {
        private const string ApiPath = "/api/v2/sql";
        private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;

        public LindormClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async IAsyncEnumerable<string> ShowDatabasesAsync(string url, string username, string password)
        {
            var resp = await ExecuteSqlAsync(url, username, password, "show databases");
            foreach (var row in resp!.Rows)
            {
                yield return row[0].ToString()!;
            }
        }

        public async Task<LindormResponse?> ExecuteSqlAsync(string url, string username, string password, string sql, string? database = null)
        {
            url += ApiPath;
            if (!string.IsNullOrWhiteSpace(database))
            {
                url += $"?database={database}";
            }
            var content = new StringContent(sql);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            var resp = await _httpClient.PostAsync(url, content);
            if (resp.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<LindormResponse>(await resp.Content.ReadAsStreamAsync(), jsonSerializerOptions);
            }
            else
            {
                throw await JsonSerializer.DeserializeAsync<LindormExecuteSqlException>(await resp.Content.ReadAsStreamAsync(), jsonSerializerOptions) ?? new Exception("execute sql unknow error.");
            }
        }

        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
