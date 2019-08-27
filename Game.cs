using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace HungerGamesTelegram
{
    public class Game 
    {
        public string Name { get; set; }

        private INotificator Notificator { get; }

        public List<Location> Locations { get; set; }
        public List<Actor> Players { get; set; } = new List<Actor>();

        public List<Actor> Results {get;set;} = new List<Actor>();
        
        public bool Started {get;set;} = false;

        public bool Completed {get;set;} = false;

        public TimeSpan RoundDelay { get; internal set; } = TimeSpan.FromSeconds(15);

        public int Dimension {get;set;} = 6;

        private int _playersThisRound = 0;

        public Game(INotificator notificator)
        {
            Notificator = notificator;
            Name = DateTime.Now.ToString("yyyy MMMM dd hhmm");
            Logger.Log(this, "Game created");
        }

        public async Task StartGame() 
        {
            if(Started)
                return;
            Started = true;

            Logger.Log(this, $"Game started: {Dimension}x{Dimension} grid. {Players.Count} players:");

            Locations = LocationFactory.CreateLocations(Dimension);

            for (int i = 0; i < 20; i++)
                Players.Add(new RandomBot());

            var startLocation = Locations.First(x=>x.IsStartingPoint);
            foreach (var player in Players)
            {
                Logger.Log(player, $"Player starting at {startLocation.Name}");
                player.Location = startLocation;
            }

            Notificator.GameHasStarted();

            int roundCount = 0;
            while (Players.Count > 1)
            {
                // Round start
                _playersThisRound = Players.Count;
                roundCount++;

                Logger.Log(this, $"Runde {roundCount}, {Players.Count} spillere");

                // Movement
                if(Locations.Count(x=>!x.IsDeadly) > 1)
                {
                    await DoMovements();
                }

                // Kill players that are still in the storm - they either moved into the storm, or stood still
                KillPlayersInDeadZone();

                // Run encounters
                await DoEncountersAsync();

                // Round end
                Notificator.RoundHasEnded(roundCount);

                // Limit play area every 3 rounds
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
            
            Notificator.GameHasEnded(Results);

            Logger.Log(this, $"Spillet er over.\n\nResultater:");
            Logger.Log(this, string.Join("\n ", Results.Select(x=>$"{x.Rank}. {x.Name}")));

            Completed = true;
        }

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
            player.Rank = _playersThisRound;
            player.Result(_playersThisRound);
        }

        private void LimitPlayArea()
        {
            var remainingTiles = Locations.Count(x=>!x.IsDeadly);
            if(remainingTiles == 1){
                return;
            }
            if(remainingTiles == 4 || remainingTiles == 2)
            {
                var location = Locations.Where(x=>!x.IsDeadly).OrderBy(x=>Guid.NewGuid()).First();
                location.IsDeadly = true;
                location.Environment = location.Directions.FirstOrDefault(x => x.Value.IsDeadly).Value.Environment;
                Notificator.GameAreaIsReduced();
                return;
            }

            if (remainingTiles == 3)
            {
                var location = Locations.Where(x => !x.IsDeadly).FirstOrDefault(x=>x.Directions.Count(y=>y.Value.IsDeadly) == 3);

                location.IsDeadly = true;
                location.Environment = location.Directions.FirstOrDefault(x => x.Value.IsDeadly).Value.Environment;
                Notificator.GameAreaIsReduced();
                return;
            }

            foreach (var location in Locations.Where(x=>x.IsDeadly).ToList())
            {
                foreach (var next in location.Directions)
                {
                    next.Value.IsDeadly = true;
                    next.Value.Environment = location.Environment;
                }
            }

            Notificator.GameAreaIsReduced();
            Logger.Log(this, "Området er redusert");
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

            List<IEncounter> encounters = new List<IEncounter>();

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
                foreach (var player in encounter.GetDeadPlayers())
                {
                    RemovePlayer(player);
                }
            }
        }

        private List<IEncounter> GetLocationEncounters(List<Actor> locationPlayers)
        {
            var players = new Stack<Actor>(locationPlayers.OrderBy(x => Guid.NewGuid()).ToList());

            List<IEncounter> encounters = new List<IEncounter>();

            while (players.Count > 1)
            {
                var p1 = players.Pop();
                var p2 = players.Pop();
                encounters.Add(new Encounter(p1, p2));
            }

            if (players.Count == 1)
            {
                encounters.Add(new EventEncounter(players.Pop()));
            }

            return encounters;
        }

        public string GetLocationString(Location location)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < Dimension; y++)
            {
                for (int x = 0; x < Dimension; x++)
                {
                    var location1 = Locations[x + y * Dimension];
                    if (location1 == location)
                    {
                        if (location1.IsDeadly)
                        {
                            sb.Append("⚠️");
                        }
                        else
                        {
                            sb.Append("❌");
                        }
                    }
                    else if (location1.IsDeadly)
                    {
                        sb.Append("⬛️");
                    }
                    else
                    {
                        sb.Append("⬜️");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public interface INotificator
    {
        void GameHasStarted();

         void GameHasEnded(List<Actor> results);

         void RoundHasEnded(int round);

         void GameAreaIsReduced();
    }
}
