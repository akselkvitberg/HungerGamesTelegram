using System;
using System.Collections.Generic;

namespace HungerGamesTelegram
{
    class Encounter: IEncounter
    {
        public Actor Player1 { get; set; }
        public Actor Player2 { get; set; }

        public virtual void Prompt()
        {
            Player1.EncounterPrompt(Player2);
            Player2.EncounterPrompt(Player1);
        }

        public virtual void RunEncounter()
        {
            switch ((Player1.EncounterAction, Player2.EncounterAction))
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
            var diff = 0.5 - ((player1.Level - player2.Level) / 7.0);
            
            if(random.NextDouble() > diff)
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

    enum EncounterReply
    {
        Attack,
        Loot,
        RunAway
    }

    internal interface IEncounter
    {
        void Prompt();

        void RunEncounter();
        List<Actor> GetDeadPlayers();
    }
}
