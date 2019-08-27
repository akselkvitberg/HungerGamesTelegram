using System;
using System.Linq;
using System.Threading.Tasks;

namespace HungerGamesTelegram
{
    public abstract class Actor
    {
        public Location Location { get; set; }
        public int Level { get; set; } = 1;
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public int Rank { get; set; }
        public string EventEncounterReply { get; internal set; }

        public abstract void EventPrompt(string message, string[] options);

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

        public virtual void Loot(Actor player1)
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

        public abstract void Result(int rank);
        public abstract void Message(params string[] message);
    }

    abstract class Bot : Actor
    {

        public Bot() 
        {
            Name = GetType().Name;
        }

        public override void EventPrompt(string message, string[] options)
        {
            EventEncounterReply = options.GetRandom();
        }

        public override void MovePrompt()
        {

        }

        public override void Move() 
        {
            var nextLocation = Location.Directions.Values.Concat(new []{Location}).Where(x=>!x.IsDeadly).OrderBy(x=>Guid.NewGuid()).FirstOrDefault();
            base.Move(nextLocation);
        }

        public override void Result(int rank)
        {
            
        }

        public override void Message(params string[] message)
        {
            
        }
    }

    class RandomBot : Bot
    {
        public override void EventPrompt(string message, string[] options)
        {
            EventEncounterReply = options.GetRandom();
        }
    }
}
