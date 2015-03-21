using System;

namespace LifeSimulation
{
    public class Agent
    {
        private const int MaxFoodEnergy = 50;
        public const int MaxEnergy = 200;

        public const int MaxInputs = 12;
        public const int MaxOutputs = 4;

        private Agent()
        {
        }

        public Agent(AgentType type)
        {
            Type = type;
            Energy = MaxEnergy / 2;
            Age = 0;
            Generation = 1;

            for (int i = 0; i < MaxInputs * MaxOutputs; i++)
            {
                WeightOI[i] = Helpers.GetWeight();
            }

            for (int i = 0; i < MaxOutputs; i++)
            {
                Biaso[i] = Helpers.GetWeight();
            }
        }

        public readonly AgentType Type;
        public int Energy;
        public int Parent;
        public int Age;
        public int Generation;
        public Location Location;
        public Direction Direction;
        public int[] Inputs = new int[MaxInputs];
        public int[] WeightOI = new int[MaxInputs * MaxOutputs];
        public int[] Biaso = new int[MaxOutputs];
        public int[] Actions = new int[MaxOutputs];

        public void Eat()
        {
            Energy += Type == AgentType.Herbivore ? MaxFoodEnergy : MaxFoodEnergy*2;

            if (Energy > MaxEnergy)
            {
                Energy = MaxEnergy;
            }
        }

        public Agent DeepClone()
        {
            var result = (Agent)MemberwiseClone();
            result.Location = Location.DeepClone();
            result.Inputs = (int[])Inputs.Clone();
            result.WeightOI = (int[])WeightOI.Clone();
            result.Biaso = (int[])Biaso.Clone();
            result.Actions = (int[])Actions.Clone();

            return result;
        }

        public static Agent CreatePlant()
        {
            return new Agent();
        }
    }


}