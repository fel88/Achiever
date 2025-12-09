using Achiever.Common.Model;
using Achiever.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Achiever.Telegram
{
    public class AddCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public AddCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("add"))
                return false;

            var spl = message.ToLower().Split(new char[] { ' ' }).ToArray();
            var ach = spl[1];
            var cnt = int.Parse(spl[2]);
            StringBuilder sb = new StringBuilder();
            var context = new AchieverContext();
            var user = context.Users.First(z => z.TelegramChatId == service.ChatId);

            //var user = context.Users.First();
            user = context.Users.Find(user.Id);
            foreach (var item in context.AchievementItems)
            {
                var nm = item.Name;
            }
            var ww = context.AchievementItems.ToArray().Where(z => z.Name.ToLower().Contains(ach)).ToArray();
            int? aId = null;
            if (ww.Length == 1)
            {
                aId = ww.First().Id;
            }
            else
            {
                if (ww.Any(z => z.Name.ToLower() == ach))
                {
                    aId = ww.First(z => z.Name.ToLower() == ach).Id;
                }
                else
                {
                    aId = ww.First().Id;
                }
            }

            var a = context.AchievementItems.Single(z => z.Id == aId.Value);
            context.AchievementValueItems.Add(new AchievementValueItem()
            {
                Timestamp = DateTime.Now,
                Count = cnt,
                Achievement = a,
                User = user
            });


            await context.SaveChangesAsync();
            Helper.CheckAllChallenges(user.Id);

            await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text:
                "well done: " + a.Name + ": " + cnt, cancellationToken: service.CancellationToken);

            return true;
        }
    }
}