using System.Collections.Generic;

namespace LifeSimulation
{
    public class Statistics
    {
        private readonly Dictionary<AgentType, Agent> _bestAgents = new Dictionary<AgentType, Agent>();
        private readonly Dictionary<AgentType, Agent> _agentMaxGen = new Dictionary<AgentType, Agent>();

        public readonly Dictionary<AgentType, int> AgentTypeCounts = new Dictionary<AgentType, int>
        {
            {AgentType.Carnivore, 0},
            {AgentType.Herbivore, 0},
        };

        public readonly Dictionary<AgentType, int> AgentTypeReproductions = new Dictionary<AgentType, int>
        {
            {AgentType.Carnivore, 0},
            {AgentType.Herbivore, 0},
        };

        private readonly Dictionary<AgentType, Agent> _agentsMaxAge = new Dictionary<AgentType, Agent>
        {
            {AgentType.Herbivore, null},
            {AgentType.Carnivore, null}
        };


        public void CheckMaxGenAgent(Agent agent)
        {
            Agent maxGenAgent;
            if (_agentMaxGen.TryGetValue(agent.Type, out maxGenAgent))
            {
                _agentMaxGen.Add(agent.Type, agent.DeepClone());
                return;
            }

            if (maxGenAgent.Generation < agent.Generation)
            {
                _agentMaxGen[agent.Type] = agent.DeepClone();
            }
        }

        public void CheckBestAgent(Agent agent)
        {
            Agent bestAgent;
            if (!_bestAgents.TryGetValue(agent.Type, out bestAgent))
            {
                _bestAgents.Add(agent.Type, agent.DeepClone());
                return;
            }

            if (agent.Age > bestAgent.Age)
            {
                _bestAgents[agent.Type] = agent.DeepClone();
            }
        }

        public void CheckMaxAge(Agent agent)
        {
            Agent maxAgeAgent;
            if (!_agentsMaxAge.TryGetValue(agent.Type, out maxAgeAgent))
            {
                _agentsMaxAge.Add(agent.Type, agent.DeepClone());
            }

            if (agent.Age > maxAgeAgent.Age)
            {
                _agentsMaxAge[agent.Type] = agent.DeepClone();
            }
        }

    }
}