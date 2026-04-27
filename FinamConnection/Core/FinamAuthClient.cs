using System.Text;
using System.Text.Json;

namespace FinamConnection.Core
{
    /// <summary>
    /// Отвечает за авторизацию в Финам API.
    /// Обменивает секретный ключ на JWT токен.
    /// JWT живёт 15 минут — после этого нужно получить новый.
    /// </summary>
    public class FinamAuthClient
    {
        private readonly FinamApiSettings _settings;
        private readonly HttpClient _httpClient;

        private string _jwtToken = string.Empty;
        private DateTime _tokenExpiresAt = DateTime.MinValue;

        public FinamAuthClient(FinamApiSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Возвращает действующий JWT токен.
        /// Если токен истёк или отсутствует — автоматически получает новый.
        /// </summary>
        public async Task<string> GetJwtAsync()
        {
            // Если токен ещё действует — возвращаем его
            if (!string.IsNullOrEmpty(_jwtToken) && DateTime.UtcNow < _tokenExpiresAt)
                return _jwtToken;

            // Иначе — получаем новый
            await RefreshTokenAsync();
            return _jwtToken;
        }

        /// <summary>
        /// Возвращает заголовки для HTTP запросов к Финам API
        /// </summary>
        public async Task<Dictionary<string, string>> GetHeadersAsync()
        {
            var jwt = await GetJwtAsync();
            return new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {jwt}" },
                { "Content-Type", "application/json" }
            };
        }

        /// <summary>
        /// Запрашивает новый JWT токен у Финам API
        /// </summary>
        private async Task RefreshTokenAsync()
        {
            Console.WriteLine("Получаем JWT токен...");

            var requestBody = JsonSerializer.Serialize(new { secret = _settings.SecretKey });
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/v1/sessions",
                content);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка авторизации: {response.StatusCode} — {responseText}");

            // Парсим ответ
            var json = JsonSerializer.Deserialize<JsonElement>(responseText);
            _jwtToken = json.GetProperty("token").GetString()
                ?? throw new Exception("JWT токен отсутствует в ответе");

            // Токен живёт 15 минут — обновляем за 1 минуту до истечения
            _tokenExpiresAt = DateTime.UtcNow.AddMinutes(14);

            Console.WriteLine("JWT токен получен успешно!");
        }
    }
}
