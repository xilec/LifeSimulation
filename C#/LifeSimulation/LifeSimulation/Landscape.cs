using System.Collections.Generic;

namespace LifeSimulation
{
    public class Landscape
    {
        public const int SeedPopulation = 0;

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

        public readonly Dictionary<AgentType, int> AgentTypeCounts;
        public readonly Dictionary<AgentType, Agent> AgentsMaxAge = new Dictionary<AgentType, Agent>
        {
            {AgentType.Herbivore, null},
            {AgentType.Carnivore, null}
        };

        public Landscape()
        {
            AgentTypeCounts = new Dictionary<AgentType, int>
            {
                {AgentType.Carnivore, 0},
                {AgentType.Herbivore, 0},
            };

            InitPlants();

            InitAgents();

        }

        private void InitAgents()
        {
            for (int agentCount = 0; agentCount < MaxAgents; agentCount++)
            {
                var newAgentType = agentCount < (MaxAgents/2) ? AgentType.Herbivore : AgentType.Carnivore;
                var newAgent = new Agent(newAgentType);
                Agents[agentCount] = newAgent;
                AddAgent(newAgent);
            }
        }

        private void AddAgent(Agent agent)
        {
            AgentTypeCounts[agent.Type]++;
            FindEmptySpot(agent);
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
                var x = Helpers.GetRand(MaxGrid);
                var y = Helpers.GetRand(MaxGrid);


                if (_landscape[PLANT_PLANE][y, x] == null)
                {
                    plant.Location.X = x;
                    plant.Location.Y = y;
                    _landscape[PLANT_PLANE][y, x] = plant;

                    break;
                }
            }            
        }

        private void FindEmptySpot(Agent agent)
        {
            agent.Location.X = -1;
            agent.Location.Y = -1;

            do
            {
                agent.Location.X = Helpers.GetRand(MaxGrid);
                agent.Location.Y = Helpers.GetRand(MaxGrid);
            } while (_landscape[(int)agent.Type][agent.Location.Y, agent.Location.X] == null);

            agent.Direction = (Direction)Helpers.GetRand(MaxDirection);
            _landscape[(int)agent.Type][agent.Location.Y, agent.Location.X] = agent;
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

        /// <summary>
        /// Направления движения в координатной сетке
        /// </summary>
        /// <remarks>
        /// Первая координата смещения - y, вторая - x
        /// </remarks>
        private static readonly int[][] Offsets = { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } };
        public void Move(Agent agent)
        {
            // Удаляем агента со старого места
            RemoveAgent(agent);

            // Обновляем координаты агента
            var direction = (int)agent.Direction;
            agent.Location.X = Clip(agent.Location.X + Offsets[direction][1]);
            agent.Location.Y = Clip(agent.Location.Y + Offsets[direction][0]);
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
    }
}