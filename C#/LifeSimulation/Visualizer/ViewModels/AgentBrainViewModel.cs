using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Catel.MVVM;
using LifeSimulation;
using ViewModelBase = Visualizer.CommonWpf.ViewModelBase;

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
            get
            {
                var weight = _agent.WeightOI;
                var result = new int[weight.Length];
                for (int outIndex = 0; outIndex < OutputsCount; outIndex++)
                {
                    for (int inIndex = 0; inIndex < InputsCount; inIndex++)
                    {
                        result[inIndex * Agent.MaxOutputs + outIndex] = weight[outIndex * Agent.MaxInputs + inIndex];
                    }
                }

                return result;
            }
        }

        public int[] Bias
        {
            get { return _agent.BiasO; }
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

        public AgentType Type
        {
            get { return _agent.Type; }
        }

        public string Description
        {
            get
            {
                return string.Format("{0}\tX: {1}\tY: {2}\tEnergy: {3}", _agent.Name, _agent.Location.X, _agent.Location.Y, _agent.Energy);
            }
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