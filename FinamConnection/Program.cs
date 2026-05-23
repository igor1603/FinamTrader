using FinamConnection.Core;
using Microsoft.Extensions.Configuration;

namespace FinamConnection
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // --- КОНФИГУРАЦИЯ ---
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            var settings = new FinamApiSettings();
            config.GetSection("FinamApi").Bind(settings);

            if (string.IsNullOrEmpty(settings.SecretKey))
            {
                Console.WriteLine("Ошибка: SecretKey не задан в appsettings.local.json");
                return;
            }

            // --- АВТОРИЗАЦИЯ ---
            var authClient = new FinamAuthClient(settings);

            try
            {
                await authClient.GetJwtAsync();
                Console.WriteLine($"Авторизация успешна. Счёт: {settings.AccountId}");
                Console.WriteLine(new string('-', 50));

                // --- ПОРТФЕЛЬ ---
                var portfolioClient = new PortfolioClient(settings, authClient);
                var portfolio = await portfolioClient.GetPortfolioAsync();

                Console.WriteLine("\n=== ПОРТФЕЛЬ ===");
                Console.WriteLine($"Счёт:                {portfolio.AccountId}");
                Console.WriteLine($"Тип:                 {portfolio.AccountType}");
                Console.WriteLine($"Статус:              {portfolio.Status}");
                Console.WriteLine($"Стоимость портфеля:  {portfolio.Equity:N2} руб.");
                Console.WriteLine($"Свободные средства:  {portfolio.Cash:N2} руб.");
                Console.WriteLine($"Зарезервировано:     {portfolio.MoneyReserved:N2} руб.");
                Console.WriteLine($"Нереализованный П/У: {portfolio.UnrealizedProfit:N2} руб.");

                if (portfolio.Positions.Count > 0)
                {
                    Console.WriteLine("\n=== ПОЗИЦИИ ===");
                    foreach (var pos in portfolio.Positions)
                    {
                        Console.WriteLine($"\n{pos.Symbol} — {pos.Name}");
                        Console.WriteLine($"  Количество:    {pos.Quantity}");
                        Console.WriteLine($"  Средняя цена:  {pos.AveragePrice:N2}");
                        Console.WriteLine($"  Текущая цена:  {pos.CurrentPrice:N2}");
                        Console.WriteLine($"  Стоимость:     {pos.CurrentValue:N2} руб.");
                        Console.WriteLine($"  П/У:           {pos.PnL:N2} руб. ({pos.PnLPercent:N2}%)");
                    }
                }
                else
                {
                    Console.WriteLine("\nПозиций нет.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }

            Console.WriteLine("\nНажми любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
