using static System.Console;

namespace HungerGamesTelegram
{
    class ConsolePlayer : Actor
    {
        public override EncounterReply Encounter(Actor actor)
        {
            WriteLine();
            WriteLine($"Du møter på {actor.Name}");
            WriteLine($"Du er level {Level}");
            WriteLine($"Hva vil du gjøre?");
            Write("> ");

            var answer = ReadLine();

            switch (answer.ToLower())
            {
                case "attack":
                    return EncounterReply.Attack;
                case "loot":
                    return EncounterReply.Loot;
                case "run":
                    return EncounterReply.RunAway;
            }

            return EncounterReply.Loot;
        }

        public override void Move()
        {
            WriteLine($"Du er her: {Location.Name}");
            WriteLine("Hvor vil du gå?");


            foreach (var location in Location.Directions)
            {
                WriteLine($"> {location.Key}: {location.Value.Name}");
            }
            Write("> ");
            var input = ReadLine();

            if(Location.Directions.ContainsKey(input))
            {
                Move(Location.Directions[input]);
            }
            else
            {
                // stay for now. Implement retry logic.
            }


            
        }
        public override EncounterReply NoEncounter()
        {
            WriteLine("Du ser ingen rundt deg.");
            return EncounterReply.Loot;
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
            WriteLine($"Du drepte {actor.Name}.");
            base.SuccessAttack(actor);
        }

        public override void Die(Actor actor)
        {
            WriteLine($"{actor.Name} (lvl {actor.Level}) drepte deg");
            WriteLine($"Du døde");
            base.Die(actor);
        }

        public override void Share(Actor actor)
        {
            WriteLine($"Du og {actor.Name} delte på godene (+1 lvl)");
            base.Share(actor);
        }
    }
}
