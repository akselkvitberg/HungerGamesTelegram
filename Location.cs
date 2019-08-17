using System.Collections.Generic;

namespace HungerGamesTelegram
{
    class Location
    {
        public string Name { get; set; }
        public List<Actor> Players { get; } = new List<Actor>();
        public Dictionary<string, Location> Directions {get;} = new Dictionary<string, Location>();
        public bool IsDeadly {get;set;} = false;
    }

    static class LocationFactory
    {
        public static Location[,] CreateLocations(int dimension)
        {
            Queue<string> names = new Queue<string>()
            {
                // Add names
            };

            Location[,] locations = new Location[dimension,dimension];

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    locations[x,y] = new Location()
                    {
                        Name = $"Koordinat {x}, {y}"
                    };
                }
            }

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    if(x != 0) {
                        locations[x,y].Directions.Add("Vest",locations[x-1,y]);
                    }
                    if(y != 0) {
                        locations[x,y].Directions.Add("Nord", locations[x,y-1]);
                    }
                    if(x != dimension-1) {
                        locations[x,y].Directions.Add("Øst",locations[x+1,y]);
                    }
                    if(y != dimension-1) {
                        locations[x,y].Directions.Add("Sør", locations[x,y+1]);
                    }
                }
            }
            
            return locations;
        }
    }
}
