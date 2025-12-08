using System.Threading;
using Telegram.Bot;

namespace Achiever.Telegram
{
    public interface ITelegramBotService
    {
        ITelegramBotClient Bot { get; }
        long ChatId { get; }
        CancellationToken CancellationToken { get; }
    }
}