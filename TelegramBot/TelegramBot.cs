﻿using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Modules;

namespace TelegramBot
{
    public class TelegramBot
    {
        public static void BootTelegramBot() => new TelegramBot().Main();

        public void Main()
        {
            var config = ConfigurationBuilder();
            var cortana = new TelegramBotClient(config["token"]);
            Console.WriteLine("awqqewqeqweqqq1");
            TelegramData.Init(cortana);
            Console.WriteLine("awqqewqeqweqqq2");
            cortana.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("awqqewqeqweqqq3");
            TelegramData.SendToUser(TelegramData.NameToID("@gwynn7"), "I'm Online", false);
        }

        private Task UpdateHandler(ITelegramBotClient cortana, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    HandleCallback(cortana, update);
                    break;
                case UpdateType.Message:
                    HandleMessage(cortana, update);
                    break;
            }
            return Task.CompletedTask;
        }

        private async void HandleMessage(ITelegramBotClient cortana, Update update)
        {
            if (update.Message == null) return;
            if (update.Message.Type != MessageType.Text || update.Message.Text == null) return;
            
            var messageStats = new MessageStats
            {
                ChatID = update.Message.Chat.Id,
                UserID = update.Message.From?.Id ?? update.Message.Chat.Id,
                MessageID = update.Message.MessageId,
                ChatType = update.Message.Chat.Type,
                FullMessage = update.Message.Text.Substring(1)
            };

            if (messageStats.UserID != TelegramData.NameToID("@gwynn7") && messageStats.ChatType == ChatType.Private) await cortana.ForwardMessageAsync(TelegramData.NameToID("@gwynn7"), messageStats.ChatID, messageStats.MessageID);
            
            if (update.Message.Text.StartsWith('/'))
            {
                messageStats.Command = messageStats.FullMessage.Split(" ").First().Replace("@CortanaAIBot", "");
                messageStats.TextList = messageStats.FullMessage.Split(" ").Skip(1).ToList();
                messageStats.Text = string.Join(" ", messageStats.TextList);

                HardwareModule.ExecCommand(messageStats, cortana);
                UtilityModule.ExecCommand(messageStats, cortana);
                ShoppingModule.ExecCommand(messageStats, cortana);
            }
            else
            {
                if (UtilityModule.IsWaiting(messageStats.ChatID))
                {
                    UtilityModule.HandleCallback(messageStats, cortana);
                    return;
                }
                HardwareModule.HandleCallback(messageStats, cortana);
            }
        }


        private void HandleCallback(ITelegramBotClient cortana, Update update)
        {
            if (update.CallbackQuery == null || update.CallbackQuery.Data == null || update.CallbackQuery.Message == null) return;
            
            HardwareModule.ButtonCallback(cortana, update);
        }

        private Task ErrorHandler(ITelegramBotClient cortana, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Utility.Functions.Log("Telegram", errorMessage);
            return Task.CompletedTask;
        }

        private IConfigurationRoot ConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Data/Telegram/Token.json")
                .Build();
        }
    }
}