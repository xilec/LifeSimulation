using System;
using LifeSimulation.Training;

namespace LifeSimulation
{
    public class Landscape
    {
        public const int MaxDirection = 4;

        public const int HERB_PLANE = 0;
        public const int CARN_PLANE = 1;
        public const int PLANT_PLANE = 2;

        public const int MaxGrid = 30;
        public const int MaxAgents = 36;
        public const int MaxPlants = 35;

        private readonly Agent[][,] _landscape = { new Agent[MaxGrid, MaxGrid], new Agent[MaxGrid, MaxGrid], new Agent[MaxGrid, MaxGrid] };

        public Agent[] Agents = new Agent[MaxAgents];
        public Agent[] Plants = new Agent[MaxPlants];

        public readonly Statistics Statistics = new Statistics();

        /// <summary>
        /// Направления движения в координатной сетке
        /// </summary>
        /// <remarks>
        /// Первая координата смещения - y, вторая - x
        /// </remarks>
        private static readonly int[][] Offsets = { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } };

        // Смещения координат для суммирования объектов в поле зрения агента
        private static int[][] NorthFront = { new[] { -2, -2 }, new[] { -2, -1 }, new[] { -2, 0 }, new[] { -2, 1 }, new[] { -2, 2 } };
        private static int[][] NorthLeft = { new[] { 0, -2 }, new[] { -1, -2 } };
        private static int[][] NorthRight = { new[] { 0, 2 }, new[] { -1, 2 } };
        private static int[][] NorthProx = { new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 }, new[] { 0, 1 } };

        private static int[][] WestFront = { new[] { 2, -2 }, new[] { 1, -2 }, new[] { 0, -2 }, new[] { -1, -2 }, new[] { -2, -2 } };
        private static int[][] WestLeft = { new[] { 2, 0 }, new[] { 2, -1 } };
        private static int[][] WestRight = { new[] { -2, 0 }, new[] { -2, -1 } };
        private static int[][] WestProx = { new[] { 1, 0 }, new[] { 1, -1 }, new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 } };

        private static int[][][] NorthOffsets = { NorthFront, NorthLeft, NorthRight, NorthProx };
        private static int[][][] WestOffsets = { WestFront, WestLeft, WestRight, WestProx };


        private Landscape()
        {
        }

        private void Init()
        {
            InitPlants();
            InitAgents();
        }

        public static Landscape Create()
        {
            var landscape = new Landscape();
            landscape.Init();

            return landscape;
        }

        public static Landscape CreateTest()
        {
            return new Landscape();
        }

        public int GetRowsCount()
        {
            return MaxGrid;
        }

        public int GetColumnsCount()
        {
            return MaxGrid;
        }

        private void InitAgents()
        {
            for (int agentIndex = 0; agentIndex < MaxAgents; agentIndex++)
            {
                var newAgentType = agentIndex < MaxAgents / 2 ? AgentType.Herbivore : AgentType.Carnivore;
                var newAgent = TrainingCamp.EducateAgent(newAgentType);
                Agents[agentIndex] = newAgent;
                AddAgent(newAgent);
            }
        }

        private void AddAgent(Agent agent)
        {
            Statistics.AgentTypeCounts[agent.Type]++;
            FindEmptySpotAndFill(agent);
        }

        private void InitPlants()
        {
            for (int plantIndex = 0; plantIndex < MaxPlants; plantIndex++)
            {
                var newPlant = Agent.CreatePlant();
                Plants[plantIndex] = newPlant;
                AddPlant(newPlant);
            }
        }

        public void RemovePlant(Agent plant)
        {
            _landscape[PLANT_PLANE][plant.Location.Y, plant.Location.X] = null;
        }

        /// <summary>
        /// Add plant in random place of landscape
        /// </summary>
        /// <remarks>
        /// In original codes named GrowPlant
        /// </remarks>
        public void AddPlant(Agent plant)
        {
            while (true)
            {
                var x = Rand.GetRand(MaxGrid);
                var y = Rand.GetRand(MaxGrid);

                if (_landscape[PLANT_PLANE][y, x] == null)
                {
                    plant.Location.X = x;
                    plant.Location.Y = y;
                    _landscape[PLANT_PLANE][y, x] = plant;

                    break;
                }
            }
        }

        private void FindEmptySpotAndFill(Agent agent)
        {
            agent.Location.X = -1;
            agent.Location.Y = -1;

            do
            {
                agent.Location.X = Rand.GetRand(MaxGrid);
                agent.Location.Y = Rand.GetRand(MaxGrid);
            } while (_landscape[(int)agent.Type][agent.Location.Y, agent.Location.X] != null);

            agent.Direction = (Direction)Rand.GetRand(MaxDirection);
            SetAgentInPosition(agent);
        }

        /// <summary>
        /// Find plant in specified position
        /// </summary>
        /// <returns>
        /// Return finded plant, otherwise - null
        /// </returns>
        public Agent FindPlant(int x, int y)
        {
            return _landscape[PLANT_PLANE][y, x];
        }

        /// <summary>
        /// Find plant in specified position
        /// </summary>
        /// <returns>
        /// Return finded plant, otherwise - null
        /// </returns>
        public Agent FindHerbivore(int x, int y)
        {
            return _landscape[HERB_PLANE][y, x];
        }

        public void RemoveAgent(Agent agent)
        {
            var type = (int)agent.Type;
            _landscape[type][agent.Location.Y, agent.Location.X] = null;
        }

        public void SetAgentInPosition(Agent agent)
        {
            // Помещаем агента в новое место
            var type = (int)agent.Type;
            _landscape[type][agent.Location.Y, agent.Location.X] = agent;
        }

        public void SetPlantToPosition(Agent plant)
        {
            _landscape[PLANT_PLANE][plant.Location.Y, plant.Location.X] = plant;
        }

        public void Move(Agent agent)
        {
            // Удаляем агента со старого места
            RemoveAgent(agent);

            // Обновляем координаты агента
            var direction = (int)agent.Direction;
            agent.Location.X = Clip(agent.Location.X + Offsets[direction][1]);
            agent.Location.Y = Clip(agent.Location.Y + Offsets[direction][0]);
            SetAgentInPosition(agent);
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
                newPosition = position % MaxGrid;
            }

            return newPosition;
        }

        private bool ChooseObject(int plane, Location location, int[][] offsets, int neg, out int ox, out int oy)
        {
            var ax = location.X;
            var ay = location.Y;

            foreach (var offset in offsets)
            {
                var xOffset = Clip(ax + (offset[1] * neg));
                var yOffset = Clip(ay + (offset[0] * neg));

                // Если объект найден, возвращаем его индекс
                if (_landscape[plane][yOffset, xOffset] != null)
                {
                    ox = xOffset;
                    oy = yOffset;

                    return true;
                }
            }

            ox = -1;
            oy = -1;
            return false;
        }

        internal bool ChooseObject(Agent agent, out int ox, out int oy)
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

            var location = agent.Location;

            // Выбираем съедаемый объект в зависимости от направления агента
            bool isObjectChoosen;
            switch (agent.Direction)
            {
                case Direction.North:
                    isObjectChoosen = ChooseObject(plane, location, NorthProx, 1, out ox, out oy);
                    break;
                case Direction.South:
                    isObjectChoosen = ChooseObject(plane, location, NorthProx, -1, out ox, out oy);
                    break;
                case Direction.West:
                    isObjectChoosen = ChooseObject(plane, location, WestProx, 1, out ox, out oy);
                    break;
                case Direction.East:
                    isObjectChoosen = ChooseObject(plane, location, WestProx, -1, out ox, out oy);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return isObjectChoosen;
        }


        public void KillAgent(Agent agent)
        {
            // Пришла смерть (или агента съели)
            Statistics.CheckMaxAge(agent);

            int agentIndex = 0;
            for (; agentIndex < Agents.Length; agentIndex++)
            {
                if (Agents[agentIndex] == agent)
                {
                    break;
                }
            }

            if (agentIndex == Agents.Length)
            {
                throw new Exception("Agent no found in list");
            }

            Agents[agentIndex] = null;
            RemoveAgent(agent);
            Statistics.AgentTypeCounts[agent.Type]--;
            Statistics.AgentTypeDeathes[agent.Type]++;
        }

        public void ReproduceAgent(Agent agent)
        {
            // Не даем агенту одного типа занять более половины дотупных ячеек
            if (Statistics.AgentTypeCounts[agent.Type] >= MaxAgents / 2)
            {
                return;
            }

            // Найти пустое место и скопировать агента. При этом происходит мутация одного веса или смещение  в нейронной сети агента
            var emptyAgentIndex = 0;
            for (; emptyAgentIndex < Agents.Length; emptyAgentIndex++)
            {
                if (Agents[emptyAgentIndex] == null)
                {
                    break;
                }
            }

            if (emptyAgentIndex == Agents.Length)
            {
                return;
            }

            var child = agent.BornChild();
            Agents[emptyAgentIndex] = child;
            FindEmptySpotAndFill(child);

            Statistics.CheckMaxGen(child);
            Statistics.AgentTypeCounts[child.Type]++;
            Statistics.AgentTypeReproductions[child.Type]++;
        }

        private void Percept(Agent agent, SensorInputOffsets sensorInputOffset, int[][] offsets, int neg)
        {
            var agentLocation = agent.Location;
            var inputOffset = (int)sensorInputOffset;

            for (int planeIndex = 0; planeIndex < _landscape.Length; planeIndex++)
            {
                var plane = _landscape[planeIndex];
                agent.Inputs[inputOffset + planeIndex] = 0;

                foreach (var offset in offsets)
                {
                    var xoff = Clip(agentLocation.X + (offset[1]*neg));
                    var yoff = Clip(agentLocation.Y + (offset[0]*neg));

                    // Если в полученной точке что-то есть, то увеличиваем счетчик входов
                    var agentOnPlane = plane[yoff, xoff];
                    if (agentOnPlane != null && agentOnPlane != agent)
                    {
                        agent.Inputs[inputOffset + planeIndex]++;
                    }
                }
            }
        }

        public void UpdatePerception(Agent agent)
        {
            switch (agent.Direction)
            {
                case Direction.North:
                    PerceptAll(agent, NorthOffsets, 1);
                    break;
                case Direction.South:
                    PerceptAll(agent, NorthOffsets, -1);
                    break;
                case Direction.West:
                    PerceptAll(agent, WestOffsets, 1);
                    break;
                case Direction.East:
                    PerceptAll(agent, WestOffsets, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PerceptAll(Agent agent, int[][][] offsets, int neg)
        {
            Percept(agent, SensorInputOffsets.HERB_FRONT, offsets[0], neg);
            Percept(agent, SensorInputOffsets.HERB_LEFT, offsets[1], neg);
            Percept(agent, SensorInputOffsets.HERB_RIGTH, offsets[2], neg);
            Percept(agent, SensorInputOffsets.HERB_PROXIMITY, offsets[3], neg);
        }
    }
}