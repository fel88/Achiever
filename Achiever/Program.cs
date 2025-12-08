using System.Linq;
using Achiever.Telegram;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Achiever
{
    public class Program
    {
        static TelegramBotService bot = new TelegramBotService();

        public static void Main(string[] args)
        {
            bot.Run();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
