using System;

namespace LifeSimulation
{
    public static class Rand
    {
        private static Random _rand = new Random();

        public static void ReinitializeRandom(int seed)
        {
            _rand = new Random(seed);
        }

        internal static double GetSRand()
        {
            return _rand.NextDouble();
        }

        internal static int GetRand(int max)
        {
            return _rand.Next(max);
        }

        // Возвращает значения, которые могут принимать веса нейронной сети
        internal static int GetWeight()
        {
            return GetRand(10) - 6;
        }
    }
}