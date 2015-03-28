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

        public Landscape Landscape = Landscape.Create();

        public Simulation()
        {
        }

        public int ColumnsCount
        {
            get { return Landscape.GetColumnsCount(); }
        }

        public int RowsCount
        {
            get { return Landscape.GetRowsCount(); }
        }

        public void Simulate()
        {
            var agentTypes = new[] {AgentType.Herbivore, AgentType.Carnivore};
            foreach (var type in agentTypes)
            {
                for (int i = 0; i < Landscape.Agents.Length; i++)
                {
                    var agent = Landscape.Agents[i];
                    if (agent == null)
                    {
                        continue;
                    }

                    if (agent.Type == type)
                    {
                        SimulateAgent(agent);
                    }
                }
            }
        }

        private void SimulateAgent(Agent agent)
        {
            // Вычисление значений на входе в нейтронную сеть
            Landscape.UpdatePerception(agent);

            // Выполнение выбранного действия
            var winnerAction = agent.MakeDecision();
            switch (winnerAction)
            {
                case AgentAction.TurnLeft:
                case AgentAction.TurnRight:
                    agent.Turn(winnerAction);
                    break;
                case AgentAction.Move:
                    Landscape.Move(agent);
                    break;
                case AgentAction.Eat:
                    Eat(agent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Вычитаем "потраченную" энергию
            agent.Energy -= agent.Type == AgentType.Herbivore ? 2 : 1;

            // Если агент имеет достаточно энергии для размножения, то позволяем ему сделать это
            if (agent.Energy > (ReproduceEnergyFactor * Agent.MaxEnergy))
            {
                Landscape.ReproduceAgent(agent);
            }

            // Если энергия агента меньше или равна нулю - агент умирает
            // В противом случае проверяем, чне является ли этот агент самым старым
            if (agent.Energy <= 0)
            {
                Landscape.KillAgent(agent);
            }
            else
            {
                agent.Age++;
                Landscape.Statistics.CheckMaxGen(agent);
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
            bool isObjectChoosen;
            int ox;
            int oy;
            switch (agent.Direction)
            {
                case Direction.North:
                    isObjectChoosen = Landscape.ChooseObject(plane, ax, ay, NorthProx, 1, out ox, out oy);
                    break;
                case Direction.South:
                    isObjectChoosen = Landscape.ChooseObject(plane, ax, ay, NorthProx, -1, out ox, out oy);
                    break;
                case Direction.West:
                    isObjectChoosen = Landscape.ChooseObject(plane, ax, ay, WestProx, 1, out ox, out oy);
                    break;
                case Direction.East:
                    isObjectChoosen = Landscape.ChooseObject(plane, ax, ay, WestProx, -1, out ox, out oy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Объект нашли - съедаем его!
            if (isObjectChoosen)
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

                            Landscape.KillAgent(findedHerbivore);
                        }
                    }
                }
            }
        }
    }
}