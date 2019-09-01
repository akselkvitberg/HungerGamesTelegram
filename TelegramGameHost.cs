using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace HungerGamesTelegram
{
    public class TelegramGameHost : INotificator
    {
        private ITelegramBotClient _botClient;

        private Dictionary<long, TelegramPlayer> Players => _currentGame?.Players.OfType<TelegramPlayer>().ToDictionary(x => x.Id, x => x) ?? new Dictionary<long, TelegramPlayer>();
        readonly List<long> _playersToNotify = new List<long>();

        private long adminId = 49374973;

        Game _currentGame;

        private string rules = string.Join("\n",
            "*Hunger Games - Telegram*",
            "Målet med spillet er å være *siste person som ikke er beseiret*.",
            "Hver runde består av to valg: *Flytting* (nord, syd, ...), og *handling* (angrip, løp vekk, osv...)",
            "Du har *3 minutter* på deg på å gjøre hvert handling.",
            "Kartet er 12x12 ruter til å begynne med, men *reduseres i størrelse* helt til det bare er *en rute igjen*.",
            "Er du inne i en 'død' sone etter å ha gjennomført *Flytting* er du ute av spillet.",
            "",
            "Dersom du møter på en annen person kan du velge å *angripe, være kompis, eller løpe vekk.*",
            "Hvis begge to angriper er det *en som vinner*, og *en som taper*. Den som taper er *ute av spillet.*",
            "*Levelen* din avgjør hvor *sannsynlig* det er at du vinner over en motspiller. Har dere lik level er det *50% sjangs for å vinne*.",
            "Hvis en *angriper*, og den andre er *kompis* vinner den som angriper, *uansett*",
            "Hvis begge er kompis så går begge opp i level.",
            //"Hvis du løper vekk får du *ingen ting*, men du er sikker på å overleve runden.",
            "Hvis du løper vekk har du en prosentvis sjangse til å overleve, og du får ingen ting",
            "Og hvis du er kompis, men den andre løper vekk får du to ting.",
            "Du går opp i level ved å angripe og vinne, ved å være kompis, og ved å finne våpen or andre ting rundt omkring på øya.",
            "",
            "Bli med i telegramgruppa for botten: https://t.me/joinchat/AvFm_RXBScXCKgz78UKmGg");

        private string _welcomeMessage = string.Join("\n",
            "Hunger Games - Telegram",
            "",
            "For å se reglene, send /regler",
            "For å bli med på neste runde, send /join",
            "Hvis spillet ikke har startet kan du sende /notify for å få beskjed om når det starter",
            "",
            "Bli med i telegramgruppa for botten: https://t.me/joinchat/AvFm_RXBScXCKgz78UKmGg"
            );


        public void Start() {
            var key = File.ReadAllText("botkey.key");
            _botClient = new TelegramBotClient(key);
            _botClient.OnMessage += Bot_OnMessage;
            
            _botClient.StartReceiving();
        }

        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                BotMessage(e);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        private void BotMessage(MessageEventArgs e)
        {
            Console.WriteLine(e.Message.From?.FirstName + "> " + e.Message.Text);

            if (e.Message.From?.Id == adminId)
            {
                if (HandleAdminMessage(e.Message))
                {
                    return;
                }
            }

            if (e.Message.Text == "/start")
            {
                _botClient.SendTextMessageAsync(e.Message.Chat.Id, _welcomeMessage);
            }

            if(e.Message.Text == "/regler")
            {
                _botClient.SendTextMessageAsync(e.Message.Chat.Id, rules, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }

            if(e.Message.Text == "/status")
            {
                GetStatus(e.Message.Chat.Id);
            }

            
            if (Players.ContainsKey(e.Message.Chat.Id))
            {
                Players[e.Message.Chat.Id].ParseMessage(e.Message);
                return;
            }

            switch (e.Message.Text)
            {
                case "/notify":
                    if (_currentGame?.Started == false)
                    {
                        _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Spillet er allerede i påmeldings-fasen. For å bli med, send /join");
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
                    foreach (var actor in _currentGame.Players.OfType<TelegramPlayer>())
                    {
                        _botClient.SendTextMessageAsync(actor.Id, $"{player.Name} ble med.\n{_currentGame.Players.Count} spillere");
                    }
                    Logger.Log(_currentGame, $"{player.Name} ble med.\n{_currentGame.Players.Count} spillere");
                    break;
                }
            }
        }

        private void GetStatus(long chatId)
        {
            TelegramPlayer player = null;
            if (Players.ContainsKey(chatId))
            {
                player = Players[chatId];
            }

            var status = string.Join("\n",
                $"Aktive spillere: *{_currentGame.PlayersThisRound}*",
                $"Din level: *{player?.Level ?? 0}*",
                $"Du er her: *{player.Location.Name}*",
                _currentGame.GetLocationString(player.Location));
            player.Message(status);
        }

        private bool HandleAdminMessage(Message message)
        {
            var part = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (part[0])
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
                        _botClient.SendTextMessageAsync(message.Chat.Id, "Start spillet med /start");

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
                        foreach (var player in Players)
                        {
                            _botClient.SendTextMessageAsync(player.Key, rules, ParseMode.Markdown);
                        }
                        Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(x => _currentGame.StartGame());
                    }

                    return true;
                case "/endgame":
                    foreach (var currentGamePlayer in _currentGame.Players)
                    {
                        currentGamePlayer.IsDead = true;
                    }
                    _currentGame = null;
                    _botClient.SendTextMessageAsync(message.Chat.Id, "Stoppet. Lag et nytt spill: /newgame");
                    return true;
                case "/roundtime":
                    if (part.Length >= 2)
                    {
                        if (int.TryParse(part[1], out var time) && time > 5)
                        {
                            _currentGame.RoundDelay = TimeSpan.FromSeconds(time);
                            _botClient.SendTextMessageAsync(adminId, $"Rundetid er: {_currentGame.RoundDelay.TotalSeconds}s");
                        }
                        _botClient.SendTextMessageAsync(adminId, $"Kunne ikke parse tid");

                    }
                    else
                    {
                        _botClient.SendTextMessageAsync(adminId, $"Rundetid er: {_currentGame.RoundDelay.TotalSeconds}s");
                    }
                    return true;
                case "/say":
                    var text = string.Join(" ", part.Skip(1));
                    foreach (var telegramPlayer in Players)
                    {
                        telegramPlayer.Value.Message(text);
                    }

                    return true;
                case "/dimension":
                    if (_currentGame.Started)
                    {
                        _botClient.SendTextMessageAsync(adminId, "Kan ikke endre størrelse. Spillet er allerede i gang.");
                    }
                    if (part.Length >= 2)
                    {
                        if (int.TryParse(part[1], out var dimension) && dimension > 4)
                        {
                            _currentGame.Dimension = dimension;
                            _botClient.SendTextMessageAsync(adminId, $"Dimensjon er: {_currentGame.Dimension}x{_currentGame.Dimension}");
                        }
                        _botClient.SendTextMessageAsync(adminId, $"Kunne ikke parse dimensjon");
                    }
                    else
                    {
                        _botClient.SendTextMessageAsync(adminId, $"Dimensjon er: {_currentGame.Dimension}x{_currentGame.Dimension}");
                    }
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
