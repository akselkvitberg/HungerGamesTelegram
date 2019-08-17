using System.Collections.Generic;
using System.Linq;

namespace HungerGamesTelegram
{
    class Location
    {
        public string Name { get; set; }
        public List<Actor> Players { get; } = new List<Actor>();
        public Dictionary<string, Location> Directions {get;} = new Dictionary<string, Location>();
        public bool IsDeadly {get;set;} = false;
        public bool IsStartingPoint { get; internal set; }
    }

    static class LocationFactory
    {
        public static List<Location> CreateLocations(int dimension)
        {
            Queue<string> names = new Queue<string>()
            {
                // Add names
            };

            Location[,] locations = new Location[dimension,dimension];
            List<Location> list = new List<Location>();

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    locations[x,y] = new Location()
                    {
                        Name = $"Koordinat {x}, {y}"
                    };
                    list.Add(locations[x,y]);
                }
            }

            locations[dimension/2, dimension/2].IsStartingPoint = true;
            locations[0, 0].IsDeadly = true;
            locations[0,dimension-1].IsDeadly = true;
            locations[dimension-1, dimension-1].IsDeadly=true;
            locations[dimension-1, 0].IsDeadly = true;

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    if(x != 0) {
                        locations[x,y].Directions.Add("vest",locations[x-1,y]);
                    }
                    if(y != 0) {
                        locations[x,y].Directions.Add("nord", locations[x,y-1]);
                    }
                    if(x != dimension-1) {
                        locations[x,y].Directions.Add("øst",locations[x+1,y]);
                    }
                    if(y != dimension-1) {
                        locations[x,y].Directions.Add("sør", locations[x,y+1]);
                    }
                }
            }
            
            return list;
        }
    }
}
