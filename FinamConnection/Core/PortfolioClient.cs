using System.Text.Json;

namespace FinamConnection.Core
{
    /// <summary>
    /// Получает данные портфеля и позиций со счёта Финам.
    /// Эндпоинт: GET /v1/accounts/{account_id}
    /// </summary>
    public class PortfolioClient
    {
        private readonly FinamApiSettings _settings;
        private readonly FinamAuthClient _authClient;
        private readonly HttpClient _httpClient;

        public PortfolioClient(FinamApiSettings settings, FinamAuthClient authClient)
        {
            _settings = settings;
            _authClient = authClient;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Запрашивает данные по счёту: баланс и позиции
        /// </summary>
        public async Task<Portfolio> GetPortfolioAsync()
        {
            Console.WriteLine($"Запрашиваем данные счёта {_settings.AccountId}...");

            var jwt = await _authClient.GetJwtAsync();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

            var url = $"{_settings.BaseUrl}/v1/accounts/{_settings.AccountId}";
            var response = await _httpClient.GetAsync(url);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка: {response.StatusCode} — {responseText}");

            var json = JsonSerializer.Deserialize<JsonElement>(responseText);
            return ParsePortfolio(json);
        }

        /// <summary>
        /// Разбирает JSON ответ под реальную структуру Финам API.
        /// Структура ответа для FORTS счёта:
        /// {
        ///   "account_id", "type" (FORTS/MOEX), "status",
        ///   "equity": {"value": "0.0"},
        ///   "unrealized_profit": {"value": "0.0"},
        ///   "positions": [],
        ///   "cash": [],
        ///   "portfolio_forts": {
        ///     "available_cash": {"value": "0.0"},
        ///     "money_reserved": {"value": "0.0"}
        ///   }
        /// }
        /// </summary>
        private Portfolio ParsePortfolio(JsonElement json)
        {
            var portfolio = new Portfolio
            {
                AccountId = _settings.AccountId,
                Positions = new List<Position>()
            };

            // Тип и статус счёта
            if (json.TryGetProperty("type", out var type))
                portfolio.AccountType = type.GetString() ?? string.Empty;

            if (json.TryGetProperty("status", out var status))
                portfolio.Status = status.GetString() ?? string.Empty;

            // Общая стоимость портфеля (equity)
            if (json.TryGetProperty("equity", out var equity))
                portfolio.Equity = GetDecimalFromValueObject(equity);

            // Нереализованная прибыль/убыток
            if (json.TryGetProperty("unrealized_profit", out var unrealizedProfit))
                portfolio.UnrealizedProfit = GetDecimalFromValueObject(unrealizedProfit);

            // Для FORTS счёта — свободные средства
            if (json.TryGetProperty("portfolio_forts", out var forts))
            {
                if (forts.TryGetProperty("available_cash", out var availCash))
                    portfolio.Cash = GetDecimalFromValueObject(availCash);

                if (forts.TryGetProperty("money_reserved", out var reserved))
                    portfolio.MoneyReserved = GetDecimalFromValueObject(reserved);
            }

            // Позиции
            if (json.TryGetProperty("positions", out var positions))
            {
                foreach (var pos in positions.EnumerateArray())
                {
                    var position = new Position();

                    if (pos.TryGetProperty("symbol", out var symbol))
                        position.Symbol = symbol.GetString() ?? string.Empty;

                    if (pos.TryGetProperty("name", out var name))
                        position.Name = name.GetString() ?? string.Empty;

                    if (pos.TryGetProperty("quantity", out var qty))
                        position.Quantity = GetDecimalFromValueObject(qty);

                    if (pos.TryGetProperty("average_price", out var avgPrice))
                        position.AveragePrice = GetDecimalFromValueObject(avgPrice);

                    if (pos.TryGetProperty("current_price", out var curPrice))
                        position.CurrentPrice = GetDecimalFromValueObject(curPrice);

                    if (pos.TryGetProperty("current_value", out var curValue))
                        position.CurrentValue = GetDecimalFromValueObject(curValue);

                    if (pos.TryGetProperty("unrealized_pnl", out var pnl))
                        position.PnL = GetDecimalFromValueObject(pnl);

                    portfolio.Positions.Add(position);
                    portfolio.PositionsValue += position.CurrentValue;
                }
            }

            return portfolio;
        }

        /// <summary>
        /// Финам присылает числа в виде объекта {"value": "123.45"} — читаем из него decimal
        /// </summary>
        private decimal GetDecimalFromValueObject(JsonElement element)
        {
            // Если это объект с полем "value"
            if (element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty("value", out var valueField))
            {
                return ParseDecimal(valueField);
            }

            // Если это просто число или строка
            return ParseDecimal(element);
        }

        private decimal ParseDecimal(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.String => decimal.TryParse(
                    element.GetString(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var result) ? result : 0,
                _ => 0
            };
        }
    }
}
