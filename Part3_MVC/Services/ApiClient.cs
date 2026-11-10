using System.Net.Http.Json;
using System.Text.Json;

namespace POE_P3_WEB.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            var baseUrl = configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("ApiSettings:BaseUrl is missing from configuration.");

            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            _httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, endpoint);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await ReadErrorAsync(response), null, response.StatusCode);
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Post, endpoint);
            request.Content = JsonContent.Create(data, options: _jsonOptions);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Put, endpoint);
            request.Content = JsonContent.Create(data, options: _jsonOptions);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Delete, endpoint);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
        {
            var request = new HttpRequestMessage(method, endpoint);
            var token = _httpContextAccessor.HttpContext?.Session.GetString("ApiToken");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        public static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(body)
                ? $"The API returned {(int)response.StatusCode} ({response.ReasonPhrase})."
                : body;
        }

        public static string? ReadStringProperty(JsonElement element, params string[] names)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (names.Any(n => string.Equals(n, property.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                            return property.Value.GetString();
                        if (property.Value.ValueKind == JsonValueKind.Number)
                            return property.Value.ToString();
                    }
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        var nested = ReadStringProperty(property.Value, names);
                        if (!string.IsNullOrWhiteSpace(nested)) return nested;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = ReadStringProperty(item, names);
                    if (!string.IsNullOrWhiteSpace(nested)) return nested;
                }
            }
            return null;
        }
    }
}
