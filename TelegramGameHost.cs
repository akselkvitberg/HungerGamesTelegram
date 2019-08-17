using System;
using System.Collections.Generic;
using System.IO;
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
            Console.WriteLine(e.Message);

            if (players.ContainsKey(e.Message.Chat.Id))
            {
                players[e.Message.Chat.Id].ParseMessage(e.Message);
            }

            if(e.Message.Text == "/join"){
                TelegramPlayer player = new TelegramPlayer(e.Message.Chat.Id, _botClient);
                players.Add(e.Message.Chat.Id, player);
                Game game = new Game();
                game.StartGame(player);
            }
        }
    }
}
