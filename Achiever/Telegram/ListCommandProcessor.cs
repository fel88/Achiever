using Achiever.Common.Model;
using Achiever.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Achiever.Telegram
{
    public class LogCommandProcessor : CommandProcessor, ICommandProcessor
    {
        public LogCommandProcessor(ITelegramBotService service) : base(service)
        {
        }

        public async Task<bool> Process(string message)
        {
            if (!message.ToLower().Trim().StartsWith("log"))
                return false;

            var spl = message.ToLower().Split(new char[] { ' ' }).ToArray();
            int cnt = 10;
            if (spl.Length > 1)
            {
                cnt = int.Parse(spl[1]);
            }
            var context = AchieverContextHolder.GetContext();

            var user = context.Users.First(z => z.TelegramChatId == service.ChatId);
            var tt = context.AchievementValueItems.Where(z => z.User.Id == user.Id).Include(z => z.Achievement).OrderByDescending(z => z.Timestamp).Take(cnt).ToArray();
            var grp = tt.GroupBy(z => z.Timestamp.Date).ToArray();

            foreach (var g in grp)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var item in g)
                {
                    sb.Append($"{item.Timestamp} - {item.Achievement.Name}: " + item.Count);
                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        sb.Append($"  ({item.Description})");
                    }
                    sb.AppendLine();
                }
                await service.Bot.SendTextMessageAsync(chatId: service.ChatId, text: sb.ToString(), cancellationToken: service.CancellationToken);
            }

            return true;
        }
    }
}