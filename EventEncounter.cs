using System;
using System.Collections.Generic;
using System.Linq;

namespace HungerGamesTelegram
{
    internal class EventEncounter : IEncounter
    {
        internal Actor Player { get; }

        public EventBase CurrentEvent { get; }

        public EventEncounter(Actor player)
        {
            Player = player;
            CurrentEvent = new FeralDogsEvent();
        }

        public void Prompt()
        {
            Player.EventPrompt(CurrentEvent.EventMessage, CurrentEvent.Responses);
        }

        public void RunEncounter()
        {
            CurrentEvent.RunEvent(Player, Player.EventEncounterReply);
        }

        public List<Actor> GetDeadPlayers()
        {
            if (Player.IsDead)
            {
                return new List<Actor>()
                {
                    Player
                };
            }

            return new List<Actor>();
        }
    }

    public abstract class EventBase
    {
        public abstract string EventMessage { get; }

        public string[] Responses => Options.Select(x => x.Key).ToArray();
        public abstract Dictionary<string, Action<Actor>> Options { get; }

        public void RunEvent(Actor actor, string response)
        {
            if (Options.ContainsKey(response))
            {
                Options[response](actor);
            }
            else
            {
                Options.First().Value(actor);
            }
        }
    }

    public class FeralDogsEvent : EventBase
    {
        public override string EventMessage => "Ville hunder angriper deg!\nHva gjør du?";

        public override Dictionary<string, Action<Actor>> Options { get; } = new Dictionary<string, Action<Actor>>()
        {
            ["Angrip"] = actor =>
            {
                actor.Level += 1;
                actor.Message($"Du jagde vekk hundene **(+1 lvl)**.\nDu er level *{actor.Level}*");
            },
            ["Løp vekk"] = actor =>
            {
                actor.Message("Du slapp unna de ville hundene.");

            },
            ["Gjem deg"] = actor =>
            {
                actor.Message("Hundene drepte deg\nSpillet er over");
                actor.IsDead = true;
            },
        };
    }
}