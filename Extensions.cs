using System;
using System.Collections.Generic;

namespace HungerGamesTelegram
{
    public static class Extensions {
        public static Random Random { get; }= new Random();

        public static T GetRandom<T>(this List<T> list){
            return list[Random.Next(list.Count)];
        }

        public static T GetRandom<T>(this T[] list){
            return list[Random.Next(list.Length)];
        }
    }
}