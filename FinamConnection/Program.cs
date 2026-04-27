using FinamConnection.Core;
using Microsoft.Extensions.Configuration;

namespace FinamConnection
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Читаем настройки из appsettings.local.json
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

            // Проверяем подключение
            var authClient = new FinamAuthClient(settings);

            try
            {
                var jwt = await authClient.GetJwtAsync();
                Console.WriteLine($"Подключение успешно!");
                Console.WriteLine($"Счёт: {settings.AccountId}");
                Console.WriteLine($"JWT: {jwt[..20]}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажми любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
