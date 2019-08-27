﻿using System;
using System.IO;

namespace HungerGamesTelegram
{
    public static class Logger
    {
        public static void Log(Actor actor, string message)
        {
            var line = DateTime.Now.ToString("HH:mm:ss") + ": " + message.Replace("\n", "\n          ");
            if (actor is TelegramPlayer player)
            {
                try
                {
                    EnsureFolder(Path.Combine("log", player.Game.Name));

                    File.AppendAllText(Path.Combine("log", player.Game.Name, "eventlog.txt"), line + "\n");
                    File.AppendAllText(Path.Combine("log", player.Game.Name, player.Name + ".txt"), line + "\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void EnsureFolder(string combine)
        {
            Directory.CreateDirectory(combine);
        }

        public static void Log(Game game, string message)
        {
            var line = DateTime.Now.ToString("HH:mm:ss") + ": " + message.Replace("\n", "\n          ");
            try
            {
                EnsureFolder(Path.Combine("log", game.Name));
                File.AppendAllText(Path.Combine("log", game.Name, "gamelog.txt"), line + "\n");
                File.AppendAllText(Path.Combine("log", game.Name, "eventlog.txt"), line + "\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
