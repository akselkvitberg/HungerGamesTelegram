using System;

namespace HungerGamesTelegram
{
    abstract class Actor
    {
        protected static Random random = new Random();
        public Guid PlayerId { get; set; } = Guid.NewGuid();
        public Location Location { get; set; }
        public int Level { get; set; } = 1;
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDead { get; set; }

        public abstract EncounterReply Encounter(Actor actor);

        public virtual EncounterReply NoEncounter()
        {
            return EncounterReply.Loot;
        }

        public virtual void Share(Actor actor)
        {
            Level++;
        }

        public virtual void FailAttack(Actor actor)
        {
            Level += 2;
        }

        public virtual void RunAway(Actor player2)
        {
            
        }

        public virtual void Loot()
        {
            Level += 2;
        }

        public virtual void SuccessAttack(Actor actor)
        {
            Level += 1;
        }

        public virtual void Die(Actor actor)
        {
            IsDead = true;
        }

    }

    class RandomBot : Actor
    {
        public override EncounterReply Encounter(Actor actor) => (EncounterReply)random.Next(0, 3);
    }

    class AttackBot : Actor
    {
        public override EncounterReply Encounter(Actor actor) => EncounterReply.Attack;
    }

    class RunBot : Actor
    {
        public override EncounterReply Encounter(Actor actor) => EncounterReply.RunAway;
    }

    class LootBot : Actor
    {
        public override EncounterReply Encounter(Actor actor) => EncounterReply.Loot;
    }

    class Bot1 : Actor
    {
        public override EncounterReply Encounter(Actor actor) 
        {
            if(Level > 5)
            {
                return EncounterReply.Attack;
            }
            return EncounterReply.Loot;
        }
    }

    class Bot2 : Actor
    {
        public override EncounterReply Encounter(Actor actor) 
        {
            if(Level % 2 == 0)
            {
                return EncounterReply.Attack;
            }
            return EncounterReply.Loot;
        }
    }
}
