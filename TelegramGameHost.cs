using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace HungerGamesTelegram
{
    class TelegramGameHost : INotificator{
        private ITelegramBotClient _botClient;

        Dictionary<long, TelegramPlayer> players = new Dictionary<long, TelegramPlayer>();

        Game currentGame;

        public void Start() {
            var key = File.ReadAllText("botkey.key");
            _botClient = new TelegramBotClient(key);
            _botClient.OnMessage += Bot_OnMessage;
            
            _botClient.StartReceiving();
        }

        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message.From?.FirstName + "> " + e.Message.Text);


            if (e.Message.Text == "/start") {
                currentGame?.StartGame();
            }
            
            if (players.ContainsKey(e.Message.Chat.Id))
            {
                players[e.Message.Chat.Id].ParseMessage(e.Message);
                return;
            }

            if(e.Message.Text == "/join")
            {
                if(currentGame == null){
                    currentGame = new Game(this);
                }
                if(currentGame.Started)
                {
                    // already started, please wait
                    _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Spillet er allerede i gang. Vennligst vent");
                }

                TelegramPlayer player = new TelegramPlayer(currentGame, e.Message.Chat.Id, _botClient, $"{e.Message.From?.FirstName} {e.Message.From?.LastName}");
                players.Add(e.Message.Chat.Id, player);
                currentGame.Players.Add(player);
            }
        }

        public void GameAreaIsReduced()
        {
            foreach (var player in players)
            {
                if(!player.Value.IsDead){
                    _botClient.SendTextMessageAsync(player.Value.Id, "Området er redusert", replyMarkup: new ReplyKeyboardRemove());
                }
            }
        }

        public void GameHasEnded(List<Actor> results)
        {
            string str = "*Resultater*\n===\n---\n";
            for (int i = 0; i < results.Count; i++)
            {
                str += $"*{results.Count -1}.* {results[i].Name}";
            }
            foreach (var player in players)
            {
                _botClient.SendTextMessageAsync(player.Value.Id, str, Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public void GameHasStarted()
        {
            foreach (var player in players)
            {
                _botClient.SendTextMessageAsync(player.Value.Id, "Runden har startet!\n3... 2... 1... GO", Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public void RoundHasEnded(int round)
        {
        }
    }
}
