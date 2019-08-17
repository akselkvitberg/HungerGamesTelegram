using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace HungerGamesTelegram
{
    class TelegramGameHost {
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
                    currentGame = new Game(new TelegramNotificator(_botClient));
                }
                if(currentGame.Started)
                {
                    // already started, please wait
                    _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Spillet er allerede i gang. Vennligst vent");
                }

                TelegramPlayer player = new TelegramPlayer(currentGame, e.Message.Chat.Id, _botClient);
                players.Add(e.Message.Chat.Id, player);
                currentGame.Players.Add(player);
            }

        }
    }

    internal class TelegramNotificator : INotificator
    {
        private ITelegramBotClient _botClient;

        public TelegramNotificator(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public void GameAreaIsReduced()
        {
            //_botClient.SendTextMessageAsync()
        }

        public void GameHasEnded()
        {
        }

        public void GameHasStarted()
        {
        }

        public void RoundHasEnded(int round)
        {
        }
    }
}
