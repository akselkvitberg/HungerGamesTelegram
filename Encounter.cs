﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HungerGamesTelegram
{
    class Encounter: IEncounter
    {
        static Encounter()
        {
            var buddyMessages = File.ReadAllLines("Events\\buddy.csv").Skip(1).Select(x=>x.Split(new []{"\t"}, StringSplitOptions.RemoveEmptyEntries));
            foreach (var buddyMessage in buddyMessages)
            {
                var value = int.Parse(buddyMessage[1]);
                var weight = int.Parse(buddyMessage[2]);
                buddyRewards.Add((buddyMessage[0], value, weight));
            }
        }

        static readonly List<(string text, int value, int weight)> buddyRewards = new List<(string text, int value, int weight)>();

        public Actor Player1 { get; }
        public Actor Player2 { get; }
        private readonly bool _limitOptions;

        private readonly Dictionary<string, EncounterReply> options = new Dictionary<string, EncounterReply>()
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

            if (player1.Location.Directions.All(x => x.Value.IsDeadly))
            {
                _limitOptions = true;
            }
        }

        public virtual void Prompt()
        {
            if (_limitOptions)
            {
                Player1.EventPrompt($"Du møter på *{Player2.Name}* (lvl {Player2.Level})\nDere er trengt opp i et hjørne!\nANGRIP!", new[] { "Angrip" });
                Player2.EventPrompt($"Du møter på *{Player1.Name}* (lvl {Player1.Level})\nDere er trengt opp i et hjørne!\nANGRIP!", new[] { "Angrip" });
            }
            else
            {
                string attackPercentage1 = GetWinChance(Player1.Level, Player2.Level);
                string runPercentage1 = GetRunAwayChance(Player1.Level, Player2.Level);
                string attackPercentage2 = GetWinChance(Player2.Level, Player1.Level);
                string runPercentage2 = GetRunAwayChance(Player2.Level, Player1.Level);

                Player1.EventPrompt($"Du møter på *{Player2.Name}* (lvl {Player2.Level})\nHva vil du gjøre?\nAngrip: {attackPercentage1}%\nLøp vekk {runPercentage1}%", Options);
                Player2.EventPrompt($"Du møter på *{Player1.Name}* (lvl {Player1.Level})\nHva vil du gjøre?\nAngrip: {attackPercentage2}%\nLøp vekk {runPercentage2}%", Options);
            }
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
                    ShareEvent();
                    break;
                case (EncounterReply.RunAway, EncounterReply.RunAway):
                    Player1.RunAway(Player2);
                    Player2.RunAway(Player1);
                    break;

                case (EncounterReply.RunAway, EncounterReply.Attack):
                    ResolveAttackRunAway(attack:Player2, runAway:Player1);
                    break;
                case (EncounterReply.Attack, EncounterReply.RunAway):
                    ResolveAttackRunAway(Player1, Player2);
                    break;

                case (EncounterReply.RunAway, EncounterReply.Loot):
                    Player1.RunAway(Player2);
                    Player2.Loot(Player1);
                    break;
                case (EncounterReply.Loot, EncounterReply.RunAway):
                    Player1.Loot(Player2);
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

        private void ShareEvent()
        {
            var reward = GetWeightedRandomMessage();

            Player1.Level += reward.value;
            var message1 = reward.text + $"\nDu er level ({Player1.Level})";
            Player1.Message(message1);

            Player2.Level += reward.value;
            var message2 = reward.text + $"\nDu er level ({Player1.Level})";
            Player2.Message(message2);
        }

        public static (string text, int value, int weight) GetWeightedRandomMessage()
        {
            var totalWeight = buddyRewards.Sum(x => x.weight);

            var prob = Extensions.Random.Next(totalWeight);

            var sum = 0;
            foreach (var item in buddyRewards)
            {
                sum += item.weight;
                if (prob < sum)
                {
                    return item;
                }
            }

            return buddyRewards.Last();
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


        public string GetWinChance(int player1Level, int player2Level)
        {
            var diff = (0.5 - ((player1Level - player2Level) / 10.0)) * 100;
            diff = Math.Min(100, Math.Max(0, diff));
            return Math.Floor(diff).ToString();
        }

        private void ResolveAttack(Actor player1, Actor player2)
        {
            var diff = 0.5 - ((player1.Level - player2.Level) / 10.0);

            if(Extensions.Random.NextDouble() > diff)
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

        public string GetRunAwayChance(int attackLevel, int runAwayLevel)
        {
            var diff = (1 - ((runAwayLevel - attackLevel) / 10.0)) * 100;
            diff = Math.Min(100, Math.Max(0, diff));
            return Math.Floor(diff).ToString();
        }

        private void ResolveAttackRunAway(Actor attack, Actor runAway)
        {
            var diff = (attack.Level - runAway.Level) / 10.0;

            if (Extensions.Random.NextDouble() > diff)
            {
                attack.FailAttack(runAway);
                runAway.RunAway(attack);
            }
            else
            {
                attack.SuccessAttack(runAway);
                runAway.Die(attack);
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
