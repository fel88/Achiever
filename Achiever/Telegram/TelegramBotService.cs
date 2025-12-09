using Achiever.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Achiever.Telegram
{
    public class TelegramBotService : ITelegramBotService
    {
        TelegramBotClient botClient;
        List<ICommandProcessor> Processors = new List<ICommandProcessor>();
        public void LoadConfig()
        {
            var doc = XDocument.Load(configFileName);
            foreach (var item in doc.Descendants("setting"))
            {
                var nm = item.Attribute("name").Value;
                string vl = null;
                if (item.Attribute("value") != null)
                    vl = item.Attribute("value").Value;
                else
                    vl = item.Value;

                switch (nm)
                {
                    case "apiKey":
                        apiKey = vl;
                        break;
                }
            }
        }

        const string configFileName = "config.xml";
        string apiKey;


        public async void Run()
        {
            if (!System.IO.File.Exists(configFileName))
            {
                Console.WriteLine($"{configFileName} not found. You shall not pass!");
                return;
            }

            LoadConfig();
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("apiKey is empty. You shall not pass!");
                return;
            }

            botClient = new TelegramBotClient(apiKey);
            using CancellationTokenSource cts = new();
            CancellationToken = cts.Token;
            Processors.Add(new LagCommandProcessor(this));
            Processors.Add(new AddCommandProcessor(this));
            Processors.Add(new LogCommandProcessor(this));
            Processors.Add(new DeleteCommandProcessor(this));

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            botClient.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, receiverOptions: receiverOptions, cancellationToken: cts.Token);
            var me = await botClient.GetMeAsync();
        }


        public ITelegramBotClient Bot => botClient;
        public CancellationToken CancellationToken { get; set; }

        public long ChatId { get; set; }

        public bool EchoMode = false;
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;
            var chatId = message.Chat.Id;

            AchieverContext ctx = new AchieverContext();
            var ch = ctx.Users.FirstOrDefault(z => z.TelegramChatId == chatId);
            foreach (var cc in ctx.Users)
            {
                var t = cc.TelegramChatId;
            }
            if (ch == null)
            {
                Console.WriteLine("unauthorized access from chatId: " + chatId);
                return;
            }
            ChatId = chatId;

            if (EchoMode)
            {
                Console.WriteLine(messageText);
                return;
            }

            var messageTextOrigin = messageText;
            messageText = messageText.ToLower().Trim();
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            bool handled = false;
            foreach (var item in Processors)
            {
                try
                {
                    if (await item.Process(messageTextOrigin))
                    {
                        handled = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: ex.Message, cancellationToken: cancellationToken);
                }
            }

            if (handled)
                return;

            Message sentMessage = await botClient.SendTextMessageAsync(chatId: chatId, text: "Sorry, melon. Unknown command\n", cancellationToken: cancellationToken);
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}