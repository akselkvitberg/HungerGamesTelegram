using System;
using System.Linq;
using System.Threading.Tasks;

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
        public EncounterReply EncounterAction { get; internal set; }

        public abstract void EncounterPrompt(Actor actor);

        public virtual void NoEncounterPrompt()
        {
            EncounterAction = EncounterReply.Loot;
        }

        public virtual void Share(Actor actor)
        {
            Level += 1;
        }

        public virtual void FailAttack(Actor actor)
        {
            Level += 1;
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

        public abstract void MovePrompt();
        public abstract void Move();

        internal void Move(Location nextLocation)
        {
            if (nextLocation != null)
            {
                Location.Players.Remove(this);
                nextLocation.Players.Add(this);
                Location = nextLocation;
            }
        }

        public virtual void KillZone()
        {
            IsDead = true;
        }
    }

    abstract class Bot : Actor
    {

        public Bot() 
        {
            this.Name = this.GetType().Name;
        }
        
        public override void MovePrompt()
        {

        }

        public override void Move() 
        {
            var nextLocation = Location.Directions.Values.Concat(new []{Location}).Where(x=>!x.IsDeadly).OrderBy(x=>Guid.NewGuid()).FirstOrDefault();
            base.Move(nextLocation);
        }
    }
    class RandomBot : Bot
    {
        public override void EncounterPrompt(Actor actor) 
        {
            if(Location.Directions.All(x=>x.Value.IsDeadly))
            {
                EncounterAction = (EncounterReply)random.Next(0, 2);
            }
            else
            {
                EncounterAction = (EncounterReply)random.Next(0, 3);
            }
        }
    }

    class AttackBot : Bot
    {
        public override void EncounterPrompt(Actor actor)  {EncounterAction = EncounterReply.Attack;}
    }

    class RunBot : Bot
    {
        public override void EncounterPrompt(Actor actor) 
        {
            if(Location.Directions.All(x=>x.Value.IsDeadly))
            {
                EncounterAction = EncounterReply.Attack;
            }
            else
            {
                EncounterAction = EncounterReply.RunAway;
            }
        }
    }

    class LootBot : Bot
    {
        public override void EncounterPrompt(Actor actor) {EncounterAction = EncounterReply.Loot;}
    }

    class Bot1 : Bot
    {
        public override void EncounterPrompt(Actor actor) 
        {
            if(Level > 5)
            {
                EncounterAction = EncounterReply.Attack;
            }
            else EncounterAction = EncounterReply.Loot;
        }
    }

    class Bot2 : Bot
    {
        public override void EncounterPrompt(Actor actor) 
        {
            if(Level % 2 == 0)
            {
                EncounterAction = EncounterReply.Attack;
            }
            else EncounterAction = EncounterReply.Loot;
        }
    }
}
