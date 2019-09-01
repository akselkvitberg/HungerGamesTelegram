using System.Collections.Generic;
using HungerGamesTelegram.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HungerGamesTelegram
{

    public class TelegramPlayer : Actor
    {
        public Game Game { get; }
        public long Id {get;}
        private readonly ITelegramBotClient _client;

        public TelegramPlayer(Game game, long id, ITelegramBotClient client, string name)
        {
            Game = game;
            Id = id;
            _client = client;
            Name = name;

            Level = 1;
        }

        enum State 
        {
            AskForDirection,
            None,
            AskForEvent
        }

        private State _currentState = State.None;

        internal void ParseMessage(Message message)
        {
            Logger.Log(this, $" > {message.Text}");

            if(IsDead || !Game.Started)
            {
                return;
            }

            if(_currentState == State.None){
                return;
            }
            if(_currentState == State.AskForDirection){
                var direction = message.Text;
                
                if(direction != null && Location.Directions.ContainsKey(direction))
                {
                    nextLocation = Location.Directions[direction];
                }
            }
            else
            {
                EventEncounterReply = message.Text ?? "";
            }
        }

        public void Write(params string[] message)
        {
            Logger.Log(this, string.Join("\n", message));
            _client.SendTextMessageAsync(Id, string.Join("\n", message), ParseMode.Markdown, true, false, 0, new ReplyKeyboardRemove());
        }

        public void Write(IReplyMarkup markup, params string[] message)
        {
            Logger.Log(this, string.Join("\n", message));
            _client.SendTextMessageAsync(Id, string.Join("\n", message), ParseMode.Markdown, true, false, 0, markup);
        }

        public override void EventPrompt(string message, string[] options)
        {
            _currentState = State.AskForEvent;

            List<List<KeyboardButton>> optionButtons = new List<List<KeyboardButton>>();
            foreach (var option in options)
            {
                optionButtons.Add(new List<KeyboardButton> {new KeyboardButton(option)});
            }

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(optionButtons);

            Write(keyboard, $"Du er ved *{Location.Name}*", $"Du er level *{Level}*", message);
        }

        public override void MovePrompt()
        {
            List<string> locations = new List<string>();
            foreach (var location in Location.Directions)
            {
                locations.Add($"*{location.Key}*: {location.Value.Name}" + (location.Value.IsDeadly ? $" ({location.Value.Environment})" : ""));
            }


            var north = Location.Directions.ContainsKey("Nord") && !Location.Directions["Nord"].IsDeadly;
            var west = Location.Directions.ContainsKey("Vest") && !Location.Directions["Vest"].IsDeadly;
            var east = Location.Directions.ContainsKey("Øst") && !Location.Directions["Øst"].IsDeadly;
            var south = Location.Directions.ContainsKey("Sør") && !Location.Directions["Sør"].IsDeadly;
            var directions = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>()
                {
                    new KeyboardButton(" "),
                    new KeyboardButton(north ? "Nord" : " "),
                    new KeyboardButton(" "),
                },
                new List<KeyboardButton>()
                {
                    new KeyboardButton(west ? "Vest" : " "),
                    new KeyboardButton("Bli her"),
                    new KeyboardButton(east ? "Øst" : " "),
                },
                new List<KeyboardButton>()
                {
                    new KeyboardButton(" "),
                    new KeyboardButton(south ? "Sør" : " "),
                    new KeyboardButton(" "),
                }
            };

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(directions, false, false);
            

            if (Location.IsDeadly)
            {
                Write(keyboard,
                    $"Du er ved *{Location.Name}*",
                    Game.GetLocationString(Location),
                    Location.Environment,
                    "*Du må flytte på deg, ellers dør du!*",
                    string.Join("\n", locations),
                    "Hvor vil du gå?");
            }
            else
            {
                Write(keyboard, $"Du er ved *{Location.Name}*",
                    Game.GetLocationString(Location),
                    string.Join("\n", locations),
                    "Hvor vil du gå?");
            }
            
            nextLocation = null;
            _currentState = State.AskForDirection;
        }

        Location nextLocation;
        public override void Move()
        {
            Move(nextLocation);
        }

        public override void Result(int rank)
        {
            Write($"Du ble *#{rank}!*");
        }

        public override void Message(params string[] message)
        {
            Write(string.Join("\n", message));
        }

        public override void Loot(Actor player1)
        {
            var item1 = LootEvent.GetWeightedRandomItem();
            var item2 = LootEvent.GetWeightedRandomItem();
            Level += item1.value + item2.value;
            Write($"{player1.Name} ville ikke være kompis og stakk av.", $"Du fant {item1.name} **({(item1.value+1):+0;-#} level)** og {item2.name} **({(item2.value + 1):+0;-#} level)** istedet.", $"Du er level *{Level}*");
        }

        public override void RunAway(Actor player2)
        {
            Write($"Du løp vekk fra *{player2.Name}*");
        }

        public override void FailAttack(Actor actor)
        {
            var item = LootEvent.GetWeightedRandomItem();
            Level += item.value;
            Write($"{actor.Name} løp vekk.", $"Du fant {item.name} **({item.value:+0;-#} level)**", $"Du er level *{Level}*");
        }

        public override void SuccessAttack(Actor actor)
        {
            var item = LootEvent.GetWeightedRandomItem();
            Level += item.value + 2;
            Write($"Du beseiret *{actor.Name}* (+2 level).", $"Du fant {item.name} **({item.value:+0;-#} level)**", $"Du er level *{Level}*");
        }

        public override void Die(Actor actor)
        {
            Write($"*{actor.Name}* beseiret deg.", "*Du er ute av spillet.*");
            base.Die(actor);
        }

        public override void KillZone()
        {
            IsDead = true;
            Write($"Du ble tatt av {Location.Environment}","Du er ute av spillet.");
        }
    }
}
