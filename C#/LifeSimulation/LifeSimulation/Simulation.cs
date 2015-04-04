using System;

namespace LifeSimulation
{
    public class Simulation
    {
        public const int MaxSteps = 2000;

        public const double ReproduceEnergyFactor = 0.8;


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

            UpdateAgentsState(agent);
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
            DoAgentsAction(UpdateAgentsState);
        }

        private void EstimateAgentsState(Agent agent)
        {
            // Вычисление значений на входе в нейтронную сеть
            Landscape.UpdatePerception(agent);

            // Выполнение выбранного действия
            agent.MakeDecision();
        }

        private void UpdateAgentsState(Agent agent)
        {
            switch (agent.Action)
            {
                case AgentAction.TurnLeft:
                case AgentAction.TurnRight:
                    agent.Turn(agent.Action);
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
            int ox;
            int oy;
            var isObjectChoosen = Landscape.ChooseObject(agent, out ox, out oy);

            // Объект нашли - съедаем его!
            if (!isObjectChoosen)
            {
                return;
            }

            switch (agent.Type)
            {
                case AgentType.Herbivore:
                    var findedPlant = Landscape.FindPlant(ox, oy);

                    // Если растение найдено, то удаляем его и сажаем в другом месте новое
                    if (findedPlant != null)
                    {
                        agent.Eat();

                        Landscape.RemovePlant(findedPlant);
                        Landscape.AddPlant(findedPlant);
                    }

                    break;

                case AgentType.Carnivore:
                    // Найти травоядное в списке агентов (по его позиции)
                    var findedHerbivore = Landscape.FindHerbivore(ox, oy);

                    // Если нашли, то удаляем агента
                    if (findedHerbivore != null)
                    {
                        agent.Eat();

                        Landscape.KillAgent(findedHerbivore);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}