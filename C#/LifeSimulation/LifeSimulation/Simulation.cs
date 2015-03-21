using System;
using System.Collections.Generic;

namespace LifeSimulation
{
    public static class Simulation
    {
        public const int MaxSteps = 2000;

        public const int MaxAgents = 36;
        public const int MaxPlants = 35;

        public const int SeedPopulation = 0;
        public const int MaxEnergy = 200;
        public const int MaxDirection = 4;

        private const int MaxFoodEnergy = 50;
        public const double ReproduceEnergyFactor = 0.8;

        public const int MaxInputs = 12;
        public const int MaxOutputs = 4;

        public const int HERB_PLANE = 0;
        public const int CARN_PLANE = 1;
        public const int PLANT_PLANE = 2;

        public const int MaxGrid = 30;

        public static Agent[][,] Landscape = { new Agent[MaxGrid, MaxGrid], new Agent[MaxGrid, MaxGrid], new Agent[MaxGrid, MaxGrid] };

        public static Agent[] Agents = new Agent[MaxAgents];
        public static Agent[] Plants = new Agent[MaxAgents];

        // Смещения координат для суммирования объектов в поле зрения агента
        public static int[][] NorthFront = { new[] { -2, -2 }, new[] { -2, -1 }, new[] { -2, 0 }, new[] { -2, 1 }, new[] { -2, 2 } };
        public static int[][] NorthLeft = { new[] { 0, -2 }, new[] { -1, -2 } };
        public static int[][] NorthRight = { new[] { 0, 2 }, new[] { -1, 2 } };
        public static int[][] NorthProx = { new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 }, new[] { 0, 1 } };

        public static int[][] WestFront = { new[] { 2, -2 }, new[] { 1, -2 }, new[] { 0, -2 }, new[] { -1, -2 }, new[] { -2, -2 } };
        public static int[][] WestLeft = { new[] { 2, 0 }, new[] { 2, -1 } };
        public static int[][] WestRight = { new[] { -2, 0 }, new[] { -2, -1 } };
        public static int[][] WestProx = { new[] { 1, 0 }, new[] { 1, -1 }, new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 } };

        private static Agent _bestAgent = new Agent();
        private static Dictionary<AgentType, int> _agentTypeCounts;

        public static void Init()
        {
            _agentTypeCounts = new Dictionary<AgentType, int>
            {
                {AgentType.Carnivore, 0},
                {AgentType.Herbivore, 0},
            };

            // Инициализация измерения растений
            for (int plantCount = 0; plantCount < MaxPlants; plantCount++)
            {
                GrowPlant(plantCount);
            }

            if (SeedPopulation == 0)
            {
                for (int agentCount = 0; agentCount < MaxAgents; agentCount++)
                {
                    Agents[agentCount].Type = agentCount < (MaxAgents/2) ? AgentType.Herbivore : AgentType.Carnivore;

                    InitAgent(Agents[agentCount]);
                }
            }
        }

        private static void GrowPlant(int plantIndex)
        {
            while (true)
            {
                var x = Helpers.GetRand(MaxGrid);
                var y = Helpers.GetRand(MaxGrid);


                if (Landscape[PLANT_PLANE][y, x] == null)
                {
                    var plant = Plants[plantIndex];
                    plant.Location.X = x;
                    plant.Location.Y = y;
                    Landscape[PLANT_PLANE][y, x] = plant;

                    break;
                }
            }
        }

        private static void InitAgent(Agent agent)
        {
            agent.Energy = MaxEnergy/2;
            agent.Age = 0;
            agent.Generation = 1;
            _agentTypeCounts[agent.Type]++;

            FindEmptySpot(agent);

            for (int i = 0; i < MaxInputs * MaxOutputs; i++)
            {
                agent.WeightOI[i] = Helpers.GetWeight();
            }

            for (int i = 0; i < MaxOutputs; i++)
            {
                agent.Biaso[i] = Helpers.GetWeight();
            }
        }

        private static void FindEmptySpot(Agent agent)
        {
            agent.Location.X = -1;
            agent.Location.Y = -1;

            do
            {
                agent.Location.X = Helpers.GetRand(MaxGrid);
                agent.Location.Y = Helpers.GetRand(MaxGrid);
            } while (Landscape[(int) agent.Type][agent.Location.Y, agent.Location.X] == null);

            agent.Direction = (Direction)Helpers.GetRand(MaxDirection);
            Landscape[(int)agent.Type][agent.Location.Y, agent.Location.X] = agent;
        }

        public static void Simulate()
        {
            var agentTypes = new[] {AgentType.Herbivore, AgentType.Carnivore};
            foreach (var type in agentTypes)
            {
                for (int i = 0; i < MaxAgents; i++)
                {
                    if (Agents[i].Type == type)
                    {
                        SimulateAgent(Agents[i]);
                    }
                }
            }
        }

        private static void SimulateAgent(Agent agent)
        {
            var x = agent.Location.X;
            var y = agent.Location.Y;

            // Вычисление значений на входе в нейтронную сеть
            switch (agent.Direction)
            {
                case Direction.North:
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_FRONT], NorthFront, 1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_LEFT], NorthFront, 1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_RIGTH], NorthFront, 1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_PROXIMITY], NorthFront, 1);
                    break;
                case Direction.South:
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_FRONT], NorthFront, -1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_LEFT], NorthFront, -1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_RIGTH], NorthFront, -1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_PROXIMITY], NorthFront, -1);
                    break;
                case Direction.West:
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_FRONT], WestFront, 1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_LEFT], WestFront, 1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_RIGTH], WestFront, 1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_PROXIMITY], WestFront, 1);
                    break;
                case Direction.East:
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_FRONT], WestFront, -1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_LEFT], WestFront, -1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_RIGTH], WestFront, -1);
                    Percept(x, y, ref agent.Inputs[(int)SensorInputs.HERB_PROXIMITY], WestFront, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Вычисление в сети
            for (int outIndex = 0; outIndex < MaxOutputs; outIndex++)
            {
                // Инициализация входной ячейки сложением
                agent.Actions[outIndex] = agent.Biaso[outIndex];
                
                // Перемножаем значения на входе выходной ячейки на соответствующие веса
                for (int inIndex = 0; inIndex < MaxInputs; inIndex++)
                {
                    agent.Inputs[inIndex] += (agent.Inputs[inIndex]*agent.WeightOI[(outIndex*MaxInputs) + inIndex]);
                }
            }
            var largest = -9;
            var winner = -1;

            // Выбор ячейки с максимальным значением (победитель получает все)
            for (int outIndex = 0; outIndex < MaxOutputs; outIndex++)
            {
                if (agent.Actions[outIndex] >= largest)
                {
                    largest = agent.Actions[outIndex];
                    winner = outIndex;
                }
            }

            // Выполнение выбранного действия
            var winnerAction = (AgentActions)winner;
            switch (winnerAction)
            {
                case AgentActions.TurnLeft:
                case AgentActions.TurnRight:
                    Turn(winnerAction, agent);
                    break;
                case AgentActions.Move:
                    Move(agent);
                    break;
                case AgentActions.Eat:
                    Eat(agent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Вычитаем "потраченную" энергию
            agent.Energy -= agent.Type == AgentType.Herbivore ? 2 : 1;

            // Если энергия агента меньше или равна нулю - агент умирает
            // В противом случае проверяем, чне является ли этот агент самым старым

            if (agent.Energy <= 0)
            {
                KillAgent(agent);
            }
            else
            {
                agent.Age++;
                if (agent.Age > AgentsMaxAge[agent.Type].Age)
                {
                    AgentsMaxAge[agent.Type] = agent.DeepClone();
                }
            }
        }

        public static readonly Dictionary<AgentType, Agent> AgentsMaxAge = new Dictionary<AgentType, Agent>
        {
            {AgentType.Herbivore, null},
            {AgentType.Carnivore, null}
        };

        private static void KillAgent(Agent agent)
        {
            throw new NotImplementedException();
        }

        private static void Eat(Agent agent)
        {
            // Сначала определяем слой, объект в котором будет съеден
            int plane = 0;
            if (agent.Type == AgentType.Carnivore)
            {
                plane = HERB_PLANE;
            }
            else
            {
                if (agent.Type == AgentType.Herbivore)
                {
                    plane = PLANT_PLANE;
                }
            }

            var ax = agent.Location.X;
            var ay = agent.Location.Y;

            // Выбираем съедаемый объект в зависимости от направления агента
            int ret = 0;
            int ox;
            int oy;
            switch (agent.Direction)
            {
                case Direction.North:
                    ret = ChooseObject(plane, ax, ay, NorthProx, 1, out ox, out oy);
                    break;
                case Direction.South:
                    ret = ChooseObject(plane, ax, ay, NorthProx, -1, out ox, out oy);
                    break;
                case Direction.West:
                    ret = ChooseObject(plane, ax, ay, WestProx, 1, out ox, out oy);
                    break;
                case Direction.East:
                    ret = ChooseObject(plane, ax, ay, WestProx, -1, out ox, out oy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Объект нашли - съедаем его!
            if (ret != 0)
            {
                if (plane == PLANT_PLANE)
                {
                    // Найти растение по его позиции
                    int i;
                    for (i = 0; i < MaxPlants; i++)
                    {
                        if ((Plants[i].Location.X == ox) && (Plants[i].Location.Y == oy))
                        {
                            break;
                        }
                    }

                    // Если растение найдено, то удаляем его и сажаем в другом месте новое
                    if (i < MaxPlants)
                    {
                        agent.Energy += MaxFoodEnergy;

                        if (agent.Energy > MaxEnergy)
                        {
                            agent.Energy = MaxEnergy;
                        }

                        Landscape[PLANT_PLANE][oy, ox] = null;
                        
                        GrowPlant(i);
                    }
                }
                else
                {
                    if (plane == HERB_PLANE)
                    {
                        // Найти травоядное в списке агентов (по его позиции)
                        int i;
                        for (i = 0; i < MaxAgents; i++)
                        {
                            if ((Agents[i].Location.X == ox) && (Agents[i].Location.Y == oy))
                            {
                                break;
                            }
                        }

                        // Если нашли, то удаляем агента
                        if (i < MaxAgents)
                        {
                            agent.Energy += (MaxFoodEnergy*2);
                            if (agent.Energy > MaxEnergy)
                            {
                                agent.Energy = MaxEnergy;
                            }

                            KillAgent(Agents[i]);
                        }
                    }
                }
            }

            // Если агент имеет достаточно энергии для размножения, то позволяем ему сделать это
            if (agent.Energy > (ReproduceEnergyFactor * MaxEnergy))
            {
                ReproduceAgent(agent);
            }
        }

        private static void ReproduceAgent(Agent agent)
        {
            throw new NotImplementedException();
        }

        private static int ChooseObject(int plane, int ax, int ay, int[][] offsets, int neg, out int ox, out int oy)
        {
            // TODO разобраться с основным смыслом метода

            throw new NotImplementedException();
        }

        /// <summary>
        /// Направления движения в координатной сетке
        /// </summary>
        /// <remarks>
        /// Первая координата смещения - y, вторая - x
        /// </remarks>
        private static readonly int[][] Offsets = { new[] { -1, 0 }, new []{1, 0}, new []{0, 1}, new []{0, -1}};

        private static void Move(Agent agent)
        {
            // Удаляем агента со старого места
            var type = (int)agent.Type;
            Landscape[type][agent.Location.Y, agent.Location.X] = null;

            // Обновляем координаты агента
            var direction = (int)agent.Direction;
            agent.Location.X = Clip(agent.Location.X + Offsets[direction][1]);
            agent.Location.Y = Clip(agent.Location.Y + Offsets[direction][0]);

            // Помещаем агента в новое место
            Landscape[type][agent.Location.Y, agent.Location.X] = agent;
        }

        private static int Clip(int position)
        {
            int newPosition;
            if (position <= MaxGrid - 1)
            {
                if (position < 0)
                {
                    newPosition = MaxGrid + position;
                }
                else
                {
                    newPosition = position;
                }
            }
            else
            {
                newPosition = position%MaxGrid;
            }

            return newPosition;
        }

        private static void Turn(AgentActions action, Agent agent)
        {
            // В зависимости от направления поворота агента вычисляем новое направление движения
            switch (agent.Direction)
            {
                case Direction.North:
                    agent.Direction = action == AgentActions.TurnLeft ? Direction.West : Direction.East;
                    break;
                case Direction.South:
                    agent.Direction = action == AgentActions.TurnLeft ? Direction.East : Direction.West;
                    break;
                case Direction.West:
                    agent.Direction = action == AgentActions.TurnLeft ? Direction.North : Direction.South;
                    break;
                case Direction.East:
                    agent.Direction = action == AgentActions.TurnLeft ? Direction.South : Direction.North;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Percept(int x, int y, ref int inputs, int[][] northFront, int i1)
        {
            // TODO нужно разобраться более внимательно (в книге написана какая-то чушь с инициализацией)
            throw new NotImplementedException();
        }
    }
}