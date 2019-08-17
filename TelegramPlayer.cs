using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Console;

namespace HungerGamesTelegram
{

    class TelegramPlayer : Actor
    {
        long Id;
        private readonly ITelegramBotClient _client;

        public TelegramPlayer(long id, ITelegramBotClient client)
        {
            Id = id;
            _client = client;
        }

        enum State {
            AskForDirection,
            AskForAction,
            None,
        }

        State currentstate = State.None;

        internal void ParseMessage(Message message)
        {
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

        public override void EncounterPrompt(Actor actor)
        {
            _client.SendTextMessageAsync(Id,
            $"Du møter på {actor.Name}\n"+
            "Du er level {Level}\n"+
            "Hva vil du gjøre?");
            currentstate = State.AskForAction;
        }

        public override void MovePrompt()
        {
            _client.SendTextMessageAsync(Id,
            $"Du er her: {Location.Name}\n"+
            "Hvor vil du gå?");
            foreach (var location in Location.Directions)
            {
                //WriteLine($"> {location.Key}: {location.Value.Name}");
            }
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
            _client.SendTextMessageAsync(Id,"Du ser ingen rundt deg.");
        }

        public override void Loot()
        {
            _client.SendTextMessageAsync(Id,"Du fant et bedre våpen (+2 lvl)");
            base.Loot();
        }

        public override void RunAway(Actor player2)
        {
            _client.SendTextMessageAsync(Id,$"Du løp vekk fra {player2.Name}");

            base.RunAway(player2);
        }

        public override void FailAttack(Actor actor)
        {
            _client.SendTextMessageAsync(Id,$"{actor.Name} løp vekk.");
            _client.SendTextMessageAsync(Id,$"Du fant et bedre våpen (+1 lvl)");
            base.FailAttack(actor);
        }

        public override void SuccessAttack(Actor actor)
        {
            _client.SendTextMessageAsync(Id,$"Du drepte {actor.Name}.");
            base.SuccessAttack(actor);
        }

        public override void Die(Actor actor)
        {
            _client.SendTextMessageAsync(Id,$"{actor.Name} (lvl {actor.Level}) drepte deg");
            _client.SendTextMessageAsync(Id,$"Du døde");
            base.Die(actor);
        }

        public override void Share(Actor actor)
        {
            _client.SendTextMessageAsync(Id,$"Du og {actor.Name} delte på godene (+1 lvl)");
            base.Share(actor);
        }
    }
}
