using System;

namespace LifeSimulation
{
    public class Agent
    {
        public Agent()
        {
        }

        public AgentType Type;
        public int Energy;
        public int Parent;
        public int Age;
        public int Generation;
        public Location Location;
        public Direction Direction;
        public int[] Inputs = new int[Simulation.MaxInputs];
        public int[] WeightOI = new int[Simulation.MaxInputs * Simulation.MaxOutputs];
        public int[] Biaso = new int[Simulation.MaxOutputs];
        public int[] Actions = new int[Simulation.MaxOutputs];

        public Agent DeepClone()
        {
            var result = (Agent)MemberwiseClone();
            result.Location = Location.DeepClone();
            result.Inputs = (int[])Inputs.Clone();
            result.WeightOI = (int[])WeightOI.Clone();
            result.Biaso = (int[])Biaso.Clone();
            result.Actions = (int[])Actions.Clone();

            return result;
        }
    }


}