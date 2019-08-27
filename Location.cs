using System;
using System.Collections.Generic;
using System.Linq;

namespace HungerGamesTelegram
{
    public class Location
    {
        public string Name { get; set; }
        public List<Actor> Players { get; } = new List<Actor>();
        public Dictionary<string, Location> Directions {get;} = new Dictionary<string, Location>();
        public string Environment { get; set; }
        public bool IsDeadly {get;set;} = false;
        public bool IsStartingPoint { get; internal set; }
    }

    public static class LocationFactory
    {
        public static List<Location> CreateLocations(int dimension)
        {
            var adj = new[]
            {
                "Skumle",
                "Rotten",
                "Store",
                "Svarte",
                "Ensom",
                "Øde",
                "Røde",
                "Gamle",
                "Døds",
                "Uffe",
                "Troll",
                "Jotne",
                "Fryse",
                "Gufse",
                "Hulder",
                "Sekke",
                "Flamme",
                "Gremme",
                "Lille",
                "Ild",
                "Synke",
                "Glemsom",
                "Verke",
                "Konge",
                "Løve",
                "Drage",
                "Grav",
                "Goliat",
                "Skumrings",
                "Fuktig",
                "Våte",
                "Støve",
                "Gress",
                "Løv",
            };

            var sub = new[] {
                "fjellet",
                "myra",
                "dalen",
                "skogen",
                "skråningen",
                "bukta",
                "odden",
                "vika",
                "hullet",
                "lia",
                "slotta",
                "marken",
                "elva",
                "bekken",
                "krattet",
                "berget",
                "stad",
                "åkeren",
                "skauen",
                "lysningen",
                "engen",
                "glennen",
                "grotta",
                "kysten",
                "stranda",
                "klippen",
                "vannet",
                "bakken",
                "åsen",
                "treet",
                "graven",
            };

            List<string> placeNames = new List<string>();

            while(placeNames.Count != dimension*dimension)
            {
                var s = sub.GetRandom();
                var a = adj.GetRandom();
                var txt = a + s;
                if(placeNames.Contains(txt))
                {
                    continue;
                }
                placeNames.Add(txt);
            }

            int nextPlaceName = 0;
            Location[,] locations = new Location[dimension,dimension];
            List<Location> list = new List<Location>();

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    locations[x,y] = new Location()
                    {
                        Name = placeNames[nextPlaceName++]
                    };
                    list.Add(locations[x,y]);
                }
            }

            locations[dimension/2, dimension/2].IsStartingPoint = true;

            locations[0, 0].IsDeadly = true;
            locations[0, 0].Environment = "En dødlig storm.";

            locations[0,dimension-1].IsDeadly = true;
            locations[0,dimension-1].Environment = "En voldsom skogbrann.";

            locations[dimension-1, dimension-1].IsDeadly=true;
            locations[dimension-1, dimension-1].Environment = "Giftig gass.";

            locations[dimension-1, 0].IsDeadly = true;
            locations[dimension-1, 0].Environment = "Flytende lava.";

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    
                    if(y != 0)
                    {
                        locations[x,y].Directions.Add("Nord", locations[x,y-1]);
                    }

                    if (x != 0)
                    {
                        locations[x, y].Directions.Add("Vest", locations[x - 1, y]);
                    }

                    if(x != dimension-1)
                    {
                        locations[x,y].Directions.Add("Øst",locations[x+1,y]);
                    }
                    if(y != dimension-1)
                    {
                        locations[x,y].Directions.Add("Sør", locations[x,y+1]);
                    }
                }
            }
            
            return list;
        }
    }
}
