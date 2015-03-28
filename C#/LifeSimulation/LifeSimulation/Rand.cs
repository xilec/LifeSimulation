using System;

namespace LifeSimulation
{
    public static class Rand
    {
         private static Random _rand = new Random();

        public static double GetSRand()
        {
            return _rand.NextDouble();
        }

        public static int GetRand(int max)
        {
            return _rand.Next(max);
        }

        // Возвращает значения, которые могут принимать веса нейронной сети
        public static int GetWeight()
        {
            return GetRand(10) - 6;
        }
    }
}