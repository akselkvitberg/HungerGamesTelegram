using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Console;

namespace HungerGamesTelegram
{

    class TelegramPlayer : Actor
    {
        public Game Game { get; }
        public long Id {get;}
        private readonly ITelegramBotClient _client;

        public TelegramPlayer(Game game, long id, ITelegramBotClient client)
        {
            Game = game;
            Id = id;
            _client = client;
        }

        public event Action<TelegramPlayer> Died;

        enum State {
            AskForDirection,
            AskForAction,
            None,
        }

        State currentstate = State.None;

        internal void ParseMessage(Message message)
        {
            if(IsDead || !Game.Started)
            {
                return;
            }

            if(currentstate == State.None){
                return;
            }
            if(currentstate == State.AskForDirection){
                var direction = message.Text.ToLower();
                if(Location.Directions.ContainsKey(direction))
                {
                    nextLocation = Location.Directions[direction];
                }
            }
            else 
            {
                var answer = message.Text.ToLower();
                switch (answer.ToLower())
                {
                    case "attack":
                        EncounterAction = EncounterReply.Attack;
                        break;
                    case "loot":
                        EncounterAction =  EncounterReply.Loot;
                        break;
                    case "run":
                        EncounterAction =  EncounterReply.RunAway;
                        break;
                }
            }
        }

        public void Write(params string[] message)
        {
            _client.SendTextMessageAsync(Id, string.Join("\n", message), ParseMode.Markdown, true, false, 0, new ReplyKeyboardRemove());
        }

        public void Write(IReplyMarkup markup, params string[] message)
        {
            _client.SendTextMessageAsync(Id, string.Join("\n", message), ParseMode.Markdown, true, false, 0, markup);
        }

        public override void EncounterPrompt(Actor actor)
        {
            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new List<KeyboardButton>(){
                new KeyboardButton("attack"),
                new KeyboardButton("loot"),
                new KeyboardButton("run"),
            },true, true);

            Write(keyboard, $"Du er her: *{Location.Name}*", $"Du møter på *{actor.Name}*", $"Du er level *{Level}*", "Hva vil du gjøre?");

            currentstate = State.AskForAction;
            EncounterAction = EncounterReply.Loot;
        }

        public override void MovePrompt()
        {
            List<KeyboardButton> directions = new List<KeyboardButton>();
            foreach (var location in Location.Directions)
            {
                directions.Add(new KeyboardButton($"{location.Key}"));
            }

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(directions, false, false);

            Write(keyboard, $"Du er her: *{Location.Name}*", "Hvor vil du gå?");
            
            nextLocation = null;
            currentstate = State.AskForDirection;
        }

        Location nextLocation = null;
        public override void Move()
        {
            Move(nextLocation);            
        }
        
        public override void NoEncounterPrompt()
        {
            Write($"Du er her: *{Location.Name}*", "Du ser ingen rundt deg.", "Du leter etter våpen.");
            base.NoEncounterPrompt();
        }

        public override void Loot()
        {
            base.Loot();
            Write("Du fant et bedre våpen **(+2 lvl)**", $"Du er level *{Level}*");
        }

        public override void RunAway(Actor player2)
        {
            Write($"Du løp vekk fra *{player2.Name}*");

            base.RunAway(player2);
        }

        public override void FailAttack(Actor actor)
        {
            base.FailAttack(actor);
            Write($"{actor.Name} løp vekk.", $"Du fant et bedre våpen **(+1 lvl)**", $"Du er level *{Level}*");
        }

        public override void SuccessAttack(Actor actor)
        {
            base.SuccessAttack(actor);
            Write($"Du drepte {actor.Name}.", $"Du fant et bedre våpen **(+1 lvl)**", $"Du er level *{Level}*");
        }

        public override void Die(Actor actor)
        {
            Write($"*{actor.Name}* (level **{actor.Level}**) drepte deg.", "*Spillet er over.*");
            base.Die(actor);
            if(Died != null){
                Died(this);
            }
        }

        public override void Share(Actor actor)
        {
            base.Share(actor);
            Write($"Du og *{actor.Name}* delte på godene **(+1 lvl)**", $"Du er level *{Level}*");
        }
    }
}
