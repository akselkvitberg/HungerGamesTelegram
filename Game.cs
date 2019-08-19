using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace HungerGamesTelegram
{
    class Game 
    {
        private readonly INotificator notificator;

        public List<Location> Locations { get; set; }
        public List<Actor> Players { get; set; } = new List<Actor>();

        public List<Actor> Results {get;set;} = new List<Actor>();
        
        public bool Started {get;set;} = false;

        public bool Completed {get;set;} = false;

        public int RoundDelay { get; internal set; } = 10000;

        public int Dimension {get;set;} = 6;

        public Game(INotificator notificator)
        {
            this.notificator = notificator;
        }

        public async Task StartGame() 
        {
            if(Started)
                return;
            Started = true;

            Locations = LocationFactory.CreateLocations(Dimension);
            
            for (int i = 0; i < 15; i++)
                Players.Add(new RandomBot());
            for (int i = 0; i < 10; i++)
                Players.Add(new AttackBot());
            for (int i = 0; i < 10; i++)
                Players.Add(new LootBot());
            for (int i = 0; i < 10; i++)
                Players.Add(new Bot1());
            for (int i = 0; i < 10; i++)
                Players.Add(new Bot2());
            for (int i = 0; i < 15; i++)
                Players.Add(new Bot2());
            // for (int i = 0; i < 20; i++)
            //     Players.Add(new RunBot());

            var startLocation = Locations.First(x=>x.IsStartingPoint);
            foreach (var player in Players)
            {
                player.Location = startLocation;
            }

            notificator.GameHasStarted();
            
            int roundCount = 1;
            while (Players.Count > 1)
            {
                // Round start
                playersThisRound = Players.Count;
                if(Locations.Count(x=>!x.IsDeadly) > 1)
                {
                    await DoMovements();
                }
                KillPlayersInDeadZone();

                await DoEncountersAsync();
                notificator.RoundHasEnded(roundCount++);
                if(roundCount % 3 == 0)
                {
                    await Task.Delay(1000);
                    LimitPlayArea();
                }
                await Task.Delay(2000);
            }

            foreach (var actor in Players.ToList())
            {
                RemovePlayer(actor);
            }

            notificator.GameHasEnded(Results);
            Completed = true;
        }

        private int playersThisRound = 0;
        private void KillPlayersInDeadZone()
        {
            foreach(var player in Players.Where(x=>x.Location.IsDeadly).ToList())
            {
                player.KillZone();
                RemovePlayer(player);
            }
        }

        private void RemovePlayer(Actor player)
        {
            Players.Remove(player);
            Results.Add(player);
            player.Rank = playersThisRound;
            player.Result(playersThisRound);
        }

        private void LimitPlayArea()
        {
            var remainingTiles = Locations.Count(x=>!x.IsDeadly);
            if(remainingTiles == 1){
                return;
            }
            if(remainingTiles <= 4)
            {
                var location = Locations.Where(x=>!x.IsDeadly).OrderBy(x=>Guid.NewGuid()).First();
                location.IsDeadly = true;
                notificator.GameAreaIsReduced();
                return;
            }
            
            foreach (var location in Locations.Where(x=>x.IsDeadly).ToList())
            {
                foreach (var next in location.Directions)
                {
                    next.Value.IsDeadly = true;
                }
            }

            notificator.GameAreaIsReduced();
        }

        private async Task DoMovements()
        {
            foreach (var actor in Players)
            {
                actor.MovePrompt();
            }
            await Task.Delay(RoundDelay);
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
            
            await Task.Delay(RoundDelay);

            foreach (var encounter in encounters)
            {
                encounter.RunEncounter();
                if (encounter.Player1.IsDead)
                {
                    RemovePlayer(encounter.Player1);
                }
                if (encounter.Player2?.IsDead == true)
                {
                    RemovePlayer(encounter.Player2);
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

    interface INotificator
    {
        void GameIsStarting();
         void GameHasStarted();

         void GameHasEnded(List<Actor> results);

         void RoundHasEnded(int round);

         void GameAreaIsReduced();
    }
}
