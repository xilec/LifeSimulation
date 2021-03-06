﻿using System;
using LifeSimulation.Training;

namespace LifeSimulation
{
    public class Landscape
    {
        public const int GridSize = 30;
        public const int MaxAgents = 36;
        public const int MaxPlants = 35;

        private const int DirectionsCount = 4;

        private const int HerbivorePlane = 0;
        private const int CarnivorePlane = 1;
        private const int PlantPlane = 2;

        private readonly Plant[,] _plantLandscape = new Plant[GridSize, GridSize];
        private readonly Agent[][,] _agentLandscapes = { new Agent[GridSize, GridSize], new Agent[GridSize, GridSize] };

        public Agent[] Agents = new Agent[MaxAgents];
        public Plant[] Plants = new Plant[MaxPlants];
        private readonly ISimulationObject[][,] _allLandscapes;

        private const double ReproduceEnergyFactor = 0.8;
        
        public readonly Statistics Statistics = new Statistics();

        /// <summary>
        /// Направления движения в координатной сетке
        /// </summary>
        private static readonly Offset[] Offsets = { new Offset(0, -1), new Offset(0, 1), new Offset(1, 0), new Offset(-1, 0) };

        // Смещения координат для суммирования объектов в поле зрения агента
        private static readonly Offset[] NorthFront = { new Offset(-2, -2), new Offset(-1, -2), new Offset(0, -2), new Offset(1, -2), new Offset(2, -2) };
        private static readonly Offset[] NorthLeft = { new Offset(-2, 0), new Offset(-2, -1) };
        private static readonly Offset[] NorthRight = { new Offset(2, 0), new Offset(2, -1) };
        private static readonly Offset[] NorthProx = { new Offset(-1, 0), new Offset(-1, -1), new Offset(0, -1), new Offset(1, -1), new Offset(1, 0) };

        private static readonly Offset[] WestFront = { new Offset(-2, 2), new Offset(-2, 1), new Offset(-2, 0), new Offset(-2, -1), new Offset(-2, -2) };
        private static readonly Offset[] WestLeft = { new Offset(0, 2), new Offset(-1, 2) };
        private static readonly Offset[] WestRight = { new Offset(0, -2), new Offset(-1, -2) };
        private static readonly Offset[] WestProx = { new Offset(0, 1), new Offset(-1, 1), new Offset(-1, 0), new Offset(-1, -1), new Offset(0, -1) };

        private static readonly Offset[][] NorthOffsets = { NorthFront, NorthLeft, NorthRight, NorthProx };
        private static readonly Offset[][] WestOffsets = { WestFront, WestLeft, WestRight, WestProx };


        private Landscape()
        {
            _allLandscapes = new ISimulationObject[][,] { _agentLandscapes[0], _agentLandscapes[1], _plantLandscape };
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

        public static Landscape CreateForTest()
        {
            return new Landscape();
        }

        public int GetRowsCount()
        {
            return GridSize;
        }

        public int GetColumnsCount()
        {
            return GridSize;
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
                var newPlant = new Plant();
                Plants[plantIndex] = newPlant;
                AddPlantOnPlane(newPlant);
            }
        }

        private void AddPlantOnPlane(Plant plant)
        {
            var agentLocation = FindEmptySpot(_plantLandscape);
            plant.Location = agentLocation;
            _plantLandscape[agentLocation.Y, agentLocation.X] = plant;
        }

        private void RemovePlantFromPlane(Plant plant)
        {
            _plantLandscape[plant.Location.Y, plant.Location.X] = null;
        }

        private void FindEmptySpotAndFill(Agent agent)
        {
            agent.Location = FindEmptySpot(_agentLandscapes[(int)agent.Type]);

            agent.Direction = (Direction)Rand.GetRand(DirectionsCount);
            SetAgentInPosition(agent);
        }

        private static Location FindEmptySpot(ISimulationObject[,] plane)
        {
            int x = 0;
            int y = 0;
            do
            {
                x = Rand.GetRand(GridSize);
                y = Rand.GetRand(GridSize);
            } while (plane[y, x] != null);

            return new Location(x, y);
        }

        private Plant FindPlant(int x, int y)
        {
            return _plantLandscape[y, x];
        }

        private Agent[,] GetHerbivorePlane()
        {
            return _agentLandscapes[HerbivorePlane];
        }

        private Agent FindHerbivore(int x, int y)
        {
            return GetHerbivorePlane()[y, x];
        }

        private void RemoveAgentFromPlane(Agent agent)
        {
            var type = (int)agent.Type;
            _agentLandscapes[type][agent.Location.Y, agent.Location.X] = null;
        }

        internal void SetAgentInPosition(Agent agent)
        {
            // Помещаем агента в новое место
            var type = (int)agent.Type;
            _agentLandscapes[type][agent.Location.Y, agent.Location.X] = agent;
        }

        internal void SetPlantToPosition(Plant plant)
        {
            _plantLandscape[plant.Location.Y, plant.Location.X] = plant;
        }

        private void Move(Agent agent)
        {
            // Удаляем агента со старого места
            RemoveAgentFromPlane(agent);

            // Обновляем координаты агента
            var direction = (int)agent.Direction;
            var offset = Offsets[direction];
            agent.Location = new Location(Clip(agent.Location.X + offset.Dx), Clip(agent.Location.Y + offset.Dy));
            SetAgentInPosition(agent);
        }

        private static int Clip(int position)
        {
            int newPosition;
            if (position <= GridSize - 1)
            {
                if (position < 0)
                {
                    newPosition = GridSize + position;
                }
                else
                {
                    newPosition = position;
                }
            }
            else
            {
                newPosition = position % GridSize;
            }

            return newPosition;
        }

        private bool ChooseVictim(ISimulationObject[,]plane, Location location, Offset[] offsets, int neg, out int ox, out int oy)
        {
            var ax = location.X;
            var ay = location.Y;

            foreach (var offset in offsets)
            {
                var xOffset = Clip(ax + (offset.Dx * neg));
                var yOffset = Clip(ay + (offset.Dy * neg));

                // Если объект найден, возвращаем его индекс
                if (plane[yOffset, xOffset] != null)
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

        private bool ChooseVictim(Agent agent, out int ox, out int oy)
        {
            // Сначала определяем слой, объект в котором будет съеден
            ISimulationObject[,] plane;
            switch (agent.Type)
            {
                case AgentType.Herbivore:
                    plane = _plantLandscape;
                    break;
                case AgentType.Carnivore:
                    plane = GetHerbivorePlane();
                    break;
                default:
                    throw new ArgumentException("Only herbivore and carnivore can eat");
            }

            var location = agent.Location;

            // Выбираем съедаемый объект в зависимости от направления агента
            bool isObjectChoosen;
            switch (agent.Direction)
            {
                case Direction.North:
                    isObjectChoosen = ChooseVictim(plane, location, NorthProx, 1, out ox, out oy);
                    break;
                case Direction.South:
                    isObjectChoosen = ChooseVictim(plane, location, NorthProx, -1, out ox, out oy);
                    break;
                case Direction.West:
                    isObjectChoosen = ChooseVictim(plane, location, WestProx, 1, out ox, out oy);
                    break;
                case Direction.East:
                    isObjectChoosen = ChooseVictim(plane, location, WestProx, -1, out ox, out oy);
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
        private void KillAgent(Agent agent)
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

        private void ReproduceAgent(Agent agent)
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

        internal void UpdatePerception(Agent agent)
        {
            switch (agent.Direction)
            {
                case Direction.North:
                    UpdatePerceptionForAllAreas(agent, NorthOffsets, 1);
                    break;
                case Direction.South:
                    UpdatePerceptionForAllAreas(agent, NorthOffsets, -1);
                    break;
                case Direction.West:
                    UpdatePerceptionForAllAreas(agent, WestOffsets, 1);
                    break;
                case Direction.East:
                    UpdatePerceptionForAllAreas(agent, WestOffsets, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdatePerceptionForAllAreas(Agent agent, Offset[][] offsets, int neg)
        {
            UpdatePerceptionForArea(agent, SensorInputOffsets.HerbivoreFront, offsets[0], neg);
            UpdatePerceptionForArea(agent, SensorInputOffsets.HerbivoreLeft, offsets[1], neg);
            UpdatePerceptionForArea(agent, SensorInputOffsets.HerbivoreRight, offsets[2], neg);
            UpdatePerceptionForArea(agent, SensorInputOffsets.HerbivoreProximity, offsets[3], neg);
        }

        private void UpdatePerceptionForArea(Agent agent, SensorInputOffsets sensorInputOffset, Offset[] preceptionArea, int neg)
        {
            var agentLocation = agent.Location;
            var inputOffset = (int)sensorInputOffset;

            var allLandscapes = _allLandscapes;
            for (int planeIndex = 0; planeIndex < allLandscapes.Length; planeIndex++)
            {
                var plane = allLandscapes[planeIndex];
                agent.Inputs[inputOffset + planeIndex] = 0;

                foreach (var offset in preceptionArea)
                {
                    var x = Clip(agentLocation.X + (offset.Dx*neg));
                    var y = Clip(agentLocation.Y + (offset.Dy*neg));

                    // Если в полученной точке что-то есть, то увеличиваем счетчик входов
                    var agentOnPlane = plane[y, x];
                    if (agentOnPlane != null && agentOnPlane != agent)
                    {
                        agent.Inputs[inputOffset + planeIndex]++;
                    }
                }
            }
        }

        private void Eat(Agent agent)
        {
            int ox;
            int oy;
            var isObjectChoosen = ChooseVictim(agent, out ox, out oy);

            // Объект нашли - съедаем его!
            if (!isObjectChoosen)
            {
                return;
            }

            switch (agent.Type)
            {
                case AgentType.Herbivore:
                    var findedPlant = FindPlant(ox, oy);

                    // Если растение найдено, то удаляем его и сажаем в другом месте новое
                    if (findedPlant != null)
                    {
                        agent.Eat();

                        RemovePlantFromPlane(findedPlant);
                        AddPlantOnPlane(findedPlant);
                    }

                    break;

                case AgentType.Carnivore:
                    // Найти травоядное в списке агентов (по его позиции)
                    var findedHerbivore = FindHerbivore(ox, oy);

                    // Если нашли, то удаляем агента
                    if (findedHerbivore != null)
                    {
                        agent.Eat();

                        KillAgent(findedHerbivore);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal void UpdateAgentsState(Agent agent)
        {
            switch (agent.Action)
            {
                case AgentAction.TurnLeft:
                case AgentAction.TurnRight:
                    agent.Turn();
                    break;
                case AgentAction.Move:
                    Move(agent);
                    break;
                case AgentAction.Eat:
                    Eat(agent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Вычитаем "потраченную" энергию
            agent.EnergyUpdateOnTurn();

            // Если агент имеет достаточно энергии для размножения, то позволяем ему сделать это
            if (agent.Energy > ReproduceEnergyFactor * Agent.MaxEnergy)
            {
                ReproduceAgent(agent);
            }

            // Если энергия агента меньше или равна нулю - агент умирает
            // В противом случае проверяем, чне является ли этот агент самым старым
            if (agent.Energy <= 0)
            {
                KillAgent(agent);
            }
            else
            {
                agent.Age++;
                Statistics.UpdateMaxGen(agent);
            }
        }
    }
}