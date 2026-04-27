namespace FinamConnection.Core
{
    /// <summary>
    /// Настройки подключения к Финам API.
    /// Значения читаются из appsettings.local.json
    /// </summary>
    public class FinamApiSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.finam.ru";
        public string AccountId { get; set; } = string.Empty;
    }
}
