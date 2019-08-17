using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace HungerGamesTelegram
{
    class Program
    {

        public static Location[,] Locations { get; set; }
        public static List<Actor> Players { get; set; } = new List<Actor>();

        public static List<Actor> Results {get;set;} = new List<Actor>();

        public static ConsolePlayer Me {get; set;} = new ConsolePlayer();

        static void Main(string[] args)
        {
            WriteLine("Hunger Games");

            Locations = LocationFactory.CreateLocations(10);
            

            Players.Add(Me);

            // Debug
            Me.Level = 10;

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
                DoMovements();
                DoEncounters();
            }

            for (int i = 0; i < Results.Count; i++)
            {
                WriteLine($"{Results.Count - i}: {Results[i].GetType().Name} ({Results[i].Level})");
            }

            // for (int i = Results.Count; i >= 1; i--)
            // {
            //     WriteLine($"{i}: {Results[i-1].GetType().Name}");
            // }

            if(!Me.IsDead)
            {
                WriteLine("Du vant!");
            }
            else
                WriteLine("Du tapte");
            ReadLine();
        }

        private static void DoMovements()
        {
            foreach (var actor in Players)
            {
                actor.Move();
            }
        }

        private static void DoEncounters()
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

    
    
}
