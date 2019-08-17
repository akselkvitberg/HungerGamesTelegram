using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace HungerGamesTelegram
{
    class Game {
        public Location[,] Locations { get; set; }
        public List<Actor> Players { get; set; } = new List<Actor>();

        public List<Actor> Results {get;set;} = new List<Actor>();

        public Actor Me {get; set;}

        public async Task StartGame(Actor singlePlayer) {
            
            Locations = LocationFactory.CreateLocations(4);
            
            Me = singlePlayer;
            Players.Add(singlePlayer);

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

            var startLocation = Locations[2,2];
            foreach (var player in Players)
            {
                player.Location = startLocation;
            }

            int roundCount = 0;
            while (Players.Count > 1)
            {
                await DoMovements();
                await DoEncountersAsync();
                Console.WriteLine($"Runde {roundCount++} er over");
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

        private async Task DoMovements()
        {
            foreach (var actor in Players)
            {
                actor.MovePrompt();
            }
            await Task.Delay(5000);
            foreach (var actor in Players)
            {
                actor.Move();
            }
        }

        private async Task DoEncountersAsync()
        {
            var locations = Players.GroupBy(x => x.Location);

            List<Encounter> encounters = new List<Encounter>();

            foreach (var location in locations)
            {
                encounters.AddRange(GetLocationEncounters(location.ToList()));
            }

            foreach (var encounter in encounters)
            {
                encounter.Prompt();
            }
            
            await Task.Delay(5000);

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

        private List<Encounter> GetLocationEncounters(List<Actor> locationPlayers)
        {
            var players = new Stack<Actor>(locationPlayers.OrderBy(x => Guid.NewGuid()).ToList());

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
                encounters.Add(new NonEncounter() { Player1 = players.Pop() });
            }

            return encounters;
        }
    }
    
}
