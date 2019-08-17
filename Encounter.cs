using System;

namespace HungerGamesTelegram
{
    class Encounter
    {
        public Actor Player1 { get; set; }
        public Actor Player2 { get; set; }

        public virtual void RunEncounter()
        {
            var reply1 = Player1.Encounter(Player2);
            var reply2 = Player2.Encounter(Player1);

            switch ((reply1, reply2))
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

    class NonEncounter : Encounter
    {
        public override void RunEncounter()
        {
            var reply = Player1.NoEncounter();
            if (reply == EncounterReply.Loot)
            {
                Player1.Loot();
            }
        }
    }
}
