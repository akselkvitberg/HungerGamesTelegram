using System;
using System.Collections.Generic;

namespace HungerGamesTelegram.Events
{
    public class FeralDogsEvent : EventBase
    {
        public FeralDogsEvent()
        {
            EventText = "Ville hunder angriper deg!\nHva gjør du?";

            Option("Gjem deg", actor =>
            {
                actor.Message(
                    "Hundene drepte deg",
                    "Spillet er over");
                actor.IsDead = true;
            });

            Option("Angrip", actor =>
            {
                actor.Level += 1;
                actor.Message(
                    "Du jagde vekk hundene **(+1 lvl)**.",
                    $"Du er level *{actor.Level}*"
                    );
            });

            Option("Løp vekk", actor =>
            {
                actor.Message("Du slapp unna de ville hundene.");
            });
        }
    }

    public class MeteoriteEvent : EventBase
    {
        public MeteoriteEvent()
        {
            EventText = "En meteoritt kommer mot deg";
            Option("Frys", actor =>
            {
                actor.IsDead = true;
                actor.Message("Meteoritten traff deg, og du døde...");
            });

            Option("Løp vekk", actor => actor.Message("Du slapp så vidt unna"));

            Option("Slå den tilbake", actor =>
            {
                actor.Level += 3;
                actor.Message(
                    "*WOW!*",
                    "Du slo meteoritten helt ut i verdensrommet! *(lvl +3)*",
                    $"Du er nå level *{actor.Level}*");
            });
        }
    }
}