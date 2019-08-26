using System;
using System.Collections.Generic;
using System.Linq;
using HungerGamesTelegram.Events;

namespace HungerGamesTelegram
{
    internal class EventEncounter : IEncounter
    {
        internal Actor Player { get; }
        Random random = new Random();

        public EventBase CurrentEvent { get; }

        List<Func<EventBase>> _eventList = new List<Func<EventBase>>()
        {
            () => new FeralDogsEvent(),
            () => new MeteoriteEvent(),
        };

        public EventEncounter(Actor player)
        {
            Player = player;
            CurrentEvent = _eventList[random.Next(_eventList.Count)]();
        }

        public void Prompt()
        {
            Player.EventPrompt(CurrentEvent.EventText, CurrentEvent.Responses);
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
        protected Random random = new Random();

        public string EventText { get; protected set; }
        public Dictionary<string, Action<Actor>> Options { get; } = new Dictionary<string, Action<Actor>>();


        public string[] Responses => Options.Select(x => x.Key).ToArray();

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

        protected void Option(string option, Action<Actor> action)
        {
            Options.Add(option, action);
        }
    }
}