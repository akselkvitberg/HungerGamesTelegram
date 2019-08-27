using System;
using System.Collections.Generic;
using System.Linq;
using HungerGamesTelegram.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Console;

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
        }

        enum State 
        {
            AskForDirection,
            AskForAction,
            None,
            AskForEvent
        }

        State currentstate = State.None;

        internal void ParseMessage(Message message)
        {
            Logger.Log(this, $" > {message.Text}");

            if(IsDead || !Game.Started)
            {
                return;
            }

            if(currentstate == State.None){
                return;
            }
            if(currentstate == State.AskForDirection){
                var direction = message.Text;
                if(Location.Directions.ContainsKey(direction))
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
            currentstate = State.AskForEvent;

            List<List<KeyboardButton>> optionButtons = new List<List<KeyboardButton>>();
            foreach (var option in options)
            {
                optionButtons.Add(new List<KeyboardButton> {new KeyboardButton(option)});
            }

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(optionButtons);

            Write(keyboard, $"Du er her: *{Location.Name}*", $"Du er level *{Level}*", message);
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
                Write(keyboard, $"Du er her: *{Location.Name}*",
                    Game.GetLocationString(Location),
                    Location.Environment,
                    "*Du må flytte på deg, ellers dør du!*",
                    string.Join("\n", locations),
                    "Hvor vil du gå?");
            }
            else
            {
                Write(keyboard, $"Du er her: *{Location.Name}*",
                    Game.GetLocationString(Location),
                    string.Join("\n", locations),
                    "Hvor vil du gå?");
            }
            
            nextLocation = null;
            currentstate = State.AskForDirection;
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
            base.Loot(player1);
            var item = LootEvent.GetWeightedRandomItem();
            Write($"{player1.Name} stakk av.", $"Du fant {item.name} **(+{item.value+1} lvl)**", $"Du er level *{Level}*");
        }

        public override void RunAway(Actor player2)
        {
            Write($"Du løp vekk fra *{player2.Name}*");

            base.RunAway(player2);
        }

        public override void FailAttack(Actor actor)
        {
            base.FailAttack(actor);
            var item = LootEvent.GetWeightedRandomItem();
            Write($"{actor.Name} løp vekk.", $"Du fant {item.name} **(+{item.value} lvl)**", $"Du er level *{Level}*");
        }

        public override void SuccessAttack(Actor actor)
        {
            base.SuccessAttack(actor);
            var item = LootEvent.GetWeightedRandomItem();
            Write($"Du beseiret *{actor.Name}*.", $"Du fant {item.name} **(+{item.value} lvl)**", $"Du er level *{Level}*");
        }

        public override void Die(Actor actor)
        {
            Write($"*{actor.Name}* (level **{actor.Level}**) beseiret deg.", "*Du er ute av spillet.*");
            base.Die(actor);
        }

        public override void KillZone()
        {
            IsDead = true;
            Write($"Du ble tatt av {Location.Environment}","Du er ute av spillet.");
        }

        public override void Share(Actor actor)
        {
            base.Share(actor);
            Write($"Du og *{actor.Name}* delte på godene **(+1 lvl)**", $"Du er level *{Level}*");
        }
    }
}
