using System.Linq;
using LifeSimulation;
using Visualizer.CommonWpf;

namespace Visualizer.ViewModels
{
    public class AgentBrainViewModel : ViewModelBase
    {
        private readonly Agent _agent;

        public AgentBrainViewModel(Agent agent)
        {
            _agent = agent;
        }

        public int[] Inputs
        {
            get { return _agent.Inputs; }
        }

        public int[] Brain
        {
            get { return _agent.WeightOI; }
        }

        public int InputsCount
        {
            get { return Inputs.Length; }
        }

        public int OutputsCount
        {
            get { return Outputs.Length; }
        }

    public int[] Outputs
        {
            get { return _agent.Outputs; }
        }

        public AgentAction Action
        {
            get { return _agent.Action; }
        }

        public string Name
        {
            get { return _agent.Name; }
        }
    }

    public class DesignAgentBrainViewModel : AgentBrainViewModel
    {
        public DesignAgentBrainViewModel() : base(CreateDesignAgent())
        {
        }

        private static Agent CreateDesignAgent()
        {
            var agent = new Agent(AgentType.Carnivore);
            agent.Inputs = Enumerable.Range(0, Agent.MaxInputs).ToArray();
            agent.WeightOI = Enumerable.Range(0, Agent.TotalWeights).ToArray();
            agent.Outputs = Enumerable.Range(0, Agent.MaxOutputs).ToArray();
            agent.Action = AgentAction.Eat;

            return agent;
        }
    }
}