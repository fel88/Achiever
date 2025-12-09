using Achiever.Common.Model;
using Achiever.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Achiever.Telegram
{
    public class LagCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public LagCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("lag"))
                return false;

            var spl = message.ToLower().Split(new char[] { ' ' }).ToArray();
            var ach = spl[1];
            var target = int.Parse(spl[2]);

            StringBuilder sb = new StringBuilder();
            var context = new AchieverContext();

            var user = context.Users.First(z => z.TelegramChatId == service.ChatId);

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

            var aa = context.AchievementValueItems.Where(z => z.User.Id == user.Id && z.Achievement.Id == aId.Value);

            var todays = aa.Where(z => z.Timestamp.Date.Month == DateTime.UtcNow.Date.Month && z.Timestamp.Date.Year == DateTime.UtcNow.Date.Year);
            var cnt11 = todays.Sum(z => z.Count);

            var a = context.AchievementItems.Single(z => z.Id == aId.Value);

            var required = (int)Math.Ceiling(target / (float)DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            var diff = cnt11 - DateTime.Now.Day * required;

            await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text:
              a.Name + ". " + (diff < 0 ? "отставание: " : "опережение: ") + diff, cancellationToken: service.CancellationToken);

            return true;
        }
    }
}