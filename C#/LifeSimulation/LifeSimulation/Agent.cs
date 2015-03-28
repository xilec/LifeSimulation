using System;

namespace LifeSimulation
{
    public class Agent
    {
        private const int MaxFoodEnergy = 120;
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
        public int[] BiasO = new int[MaxOutputs];
        public int[] Outputs = new int[MaxOutputs];
        public AgentAction LastAction;

        public void Eat()
        {
            Energy += Type == AgentType.Herbivore ? MaxFoodEnergy : MaxFoodEnergy*2;

            if (Energy > MaxEnergy)
            {
                Energy = MaxEnergy;
            }
        }

        public void Turn(AgentAction action)
        {
            // В зависимости от направления поворота агента вычисляем новое направление движения
            switch (Direction)
            {
                case Direction.North:
                    Direction = action == AgentAction.TurnLeft ? Direction.West : Direction.East;
                    break;
                case Direction.South:
                    Direction = action == AgentAction.TurnLeft ? Direction.East : Direction.West;
                    break;
                case Direction.West:
                    Direction = action == AgentAction.TurnLeft ? Direction.North : Direction.South;
                    break;
                case Direction.East:
                    Direction = action == AgentAction.TurnLeft ? Direction.South : Direction.North;
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
            result.BiasO = (int[])BiasO.Clone();
            result.Outputs = (int[])Outputs.Clone();

            return result;
        }

        public static Agent CreatePlant()
        {
            return new Agent();
        }

        public AgentAction MakeDecision()
        {
            // Вычисление в сети
            for (int outIndex = 0; outIndex < MaxOutputs; outIndex++)
            {
                // Инициализация входной ячейки сложением
                Outputs[outIndex] = BiasO[outIndex];

                // Перемножаем значения на входе выходной ячейки на соответствующие веса
                for (int inIndex = 0; inIndex < MaxInputs; inIndex++)
                {
                    Outputs[outIndex] += (Inputs[inIndex]*WeightOI[(outIndex*MaxInputs) + inIndex]);
                }
            }
            var largest = Int32.MinValue;
            var winnerOutput = Int32.MinValue;

            // Выбор ячейки с максимальным значением (победитель получает все)
            for (int outIndex = 0; outIndex < MaxOutputs; outIndex++)
            {
                if (Outputs[outIndex] >= largest)
                {
                    largest = Outputs[outIndex];
                    winnerOutput = outIndex;
                }
            }
            var action = (AgentAction)winnerOutput;
            LastAction = action;
            return action;
        }
    }
}