namespace Achiever.Telegram
{
    public class CommandProcessor
    {
        readonly protected ITelegramBotService service;
        public CommandProcessor(ITelegramBotService service)
        {
            this.service = service;
        }
    }
}