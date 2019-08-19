using System.Collections.Generic;

namespace HungerGamesTelegram
{
    internal class EventEncounter : IEncounter
    {
        internal Actor Player { get; set; }

        public void Prompt()
        {
            Player.NoEncounterPrompt();
        }

        public void RunEncounter()
        {
            if (Player.EncounterAction == EncounterReply.Loot)
            {
                Player.Loot();
            }
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

    public class Event
    {
        public string EventMessage { get; set; }

        public string[] Responses { get; set; }

        public string RunEvent(string response)
        {
            switch (response)
            {
                case "":
                    break;
            }

            return "";
        }
    }
}