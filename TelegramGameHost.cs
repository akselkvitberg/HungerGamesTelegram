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

        public void Start() {
            var key = File.ReadAllText("botkey.key");
            _botClient = new TelegramBotClient(key);
            _botClient.OnMessage += Bot_OnMessage;
            //_botClient.OnUpdate += Bot_Update;
            
            _botClient.StartReceiving();
        }

        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message.Text);

            if (players.ContainsKey(e.Message.Chat.Id))
            {
                players[e.Message.Chat.Id].ParseMessage(e.Message);
                return;
            }

            if(e.Message.Text == "/join"){
                Game game = new Game(new TelegramNotificator(_botClient));
                TelegramPlayer player = new TelegramPlayer(game, e.Message.Chat.Id, _botClient);
                players.Add(e.Message.Chat.Id, player);
                player.Died += playerDied;
                game.StartGame(player);
            }
        }

        private void playerDied(TelegramPlayer obj)
        {
            players.Remove(obj.Id);
            obj.Died -= playerDied;
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
