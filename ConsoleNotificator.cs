using System;
using System.Collections.Generic;

namespace HungerGamesTelegram
{
    class ConsoleNotificator : INotificator
    {
        public void GameAreaIsReduced()
        {
            Console.WriteLine("Området har blitt begrenset");
        }

        public void GameHasEnded(List<Actor> results)
        {
            Console.WriteLine("Spillet er slutt");
        }

        public void GameIsStarting()
        {
            Console.WriteLine("Runden starter");
        }

        public void GameHasStarted()
        {
            Console.WriteLine("Spillet har startet");
        }

        public void RoundHasEnded(int round)
        {
            Console.WriteLine($"{round}. runde er over");
        }
    }
}