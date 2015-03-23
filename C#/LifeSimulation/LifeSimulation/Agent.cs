using System;

namespace LifeSimulation
{
    public class Agent
    {
        private const int MaxFoodEnergy = 50;
        public const int MaxEnergy = 200;

        public const int MaxInputs = 12;
        public const int MaxOutputs = 4;
        public const int TotalWeights = MaxInputs*MaxOutputs;

        private Agent()
        {
            Location = new Location(-1, -1);
        }

        public Agent(AgentType type)
        {
            Type = type;
            Energy = MaxEnergy / 2;
            Age = 0;
            Generation = 1;
            Location = new Location(-1, -1);
            Direction = Direction.West;

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
        public int[] WeightOI = new int[TotalWeights];
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

        public void Turn(AgentActions action)
        {
            // В зависимости от направления поворота агента вычисляем новое направление движения
            switch (Direction)
            {
                case Direction.North:
                    Direction = action == AgentActions.TurnLeft ? Direction.West : Direction.East;
                    break;
                case Direction.South:
                    Direction = action == AgentActions.TurnLeft ? Direction.East : Direction.West;
                    break;
                case Direction.West:
                    Direction = action == AgentActions.TurnLeft ? Direction.North : Direction.South;
                    break;
                case Direction.East:
                    Direction = action == AgentActions.TurnLeft ? Direction.South : Direction.North;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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