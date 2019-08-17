using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace HungerGamesTelegram
{
    class Program
    {

        public static Location[] Locations { get; set; } = new Location[12 * 12];
        public static List<Actor> Players { get; set; } = new List<Actor>();

        public static List<Actor> Results {get;set;} = new List<Actor>();

        public static ConsolePlayer Me {get; set;} = new ConsolePlayer();

        static void Main(string[] args)
        {
            WriteLine("Hunger Games");

            for (int i = 0; i < 12*12; i++)
            {
                Location l = new Location()
                {
                    Name = "Location " + i
                };
                Locations[i] = l;
            }

            //Players.Add(Me);

            for (int i = 0; i < 20; i++)
                Players.Add(new RandomBot());
            for (int i = 0; i < 20; i++)
                Players.Add(new AttackBot());
            for (int i = 0; i < 20; i++)
                Players.Add(new LootBot());
            for (int i = 0; i < 20; i++)
                Players.Add(new Bot1());
            for (int i = 0; i < 20; i++)
                Players.Add(new Bot2());
            // Todo: When we have shrinking size, add this
            // for (int i = 0; i < 20; i++)
            //     Players.Add(new RunBot());

            while (Players.Count > 1)
            {
                DoRound();
            }

            for (int i = 0; i < Results.Count; i++)
            {
                WriteLine($"{Results.Count - i}: {Results[i].GetType().Name} ({Results[i].Level})");
            }

            // for (int i = Results.Count; i >= 1; i--)
            // {
            //     WriteLine($"{i}: {Results[i-1].GetType().Name}");
            // }

            // if(!Me.IsDead)
            // {
            //     WriteLine("Du vant!");
            // }
            // else
            //     WriteLine("Du tapte");
            ReadLine();
        }

        private static void DoRound()
        {
            var players = new Stack<Actor>(Players.OrderBy(x => Guid.NewGuid()).ToList());

            List<Encounter> encounters = new List<Encounter>();
            
            while (players.Count > 1)
            {
                var p1 = players.Pop();
                var p2 = players.Pop();
                encounters.Add(new Encounter()
                {
                    Player1 = p1,
                    Player2 = p2
                });
            }

            if (players.Count == 1)
            {
                encounters.Add(new NonEncounter(){Player1 = players.Pop()});
            }

            foreach (var encounter in encounters)
            {
                encounter.RunEncounter();
                if (encounter.Player1.IsDead)
                {
                    Players.Remove(encounter.Player1);
                    Results.Add(encounter.Player1);
                }
                if (encounter.Player2?.IsDead == true)
                {
                    Players.Remove(encounter.Player2);
                    Results.Add(encounter.Player2);
                }
            }
        }
    }

    class Encounter
    {
        public Actor Player1 { get; set; }
        public Actor Player2 { get; set; }

        public virtual void RunEncounter()
        {
            var reply1 = Player1.Encounter(Player2);
            var reply2 = Player2.Encounter(Player1);

            switch ((reply1, reply2))
            {
                case (EncounterReply.Attack, EncounterReply.Attack):
                    ResolveAttack(Player1, Player2);
                    break;
                case (EncounterReply.Loot, EncounterReply.Loot):
                    Player1.Share(Player2);
                    Player2.Share(Player1);
                    break;
                case (EncounterReply.RunAway, EncounterReply.RunAway):
                    Player1.RunAway(Player2);
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.RunAway, EncounterReply.Attack):
                    Player1.RunAway(Player2);
                    Player2.FailAttack(Player1);
                    break;
                case (EncounterReply.Attack, EncounterReply.RunAway):
                    Player1.FailAttack(Player2);
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.RunAway, EncounterReply.Loot):
                    Player1.RunAway(Player2);
                    Player2.Loot();
                    break;
                case (EncounterReply.Loot, EncounterReply.RunAway):
                    Player1.Loot();
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.Attack, EncounterReply.Loot):
                    Player1.SuccessAttack(Player2);
                    Player2.Die(Player1);
                    break;
                case (EncounterReply.Loot, EncounterReply.Attack):
                    Player1.Die(Player2);
                    Player2.SuccessAttack(Player1);
                    break;
            }
        }

        private static Random random = new Random();

        private void ResolveAttack(Actor player1, Actor player2)
        {
            var diff = 0.5 - ((player1.Level - player2.Level) / 10.0);
            
            if(random.NextDouble() > diff)
            {
                player1.SuccessAttack(player2);
                player2.Die(player1);
            }
            else 
            {
                player1.Die(player2);
                player2.SuccessAttack(player1);
            }
        }
    }

    enum EncounterReply
    {
        Attack,
        Loot,
        RunAway
    }

    class NonEncounter : Encounter
    {
        public override void RunEncounter()
        {
            var reply = Player1.NoEncounter();
            if (reply == EncounterReply.Loot)
            {
                Player1.Loot();
            }
        }
    }

    class Location
    {
        public string Name { get; set; }
        public List<Actor> Players { get; } = new List<Actor>();
    }
}
