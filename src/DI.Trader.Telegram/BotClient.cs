using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoTrader.Core;
using CryptoTrader.Core.Queries;
using CryptoTrader.Core.Queries.Common;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace DI.Trader.Telegram
{
    public class BotClient
    {
        private static readonly string[] Commands = {"/system-stop", "/system-restart"};
        private readonly ConcurrentQueue<string> commandQueue;

        private readonly IQueryProcessor queryProcessor;
        private readonly ILogger<BotClient> logger;

        private readonly TelegramBotClient client;
        private readonly long[] chatIds;

        public BotClient(ApiSecrets secrets, ILogger<BotClient> logger, IQueryProcessor queryProcessor)
        {
            commandQueue = new ConcurrentQueue<string>();
            this.logger = logger;
            this.queryProcessor = queryProcessor;
            client = new TelegramBotClient(secrets.TelegramSecrets.BotToken);
            client.OnMessage += ClientOnOnMessage;
            chatIds = secrets.TelegramSecrets.ChatIds;
            client.StartReceiving();
        }

        //todo подумать, как команды процессить
        public IEnumerable<string> GetUserCommands()
        {
            while (commandQueue.TryDequeue(out var str))
                yield return str;
        }

        public Task SendMessage(string message, long chatId) =>
            client.SendTextMessageAsync(chatId, message);

        public async Task SendToAll(string message)
        {
            foreach (var chatId in chatIds)
                await SendMessage(message, chatId);
        }

        private async void ClientOnOnMessage(object? sender, MessageEventArgs e)
        {
            var charId = e.Message.Chat.Id;
            //авторизация на идентификаторе чата
            if(!chatIds.Contains(charId))
                return;

            try
            {
                await OnClientMessageInternal(e, charId);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Telegram error");
                await SendMessage($"Telegram error {exception.Message}", charId);
            }
        }

        private async Task OnClientMessageInternal(MessageEventArgs e, long chatId)
        {
            var message = e.Message.Text;

            if (Commands.Any(command => message.Contains(command)))
            {
                commandQueue.Enqueue(message);
                return;
            }

            await ProcessQueries(message, chatId);
            await SendKeyboard(chatId);
        }

        private async Task ProcessQueries(string message, long chatId)
        {
            var t = message.Split(' ').First().ToLower() switch
            {
                "/trade-states" => GetTradeStates(),
                "/system-state" => GetSystemStates(),
                _ => Task.CompletedTask
            };
            await t;

            async Task GetTradeStates()
            {
                var result = await queryProcessor.Process<GetTradeStatesResult>();
                await SendMessage($"{result}", chatId);
            }

            async Task GetSystemStates()
            {
                await SendMessage("Not Implemented", chatId);
            }
        }

        private async Task SendKeyboard(long chatId)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    //состояние системы
                    new KeyboardButton[] {"/system-state"},
                    //состояние стейтов
                    new KeyboardButton[] {"/trade-states"},
                    //остановить систему
                    new KeyboardButton[] {"/system-stop"},
                    //перезапустить систему
                    new KeyboardButton[] {"/system-restart"}

                },
                true
            );

            await client.SendTextMessageAsync(
                chatId,
                "_>",
                replyMarkup: replyKeyboardMarkup);
        }
    }
}
