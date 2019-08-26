using System.Linq;
using static System.Console;

namespace HungerGamesTelegram
{
    class ConsolePlayer : Actor
    {
        public ConsolePlayer(Game game) {
            game.RoundDelay = 0;
        }

        public override void EncounterPrompt(Actor actor)
        {
            WriteLine();
            WriteLine($"Du møter på {actor.Name}");
            WriteLine($"Du er level {Level}");
            WriteLine($"Hva vil du gjøre?");
            Write("> ");

            var answer = ReadLine();

            EncounterAction = EncounterReply.Loot;
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

        public override void EventPrompt(string message, string[] options)
        {
            WriteLine(message);
            foreach (var option in options)
            {
                WriteLine(">" + option);
            }

            EventEncounterReply = ReadLine();
        }

        Location nextLocation = null;
        public override void MovePrompt()
        {
            WriteLine($"Du er her: {Location.Name}");
            if(Location.IsDeadly){
                WriteLine("Du er fanget i stormen. Du må flytte deg, ellers dør du!");
            }
            WriteLine("Hvor vil du gå?");


            foreach (var location in Location.Directions.Where(x=>!x.Value.IsDeadly))
            {
                WriteLine($"> {location.Key}: {location.Value.Name}");
            }
            Write("> ");
            var input = ReadLine();

            if(Location.Directions.ContainsKey(input))
            {
                nextLocation = Location.Directions[input];
                //Move(Location.Directions[input]);
            }
            else
            {
                nextLocation = null;
                // stay for now. Implement retry logic.
            }
        }

        public override void Move()
        {
            Move(nextLocation);
        }

        public override void Loot()
        {
            WriteLine("Du fant et bedre våpen (+2 lvl)");
            base.Loot();
        }

        public override void RunAway(Actor player2)
        {
            WriteLine($"Du løp vekk fra {player2.Name}");
            base.RunAway(player2);
        }

        public override void FailAttack(Actor actor)
        {
            WriteLine($"{actor.Name} løp vekk.");
            WriteLine($"Du fant et bedre våpen (+1 lvl)");
            base.FailAttack(actor);
        }

        public override void SuccessAttack(Actor actor)
        {
            WriteLine($"Du vant over {actor.Name}.");
            base.SuccessAttack(actor);
        }

        public override void Die(Actor actor)
        {
            WriteLine($"{actor.Name} (lvl {actor.Level}) vant.");
            WriteLine($"Du er ute av spillet");
            base.Die(actor);
        }

        public override void KillZone() {
            WriteLine("Du ble tatt av stormen.");
            base.KillZone();
        }

        public override void Result(int rank)
        {
            WriteLine($"Du ble #{rank}");
        }

        public override void Message(string message)
        {
            WriteLine(message);
        }

        public override void Share(Actor actor)
        {
            WriteLine($"Du og {actor.Name} delte på godene (+1 lvl)");
            base.Share(actor);
        }
    }
}
