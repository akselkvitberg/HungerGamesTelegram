using System;
using System.Collections.Generic;
using System.Linq;

namespace HungerGamesTelegram
{
    class Encounter: IEncounter
    {
        public Actor Player1 { get; }
        public Actor Player2 { get; }

        private Dictionary<string, EncounterReply> options = new Dictionary<string, EncounterReply>()
        {
            ["Angrip"] = EncounterReply.Attack,
            ["Vær kompis"] = EncounterReply.Loot,
            ["Løp vekk"] = EncounterReply.RunAway,
        };

        private string[] Options => options.Select(x => x.Key).ToArray();

        public Encounter(Actor player1, Actor player2)
        {
            Player1 = player1;
            Player2 = player2;
        }

        public virtual void Prompt()
        {
            Player1.EventPrompt($"Du er her: *{Player1.Location.Name}*\nDu møter på *{Player2.Name}* (lvl {Player2.Level})\nDu er level *{Player1.Level}*\nHva vil du gjøre?", Options);
            Player2.EventPrompt($"Du er her: *{Player2.Location.Name}*\nDu møter på *{Player1.Name}* (lvl {Player1.Level})\nDu er level *{Player2.Level}*\nHva vil du gjøre?", Options);
        }

        public virtual void RunEncounter()
        {
            var p1Action = EncounterReply.Loot;
            var p2Action = EncounterReply.Loot;

            if (options.ContainsKey(Player1.EventEncounterReply))
            {
                p1Action = options[Player1.EventEncounterReply];
            }

            if (options.ContainsKey(Player2.EventEncounterReply))
            {
                p2Action = options[Player2.EventEncounterReply];
            }

            switch ((p1Action, p2Action))
            {
                case (EncounterReply.Attack, EncounterReply.Attack):
                    ResolveAttack(Player1, Player2);
                    break;

                case (EncounterReply.Loot, EncounterReply.Loot):
                    Player1.Share(Player2);
                    Player2.Share(Player1);
                    break;
                case (EncounterReply.RunAway, EncounterReply.RunAway):
                    Player1.RunAway(Player2);
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.RunAway, EncounterReply.Attack):
                    Player1.RunAway(Player2);
                    Player2.FailAttack(Player1);
                    break;
                case (EncounterReply.Attack, EncounterReply.RunAway):
                    Player1.FailAttack(Player2);
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.RunAway, EncounterReply.Loot):
                    Player1.RunAway(Player2);
                    Player2.Loot();
                    break;
                case (EncounterReply.Loot, EncounterReply.RunAway):
                    Player1.Loot();
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.Attack, EncounterReply.Loot):
                    Player1.SuccessAttack(Player2);
                    Player2.Die(Player1);
                    break;
                case (EncounterReply.Loot, EncounterReply.Attack):
                    Player1.Die(Player2);
                    Player2.SuccessAttack(Player1);
                    break;
            }
        }

        public List<Actor> GetDeadPlayers()
        {
            List<Actor> deadPlayers = new List<Actor>();

            if (Player1.IsDead)
            {
                deadPlayers.Add(Player1);
            }
            if (Player2.IsDead)
            {
                deadPlayers.Add(Player2);
            }
            return deadPlayers;
        }

        private static Random random = new Random();

        private void ResolveAttack(Actor player1, Actor player2)
        {
            if (player1.Level == player2.Level)
            {
                if (random.NextDouble() > 0.5)
                {
                    player1.SuccessAttack(player2);
                    player2.Die(player1);
                }
                else
                {
                    player1.Die(player2);
                    player2.SuccessAttack(player1);
                }
            }
            else if (player1.Level > player2.Level)
            {
                player1.SuccessAttack(player2);
                player2.Die(player1);
            }
            else
            {
                player1.Die(player2);
                player2.SuccessAttack(player1);
            }
        }
    }

    public enum EncounterReply
    {
        Attack,
        Loot,
        RunAway
    }

    public interface IEncounter
    {
        void Prompt();
        void RunEncounter();
        List<Actor> GetDeadPlayers();
    }
}
