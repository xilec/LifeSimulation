using System;

namespace LifeSimulation
{
    public class Simulation
    {
        public const int MaxSteps = 2000;

        public const double ReproduceEnergyFactor = 0.8;

        // Смещения координат для суммирования объектов в поле зрения агента
        public static int[][] NorthFront = { new[] { -2, -2 }, new[] { -2, -1 }, new[] { -2, 0 }, new[] { -2, 1 }, new[] { -2, 2 } };
        public static int[][] NorthLeft = { new[] { 0, -2 }, new[] { -1, -2 } };
        public static int[][] NorthRight = { new[] { 0, 2 }, new[] { -1, 2 } };
        public static int[][] NorthProx = { new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 }, new[] { 0, 1 } };

        public static int[][] WestFront = { new[] { 2, -2 }, new[] { 1, -2 }, new[] { 0, -2 }, new[] { -1, -2 }, new[] { -2, -2 } };
        public static int[][] WestLeft = { new[] { 2, 0 }, new[] { 2, -1 } };
        public static int[][] WestRight = { new[] { -2, 0 }, new[] { -2, -1 } };
        public static int[][] WestProx = { new[] { 1, 0 }, new[] { 1, -1 }, new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 } };

        //private static Agent _bestAgent = new Agent();

        public Landscape Landscape = new Landscape();


        public void Init()
        {
        }

        public void Simulate()
        {
            var agentTypes = new[] {AgentType.Herbivore, AgentType.Carnivore};
            foreach (var type in agentTypes)
            {
                for (int i = 0; i < Landscape.MaxAgents; i++)
                {
                    var agent = Landscape.Agents[i];
                    if (agent.Type == type)
                    {
                        SimulateAgent(agent);
                    }
                }
            }
        }

        private void SimulateAgent(Agent agent)
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
            for (int outIndex = 0; outIndex < Agent.MaxOutputs; outIndex++)
            {
                // Инициализация входной ячейки сложением
                agent.Actions[outIndex] = agent.Biaso[outIndex];
                
                // Перемножаем значения на входе выходной ячейки на соответствующие веса
                for (int inIndex = 0; inIndex < Agent.MaxInputs; inIndex++)
                {
                    agent.Inputs[inIndex] += (agent.Inputs[inIndex]*agent.WeightOI[(outIndex * Agent.MaxInputs) + inIndex]);
                }
            }
            var largest = -9;
            var winner = -1;

            // Выбор ячейки с максимальным значением (победитель получает все)
            for (int outIndex = 0; outIndex < Agent.MaxOutputs; outIndex++)
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
                    Landscape.Move(agent);
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
                if (agent.Age > Landscape.AgentsMaxAge[agent.Type].Age)
                {
                    Landscape.AgentsMaxAge[agent.Type] = agent.DeepClone();
                }
            }
        }

        private void Eat(Agent agent)
        {
            // Сначала определяем слой, объект в котором будет съеден
            int plane = 0;
            if (agent.Type == AgentType.Carnivore)
            {
                plane = Landscape.HERB_PLANE;
            }
            else
            {
                if (agent.Type == AgentType.Herbivore)
                {
                    plane = Landscape.PLANT_PLANE;
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
                if (plane == Landscape.PLANT_PLANE)
                {
                    var findedPlant = Landscape.FindPlant(ox, oy);

                    // Если растение найдено, то удаляем его и сажаем в другом месте новое
                    if (findedPlant != null)
                    {
                        agent.Eat();
                        
                        Landscape.RemovePlant(findedPlant);
                        Landscape.AddPlant(findedPlant);
                    }
                }
                else
                {
                    if (plane == Landscape.HERB_PLANE)
                    {
                        // Найти травоядное в списке агентов (по его позиции)
                        var findedHerbivore = Landscape.FindHerbivore(ox, oy);

                        // Если нашли, то удаляем агента
                        if (findedHerbivore != null)
                        {
                            agent.Eat();

                            KillAgent(findedHerbivore);
                        }
                    }
                }
            }

            // Если агент имеет достаточно энергии для размножения, то позволяем ему сделать это
            if (agent.Energy > (ReproduceEnergyFactor * Agent.MaxEnergy))
            {
                ReproduceAgent(agent);
            }
        }

        private void KillAgent(Agent agent)
        {
            throw new NotImplementedException();
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