using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace HungerGamesTelegram
{
    class Program
    {
        static async Task Main(string[] args)
        {
            WriteLine("Hunger Games");

            //var telegramGameHost = new TelegramGameHost();
            //telegramGameHost.Start();

            //Thread.Sleep(int.MaxValue);

            Game game = new Game(new ConsoleNotificator());
            while (true)
            {
                game.Players.Add(new ConsolePlayer(game));
                await game.StartGame();
            }
        }
    }

    class ConsoleNotificator : INotificator
    {
        public void GameAreaIsReduced()
        {
            WriteLine("Området har blitt begrenset");
        }

        public void GameHasEnded(List<Actor> results)
        {
            WriteLine("Spillet er slutt");
        }

        public void GameIsStarting()
        {
            WriteLine("Runden starter");
        }

        public void GameHasStarted()
        {
            WriteLine("Spillet har startet");
        }

        public void RoundHasEnded(int round)
        {
            WriteLine($"{round}. runde er over");
        }
    }


    public static class Extensions {
        static Random random = new Random();

        public static T GetRandom<T>(this List<T> list){
            return list[random.Next(list.Count)];
        }

        public static T GetRandom<T>(this T[] list){
            return list[random.Next(list.Length)];
        }
    }
}
