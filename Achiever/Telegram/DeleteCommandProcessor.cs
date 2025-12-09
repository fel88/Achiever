using Achiever.Common.Model;
using Achiever.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Achiever.Telegram
{
    public class DeleteCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public DeleteCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("del"))
                return false;

            var spl = message.ToLower().Split(new char[] { ' ' }).ToArray();

            var context = new AchieverContext();
            var user = context.Users.First(z => z.TelegramChatId == service.ChatId);

            //var user = context.Users.First();
            user = context.Users.First(z => z.TelegramChatId == service.ChatId);

            var a = context.AchievementValueItems.OrderByDescending(z => z.Timestamp).First();


            var ch = context.AchievementValueItems.Find(a.Id);
            context.AchievementValueItems.Remove(ch);
            await context.SaveChangesAsync();

            await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text:
                "well done", cancellationToken: service.CancellationToken);

            return true;
        }
    }
}