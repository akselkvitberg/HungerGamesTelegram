using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HungerGamesTelegram.Events
{
    public class NothingHappenedEvent : EventBase
    {
        readonly string[] messages = 
        {
            "Du er helt alene, ingen ting skjer.",
            "Plutselig hører du noe... Det var ingen ting.",
            "Det er ingen ting her bortsett fra gresshopper."
        };

        public NothingHappenedEvent()
        {
            EventText = messages.GetRandom();
            Option("Ok", player => player.Message("Du går videre"));
        }
    }

    public class LootEvent : EventBase
    {
        static LootEvent()
        {
            messages = File.ReadAllLines("Events/LootEvents.csv").Skip(1).ToArray();
            var itemlist = File.ReadAllLines("Events/items.csv").Skip(1).Select(x=>x.Split(new []{",", ";"}, StringSplitOptions.None)).ToArray();
            foreach (var item in itemlist)
            {
                var name = item[0];
                var value = int.Parse(item[1]);
                var weight = int.Parse(item[2]);
                items.Add((name, value, weight));
            }
        }

        private static readonly string[] messages;

        private static readonly List<(string name, int value, int weight)> items = new List<(string name, int value, int weight)>();

        public LootEvent()
        {
            var item = GetWeightedRandomItem();
            EventText = string.Format(messages.GetRandom(), item.name);

            Option("La det ligge", player => player.Message("Du lar det ligge, og går videre"));

            Option("Plukk det opp!", player => {
                player.Level += item.value;
                player.Message($"Du fant {item.name} (+lvl {item.value})", $"Du er level *{player.Level}*");
            });
        }

        public static (string name, int value, int weight) GetWeightedRandomItem()
        {
            var totalWeight = items.Sum(x=>x.weight);

            var prob = random.Next(totalWeight);

            var sum = 0;
            foreach (var item in items)
            {
                sum += item.weight;
                if (prob < sum)
                {
                    return item;
                }
            }

            return items.Last();
        }
    }

    public class PriceEvent : EventBase 
    {
        static List<string> prices = new List<string>
        {
            "Du finner en sjokolade. Gå til Eivind Kvitberg for å få premien på Torsteinslåtta"
        };

        public PriceEvent()
        {
            if(prices.Count == 0)
            {
                EventText = "Du finner ingen ting her.";
                Option("Ok", player => player.Message("Du går videre"));
                return;
            }

            EventText = prices.GetRandom();
            prices.Remove(EventText);

            Option("Kult!", player => {
                //player.Level++;
                player.Message("Du går videre");
            });
        }
    }

    public class FeralDogsEvent : EventBase
    {
        public FeralDogsEvent()
        {
            EventText = "Ville hunder angriper deg!\nHva gjør du?";

            Option("Gjem deg", player =>
            {
                player.Message(
                    "Hundene drepte deg",
                    "Spillet er over");
                player.IsDead = true;
            });

            Option("Angrip", player =>
            {
                player.Level += 1;
                player.Message(
                    "Du jagde vekk hundene **(+1 lvl)**.",
                    $"Du er level *{player.Level}*"
                    );
            });

            Option("Løp vekk", player =>
            {
                player.Message("Du slapp unna de ville hundene.");
            });
        }
    }

    public class MeteoriteEvent : EventBase
    {
        public MeteoriteEvent()
        {
            EventText = "En meteoritt kommer mot deg";

            Option("Frys", player =>
            {
                player.IsDead = true;
                player.Message("Meteoritten traff deg, og du døde...");
            });

            Option("Løp vekk", player => player.Message("Du slapp så vidt unna"));

            Option("Slå den tilbake", player =>
            {
                player.Level += 3;
                player.Message(
                    "*WOW!*",
                    "Du slo meteoritten helt ut i verdensrommet! *(lvl +3)*",
                    $"Du er nå level *{player.Level}*");
            });
        }
    }

    public class Excalibur : EventBase
    {
        private static bool swordIsPulled = false; // todo: dette virker ikke med flere spill...

        public Excalibur()
        {
            if (swordIsPulled)
            {
                EventText = "Du finner sverdet i steinen, men noen har allerede tatt steinen.";
                Option("Å nei!", player => player.Message("Du går videre"));
            }
            else
            {
                EventText = "Du finner sverdet i steinen. Vil du prøve å dra opp sverdet?";
                Option("Ja", player =>
                {
                    if (random.NextDouble() > 0.8)
                    {
                        swordIsPulled = true;
                        player.Message(
                            "Du klarte å trekke sverdet ut av steinen! *(+5 lvl)*",
                            $"Du er nå level *{player.Level}*");
                    }
                    else
                    {
                        player.Message("Du klarte ikke å trekke ut sverdet av steinen.");
                    }
                });
                Option("Nei", player => player.Message("Du lar sverdet være og går videre."));
            }
        }
    }

    public class Quicksand : EventBase
    {
        public Quicksand()
        {
            EventText = "Det virker som om bakken plutselig kommer nærmere... \n*Kvikksand!*\nDu har sunket allerede ned til livet!\nHva gjør du?";
            Option("Rop om hjelp", player =>
            {
                player.Message("Ingen kom og hjalp deg. Du forsvinner sakte ned i sanden.", "Spillet er over.");
                player.IsDead = true;
            });
            Option("Grip en kvist og dra deg ut", player =>
            {
                player.Message("Du klarte så vidt å dra deg opp av kvikksanden");
            });
        }
    }

    public class DeathCave : EventBase
    {
        public DeathCave()
        {
            EventText = "Et skilt peker mot en grotte:\n*Dødshulen*\n.Vil du gå inn?";
            Option("Nei", player =>
            {
                player.Message("Godt valg. Du fortsetter i den andre retningen.");
            });
            Option("Ja", player =>
            {
                player.IsDead = true;
                player.Message("Du går inn i grotten. Det lukter sopp og tåfis. Plutselig faller det en stein ned bak deg. Og en til! Grotta kollapser, og sperrer deg inne.\n");

                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    player.Message("Det er bekmørkt, og helt stille. Plutselig hører du noe som beveger seg... Som kommer nærmere.");
                    await Task.Delay(1000);
                    player.Message("og nærmere...");
                    //todo: interactive event here?
                    await Task.Delay(2000);
                    player.Message("-------------");
                    await Task.Delay(500);
                    player.Message("Du døde.\nSpillet er over");
                });
            });
        }
    }
}