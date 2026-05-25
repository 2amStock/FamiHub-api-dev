using System.Text;
using System.Text.Json;

namespace FamiHub.API.Services
{
    public class GeminiApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiApiService> _logger;

        public GeminiApiService(HttpClient httpClient, IConfiguration config, ILogger<GeminiApiService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Gửi prompt đến Gemini API và nhận response dạng text
        /// </summary>
        public async Task<string?> GenerateContentAsync(string prompt)
        {
            var apiKey = _config["GeminiAI:ApiKey"];
            var model = _config["GeminiAI:Model"] ?? "gemini-2.0-flash";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Gemini API Key chưa được cấu hình trong appsettings.json");
                return null;
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.8,
                    maxOutputTokens = int.Parse(_config["GeminiAI:MaxTokens"] ?? "4096"),
                    responseMimeType = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return null;
                }

                // Parse Gemini response structure
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString();
                    }
                }

                _logger.LogWarning("Gemini API returned unexpected structure: {Body}", responseBody);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return null;
            }
        }
    }
}
