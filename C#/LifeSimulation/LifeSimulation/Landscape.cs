using System;
using LifeSimulation.Training;

namespace LifeSimulation
{
    public class Landscape
    {
        public const int MaxDirection = 4;

        public const int HerbivorePlane = 0;
        public const int CarnivorePlane = 1;
        public const int PlantPlane = 2;

        public const int MaxGrid = 30;
        public const int MaxAgents = 36;
        public const int MaxPlants = 35;

        private readonly Agent[][,] _landscape = { new Agent[MaxGrid, MaxGrid], new Agent[MaxGrid, MaxGrid], new Agent[MaxGrid, MaxGrid] };

        public Agent[] Agents = new Agent[MaxAgents];
        public Agent[] Plants = new Agent[MaxPlants];

        public Statistics Statistics = new Statistics();

        /// <summary>
        /// Направления движения в координатной сетке
        /// </summary>
        private static readonly Offset[] Offsets = { new Offset(-1, 0), new Offset(1, 0), new Offset(0, 1), new Offset(0, -1) };

        // Смещения координат для суммирования объектов в поле зрения агента
        private static readonly Offset[] NorthFront = { new Offset(-2, -2), new Offset(-2, -1), new Offset(-2, 0), new Offset(-2, 1), new Offset(-2, 2) };
        private static readonly Offset[] NorthLeft = { new Offset(0, -2), new Offset(-1, -2) };
        private static readonly Offset[] NorthRight = { new Offset(0, 2), new Offset(-1, 2) };
        private static readonly Offset[] NorthProx = { new Offset(0, -1), new Offset(-1, -1), new Offset(-1, 0), new Offset(-1, 1), new Offset(0, 1) };

        private static readonly Offset[] WestFront = { new Offset(2, -2), new Offset(1, -2), new Offset(0, -2), new Offset(-1, -2), new Offset(-2, -2) };
        private static readonly Offset[] WestLeft = { new Offset(2, 0), new Offset(2, -1) };
        private static readonly Offset[] WestRight = { new Offset(-2, 0), new Offset(-2, -1) };
        private static readonly Offset[] WestProx = { new Offset(1, 0), new Offset(1, -1), new Offset(0, -1), new Offset(-1, -1), new Offset(-1, 0) };

        private static readonly Offset[][] NorthOffsets = { NorthFront, NorthLeft, NorthRight, NorthProx };
        private static readonly Offset[][] WestOffsets = { WestFront, WestLeft, WestRight, WestProx };


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
                AddPlantOnPlane(newPlant);
            }
        }

        public void RemovePlantFromPlane(Agent plant)
        {
            _landscape[PlantPlane][plant.Location.Y, plant.Location.X] = null;
        }

        public void AddPlantOnPlane(Agent plant)
        {
            var agentLocation = FindEmptySpot(PlantPlane);
            plant.Location = agentLocation;
            _landscape[PlantPlane][agentLocation.Y, agentLocation.X] = plant;
        }

        private void FindEmptySpotAndFill(Agent agent)
        {
            agent.Location = FindEmptySpot((int)agent.Type);

            agent.Direction = (Direction)Rand.GetRand(MaxDirection);
            SetAgentInPosition(agent);
        }

        private Location FindEmptySpot(int planeIndex)
        {
            int x = 0;
            int y = 0;
            do
            {
                x = Rand.GetRand(MaxGrid);
                y = Rand.GetRand(MaxGrid);
            } while (_landscape[planeIndex][y, x] != null);

            return new Location(x, y);
        }

        public Agent FindPlant(int x, int y)
        {
            return _landscape[PlantPlane][y, x];
        }

        public Agent FindHerbivore(int x, int y)
        {
            return _landscape[HerbivorePlane][y, x];
        }

        public void RemoveAgentFromPlane(Agent agent)
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
            _landscape[PlantPlane][plant.Location.Y, plant.Location.X] = plant;
        }

        public void Move(Agent agent)
        {
            // Удаляем агента со старого места
            RemoveAgentFromPlane(agent);

            // Обновляем координаты агента
            var direction = (int)agent.Direction;
            var offset = Offsets[direction];
            agent.Location.X = Clip(agent.Location.X + offset.Dx);
            agent.Location.Y = Clip(agent.Location.Y + offset.Dy);
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

        private bool ChooseObject(int plane, Location location, Offset[] offsets, int neg, out int ox, out int oy)
        {
            var ax = location.X;
            var ay = location.Y;

            foreach (var offset in offsets)
            {
                var xOffset = Clip(ax + (offset.Dx * neg));
                var yOffset = Clip(ay + (offset.Dy * neg));

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
                plane = Landscape.HerbivorePlane;
            }
            else
            {
                if (agent.Type == AgentType.Herbivore)
                {
                    plane = Landscape.PlantPlane;
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

        /// <summary>
        /// Убиваем агента
        /// (Пришла смерть или агента съели)
        /// </summary>
        public void KillAgent(Agent agent)
        {
            Statistics.UpdateMaxAge(agent);

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
            RemoveAgentFromPlane(agent);
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

            Statistics.UpdateMaxGen(child);
            Statistics.AgentTypeCounts[child.Type]++;
            Statistics.AgentTypeReproductions[child.Type]++;
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

        private void PerceptAll(Agent agent, Offset[][] offsets, int neg)
        {
            Percept(agent, SensorInputOffsets.HerbivoreFront, offsets[0], neg);
            Percept(agent, SensorInputOffsets.HerbivoreLeft, offsets[1], neg);
            Percept(agent, SensorInputOffsets.HerbivoreRight, offsets[2], neg);
            Percept(agent, SensorInputOffsets.HerbivoreProximity, offsets[3], neg);
        }

        private void Percept(Agent agent, SensorInputOffsets sensorInputOffset, Offset[] offsets, int neg)
        {
            var agentLocation = agent.Location;
            var inputOffset = (int)sensorInputOffset;

            for (int planeIndex = 0; planeIndex < _landscape.Length; planeIndex++)
            {
                var plane = _landscape[planeIndex];
                agent.Inputs[inputOffset + planeIndex] = 0;

                foreach (var offset in offsets)
                {
                    var xoff = Clip(agentLocation.X + (offset.Dx*neg));
                    var yoff = Clip(agentLocation.Y + (offset.Dy*neg));

                    // Если в полученной точке что-то есть, то увеличиваем счетчик входов
                    var agentOnPlane = plane[yoff, xoff];
                    if (agentOnPlane != null && agentOnPlane != agent)
                    {
                        agent.Inputs[inputOffset + planeIndex]++;
                    }
                }
            }
        }
    }
}