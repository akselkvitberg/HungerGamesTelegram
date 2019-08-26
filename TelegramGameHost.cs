using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace HungerGamesTelegram
{
    class TelegramGameHost : INotificator{
        private ITelegramBotClient _botClient;

        private Dictionary<long, TelegramPlayer> Players => _currentGame?.Players.OfType<TelegramPlayer>().ToDictionary(x => x.Id, x => x) ?? new Dictionary<long, TelegramPlayer>();
        readonly List<long> _playersToNotify = new List<long>();

        Game _currentGame;

        private string rules = string.Join("\n",
            new string[]{
                "Hunger Games - Telegram",
                "Målet med spillet er å være siste person som ikke er beseiret.",
                "Hver runde består av to valg: Flytting (nord, syd, ...), og handling (angrip, loot og løp vekk).",
                "Kartet er 12x12 ruter til å begynne med, men reduseres i størrelse helt til det bare er en rute igjen."
                ""
                
            });


        public void Start() {
            var key = File.ReadAllText("botkey.key");
            _botClient = new TelegramBotClient(key);
            _botClient.OnMessage += Bot_OnMessage;
            
            _botClient.StartReceiving();
        }

        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message.From?.FirstName + "> " + e.Message.Text);

            if (e.Message.From?.Id == 49374973)
            {
                if (HandleAdminMessage(e.Message))
                {
                    return;
                }
            }

            if(e.Message.Text == "/regler")
            {
                _botClient.SendTextMessageAsync(e.Message.Chat.Id, rules);
            }

            
            if (Players.ContainsKey(e.Message.Chat.Id))
            {
                Players[e.Message.Chat.Id].ParseMessage(e.Message);
                return;
            }

            switch (e.Message.Text)
            {
                case "/notify":
                    if (_currentGame != null)
                    {
                        _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Spillet er allerede i start-fasen. For å bli med på neste runde, send /join");
                    }
                    else
                    {
                        _playersToNotify.Add(e.Message.Chat.Id);
                        _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Du vil få beskjed før neste runde starter");
                    }
                    break;
                case "/join":
                {
                    if(_currentGame == null)
                    {
                        _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Spillet har ikke startet. For å få beskjed når en ny runde starter, send /notify");
                        return;
                    }
                    if(_currentGame.Started)
                    {
                        // already started, please wait
                        _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Spillet er allerede i gang. Vennligst vent. For å få beskjed når en ny runde starter, send /notify");
                        return;
                    }

                    TelegramPlayer player = new TelegramPlayer(_currentGame, e.Message.Chat.Id, _botClient, $"{e.Message.From?.FirstName} {e.Message.From?.LastName}");
                    _currentGame.Players.Add(player);
                    _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Du er med i neste runde.\nRunden starter snart.");
                    break;
                }
            }
        }

        private bool HandleAdminMessage(Message message)
        {
            switch (message.Text)
            {
                case "/newgame":
                    if (_currentGame == null)
                    {
                        _playersToNotify.Add(message.Chat.Id);
                        _currentGame = new Game(this);
                        foreach (var id in _playersToNotify.ToList())
                        {
                            _botClient.SendTextMessageAsync(id, "En ny runde starter snart. For å bli med, send /join");
                        }
                        _playersToNotify.Clear();
                    }
                    else
                    {
                        _botClient.SendTextMessageAsync(message.Chat.Id, "Stop spillet først. /endgame");
                    }
                    return true;
                case "/start":
                    if (_currentGame == null)
                    {
                        _botClient.SendTextMessageAsync(message.Chat.Id, "Lag nytt spill først. /newgame");
                    }
                    else if (!gameIsStarting)
                    {
                        GameIsStarting();
                        Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(x => _currentGame.StartGame());
                    }

                    return true;
                case "/endgame":
                    _currentGame = null;
                    _botClient.SendTextMessageAsync(message.Chat.Id, "Stoppet. Lag et nytt spill: /newgame");
                    return true;
            }

            return false;
        }

        public void GameAreaIsReduced()
        {
            foreach (var player in Players)
            {
                if(!player.Value.IsDead)
                {
                    _botClient.SendTextMessageAsync(player.Key, "Området er redusert", replyMarkup: new ReplyKeyboardRemove());
                }
            }
        }

        public void GameHasEnded(List<Actor> results)
        {
            string str = "*Resultater*\n\n";
            foreach (var player in results)
            {
                str += $"*{player.Rank}.* {player.Name}\n";
            }
            foreach (var player in results.OfType<TelegramPlayer>())
            {
                _botClient.SendTextMessageAsync(player.Id, str, Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }

            _currentGame = null;
        }
        private bool gameIsStarting = false;

        public void GameIsStarting()
        {
            gameIsStarting = true;
            foreach (var player in Players)
            {
                _botClient.SendTextMessageAsync(player.Key, "Runden starter om *1 minutt!*", Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public void GameHasStarted()
        {
            gameIsStarting = false;
            foreach (var player in Players)
            {
                _botClient.SendTextMessageAsync(player.Key, "*Runden har startet!*", Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public void RoundHasEnded(int round)
        {

        }
    }
}
