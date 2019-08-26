using System;
using System.Collections.Generic;

namespace HungerGamesTelegram.Events
{
    public class FeralDogsEvent : EventBase
    {
        public override string EventMessage => "Ville hunder angriper deg!\nHva gjør du?";

        public override Dictionary<string, Action<Actor>> Options { get; } = new Dictionary<string, Action<Actor>>()
        {
            ["Gjem deg"] = actor =>
            {
                actor.Message("Hundene drepte deg\nSpillet er over");
                actor.IsDead = true;
            },
            ["Angrip"] = actor =>
            {
                actor.Level += 1;
                actor.Message($"Du jagde vekk hundene **(+1 lvl)**.\nDu er level *{actor.Level}*");
            },
            ["Løp vekk"] = actor =>
            {
                actor.Message("Du slapp unna de ville hundene.");
            },
        };
    }
}