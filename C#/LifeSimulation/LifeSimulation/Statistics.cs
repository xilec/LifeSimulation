using System.Collections.Generic;
using Newtonsoft.Json;

namespace LifeSimulation
{
    public class Statistics
    {
        [JsonProperty]
        private readonly Dictionary<AgentType, Agent> _agentsMaxGen = new Dictionary<AgentType, Agent>();

        [JsonProperty]
        private readonly Dictionary<AgentType, Agent> _agentsMaxAge = new Dictionary<AgentType, Agent>();

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

        public readonly Dictionary<AgentType, int> AgentTypeDeathes = new Dictionary<AgentType, int>
        {
            {AgentType.Carnivore, 0},
            {AgentType.Herbivore, 0},
        };

        public void CheckMaxGen(Agent agent)
        {
            Agent maxGenAgent;
            if (!_agentsMaxGen.TryGetValue(agent.Type, out maxGenAgent))
            {
                _agentsMaxGen.Add(agent.Type, agent.DeepClone());
                return;
            }

            if (maxGenAgent.Generation < agent.Generation)
            {
                _agentsMaxGen[agent.Type] = agent.DeepClone();
            }
        }

        public Agent GetMaxGenAgent(AgentType agentType)
        {
            Agent agent = _agentsMaxGen.TryGetValue(agentType, out agent) ? agent : null;
            return agent;
        }

        public void CheckMaxAge(Agent agent)
        {
            Agent maxAgeAgent;
            if (!_agentsMaxAge.TryGetValue(agent.Type, out maxAgeAgent))
            {
                _agentsMaxAge.Add(agent.Type, agent.DeepClone());
                return;
            }

            if (agent.Age > maxAgeAgent.Age)
            {
                _agentsMaxAge[agent.Type] = agent.DeepClone();
            }
        }

        public Agent GetMaxAgeAgent(AgentType agentType)
        {
            Agent agent = _agentsMaxAge.TryGetValue(agentType, out agent) ? agent : null;
            return agent;
        }

    }
}