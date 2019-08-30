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
            // var adj = new[]
            // {
            //     "Skumle",
            //     "Rotten",
            //     "Store",
            //     "Svarte",
            //     "Ensom",
            //     "Øde",
            //     "Røde",
            //     "Gamle",
            //     "Døds",
            //     "Uffe",
            //     "Troll",
            //     "Jotne",
            //     "Fryse",
            //     "Gufse",
            //     "Hulder",
            //     "Sekke",
            //     "Flamme",
            //     "Gremme",
            //     "Lille",
            //     "Ild",
            //     "Synke",
            //     "Glemsom",
            //     "Verke",
            //     "Konge",
            //     "Løve",
            //     "Drage",
            //     "Grav",
            //     "Goliat",
            //     "Skumrings",
            //     "Fuktig",
            //     "Våte",
            //     "Støve",
            //     "Gress",
            //     "Løv",
            // };

            // var sub = new[] {
            //     "fjellet",
            //     "myra",
            //     "dalen",
            //     "skogen",
            //     "skråningen",
            //     "bukta",
            //     "odden",
            //     "vika",
            //     "hullet",
            //     "lia",
            //     "slotta",
            //     "marken",
            //     "elva",
            //     "bekken",
            //     "krattet",
            //     "berget",
            //     "stad",
            //     "åkeren",
            //     "skauen",
            //     "lysningen",
            //     "engen",
            //     "glennen",
            //     "grotta",
            //     "kysten",
            //     "stranda",
            //     "klippen",
            //     "vannet",
            //     "bakken",
            //     "åsen",
            //     "treet",
            //     "graven",
            // };

            // List<string> placeNames = new List<string>();

            // while(placeNames.Count != dimension*dimension)
            // {
            //     var s = sub.GetRandom();
            //     var a = adj.GetRandom();
            //     var txt = $"{a}-{s}";
            //     if(placeNames.Contains(txt))
            //     {
            //         continue;
            //     }
            //     placeNames.Add(txt);
            // }

            List<string> placeNames = new List<string>()
            {
                "Platålunden",
                "Ødegraven",
                "Rottenlunden",
                "Platågryta",
                "Uffekleiva",
                "Flammejordet",
                "Utsiktsstad",
                "Goliatskauen",
                "Storefjellet",
                "Gufsegrotta",
                "Dragebekken",
                "Svarteodden",
                "Fuktigvannet",
                "Verkestad",
                "Flammelunden",
                "Våtehaugen",
                "Ildsletta",
                "Synkelysningen",
                "Fuktigbekken",
                "Ildtoppen",
                "Rottenslotta",
                "Skumringsgrotta",
                "Synkelunden",
                "Dødshaugen",
                "Kongelia",
                "Løvsletta",
                "Støvesporet",
                "Trollkroken",
                "Ødehullet",
                "Løveskauen",
                "Flammedammen",
                "Skumleplassen",
                "Rottenelva",
                "Uffehula",
                "Kongehullet",
                "Skumringslia",
                "Glemsomdalen",
                "Svartemyra",
                "Dødslysningen",
                "Flammesteinen",
                "Skumringsvika",
                "Huldersteinen",
                "Ildvika",
                "Løvbakken",
                "Kongesporet",
                "Glemsomlunden",
                "Rødelunden",
                "Rødedammen",
                "Løvegraven",
                "Kongedalen",
                "Storevika",
                "Uffegrotta",
                "Gravsøkket",
                "Gremmedalen",
                "Goliatberget",
                "Skumringssøkket",
                "Skumleberget",
                "Gamlelysningen",
                "Dragebukta",
                "Våteberget",
                "Løvdammen",
                "Støveåsen",
                "Lillesøkket",
                "Svarteberget",
                "Kongelunden",
                "Verkelunden",
                "Gressklippen",
                "Gravdammen",
                "Ensombekken",
                "Fuktigskråningen",
                "Trolltoppen",
                "Storeklippen",
                "Ensomslotta",
                "Drageplassen",
                "Rottenkleiva",
                "Ødebekken",
                "Jotnehaugen",
                "Lillefjellet",
                "Gufsekroken",
                "Løvemyra",
                "Ødefjellet",
                "Støvekollen",
                "Gressstad",
                "Skumlelunden",
                "Gravvik",
                "Skumletreet",
                "Skumringsberget",
                "Rottenåkeren",
                "Storedalen",
                "Trollstad",
                "Goliatsteinen",
                "Kongevik",
                "Støvestad",
                "Glemsomkollen",
                "Flammeåkeren",
                "Glemsomhaugen",
                "Ensomberget",
                "Svarteklippen",
                "Utsiktsbukta",
                "Gamlegraven",
                "Trolllysningen",
                "Lilleberget",
                "Jotnelysningen",
                "Ildsøkket",
                "Gresslia",
                "Ildengen",
                "Flammesletta",
                "Trollhullet",
                "Gresslunden",
                "Gamleløkka",
                "Uffeelva",
                "Skumringslunden",
                "Dødslunden",
                "Utsiktslia",
                "Støvegraven",
                "Ensomsletta",
                "Uffemarken",
                "Drageløkka",
                "Lilleløkka",
                "Gravbakken",
                "Goliatsporet",
                "Gremmesporet",
                "Glemsommarken",
                "Ødejordet",
                "Dødsmyra",
                "Skumlelia",
                "Uffejordet",
                "Lillesporet",
                "Gremmegryta",
                "Lillejordet",
                "Støveelva",
                "Gamlebakken",
                "Utsiktsåkeren",
                "Kongeberget",
                "Rottenbukta",
                "Fuktigskauen",
                "Gufseslotta",
                "Løvelva",
                "Skumringskrattet",
                "Gamlestubben",
                "Storekrattet",
                "Rottenhula",
                "Flammestubben",
                "Hulderstad",
                "Platåsteinen",
                "Løvåkeren",
                "Trollsøkket",
                "Platåberget",
                "Støvehula",
                "Rødehaugen",
                "Goliatmarken",
                "Flammesporet",
                "Drageslotta",
                "Gresslysningen",
                "Synkebukta",
                "Gremmebakken",
                "Frysedammen",
                "Gremmevannet",
                "Skumringshullet",
                "Hulderhula",
                "Gravløkka",
                "Platååkeren",
                "Skumringsslotta",
                "Rødetoppen",
                "Jotnesteinen",
                "Uffehullet",
                "Uffeløkka",
                "Løvsøkket",
                "Utsiktsklippen",
                "Dragemarken",
                "Dødsmarken",
                "Verkedammen",
                "Gravelva",
                "Dødsodden",
                "Ødedammen",
                "Gamlevik",
                "Utsiktssletta",
                "Flammebukta",
                "Fryseodden",
                "Skumringsmyra",
                "Kongevannet",
                "Jotnesletta",
                "Fuktigkroken",
                "Utsiktsløkka",
                "Rottenberget",
                "Ødelunden",
                "Svartelunden",
                "Gravsletta",
                "Goliatstad",
                "Goliatgrotta",
                "Hulderløkka",
                "Frysekrattet",
                "Hulderbukta",
                "Ildskauen",
                "Verkeplassen",
                "Gravskauen",
                "Fuktighaugen",
                "Platåplassen",
                "Løvkroken",
                "Platåvika",
                "Gressengen",
                "Svartekollen",
                "Verkemyra",
                "Verkegraven",
                "Huldersporet",
                "Ildkrattet",
                "Dødsplassen",
                "Synkeplassen",
                "Kongebakken",
                "Gresshaugen",
                "Huldersøkket",
                "Gamlevika",
                "Storeskråningen",
                "Utsiktsvika",
                "Rødekroken",
                "Ensomlia",
                "Lillevannet",
                "Løvengen",
                "Gufseløkka",
                "Gremmekrattet",
                "Rottenhaugen",
                "Løveskråningen",
                "Frysedalen",
                "Gufseplassen",
                "Skumringsdalen",
                "Trollskogen",
                "Utsiktslunden",
                "Storelysningen",
                "Dødsbukta",
                "Ødeodden",
                "Løveløkka",
                "Uffebakken",
                "Trollvika",
                "Støveberget",
                "Våtehullet",
                "Svartelia",
                "Gufsekollen",
                "Storekollen",
                "Fuktigsletta",
                "Dragedammen",
                "Ildjordet",
                "Løvhullet",
                "Verkevik",
                "Skumlebakken",
                "Storeslotta",
                "Platåbukta",
                "Løvstubben",
                "Synkehullet",
                "Gufseberget",
                "Hulderodden",
                "Gamlemarken",
                "Gamleodden",
                "Glemsomlia",
                "Gamleengen",
                "Dødsvik",
                "Verkevannet",
            };

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
