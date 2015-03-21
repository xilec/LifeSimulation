using System;

namespace LifeSimulation
{
    public static class Helpers
    {
         private static Random _rand = new Random();

        public static double GetSRand()
        {
            return _rand.NextDouble();
        }

        public static int GetRand(int x)
        {
            return (int)(GetSRand() * x);
        }

        // Возвращает значения, которые могут принимать веса нейронной сети
        public static int GetWeight()
        {
            return GetRand(9) - 1;
        }
    }
}