using System;

namespace LifeSimulation
{
    public class Simulation
    {
        public const int MaxSteps = 2000;

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

            DoAgentsAction(SimulateAgent);
        }

        private void SimulateAgent(Agent agent)
        {
            EstimateAgentsState(agent);

            Landscape.UpdateAgentsState(agent);
        }

        private void DoAgentsAction(Action<Agent> action)
        {
            var agentTypes = new[] { AgentType.Herbivore, AgentType.Carnivore };
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
                        action(agent);
                    }
                }
            }
        }

        public void EstimateState()
        {
            DoAgentsAction(EstimateAgentsState);
        }

        public void UpdateState()
        {
            DoAgentsAction(Landscape.UpdateAgentsState);
        }

        private void EstimateAgentsState(Agent agent)
        {
            // Вычисление значений на входе в нейтронную сеть
            Landscape.UpdatePerception(agent);

            // Выполнение выбранного действия
            agent.MakeDecision();
        }
    }
}