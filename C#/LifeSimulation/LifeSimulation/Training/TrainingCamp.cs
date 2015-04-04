using System.Collections.Generic;

namespace LifeSimulation.Training
{
    public static class TrainingCamp
    {
        public static Agent EducateAgent(AgentType agentType)
        {
            var agent = new Agent(agentType);
            var tmpInputs = agent.Inputs;

            ArtificialBrain choosenBrain;
            while (true)
            {
                var brain = CreateBrain();
                SetBrain(agent, brain);
                var scores = FitnessFunction(agent, StandardTests[agentType]);

                if (scores >= 1700)
                {
                    choosenBrain = brain;
                    break;
                }
            }

            SetBrain(agent, choosenBrain);
            agent.Inputs = tmpInputs;

            return agent;
        }

        private static void SetBrain(Agent agent, ArtificialBrain brain)
        {
            agent.WeightOI = brain.WeightOI;
            agent.BiasO = brain.BiasO;
        }

        private static double FitnessFunction(Agent agent, Test[] tests)
        {
            var scores = 0d;
            foreach (var test in tests)
            {
                agent.Inputs = test.Inputs;
                agent.MakeDecision();

                scores += agent.Action == test.ExpectedAction ? test.Scores : 0;
            }

            return scores;
        }

        private static ArtificialBrain CreateBrain()
        {
            var result = new ArtificialBrain();

            for (int i = 0; i < result.WeightOI.Length; i++)
            {
                result.WeightOI[i] = Rand.GetWeight();
            }

            for (int i = 0; i < result.BiasO.Length; i++)
            {
                result.BiasO[i] = Rand.GetWeight();
            }

            return result;
        }

        private class ArtificialBrain
        {
            public readonly int[] WeightOI = new int[Agent.TotalWeights];
            public readonly int[] BiasO = new int[Agent.MaxOutputs];
        }

        private readonly static Test[] StandardHerbivoreTests = 
        {
            new Test(CreateInputs(), AgentAction.Move, 200),
            new Test(CreateInputs(plantsOnLeft:1), AgentAction.TurnLeft, 100),
            new Test(CreateInputs(plantsOnRight:1), AgentAction.TurnRight, 100),
            new Test(CreateInputs(plantsOnFront:1), AgentAction.Move, 500),
            new Test(CreateInputs(plantsOnFront:1), AgentAction.Eat, -300),
            new Test(CreateInputs(plantsOnFront:1), AgentAction.TurnLeft, -300),
            new Test(CreateInputs(plantsOnFront:1), AgentAction.TurnRight, -300),
            new Test(CreateInputs(plantsOnProximity:1), AgentAction.Eat, 1000),
        };

        private static readonly Test[] StandardCarnivoreTests = 
        {
            new Test(CreateInputs(), AgentAction.Move, 200),
            new Test(CreateInputs(herbivoresOnLeft:1), AgentAction.TurnLeft, 100),
            new Test(CreateInputs(herbivoresOnRight:1), AgentAction.TurnRight, 100),
            new Test(CreateInputs(herbivoresOnFront:1), AgentAction.Move, 500),
            new Test(CreateInputs(herbivoresOnFront:1), AgentAction.Eat, -300),
            new Test(CreateInputs(herbivoresOnFront:1), AgentAction.TurnLeft, -300),
            new Test(CreateInputs(herbivoresOnFront:1), AgentAction.TurnRight, -300),
            new Test(CreateInputs(herbivoresOnProximity:1), AgentAction.Eat, 1000),
        };

        private readonly static Dictionary<AgentType, Test[]> StandardTests = new Dictionary<AgentType, Test[]>
        {
            {AgentType.Herbivore, StandardHerbivoreTests},
            {AgentType.Carnivore, StandardCarnivoreTests},
        };

        private class Test
        {
            public Test(int[] inputs, AgentAction expectedAction, double scores)
            {
                Inputs = inputs;
                ExpectedAction = expectedAction;
                Scores = scores;
            }

            public int[] Inputs { get; set; }
            public AgentAction ExpectedAction { get; set; }
            public double Scores { get; set; }
        }

        public static int[] CreateInputs(
            int herbivoresOnFront = 0,
            int carnivoresOnFront = 0,
            int plantsOnFront = 0,
            int herbivoresOnLeft = 0,
            int carnivoresOnLeft = 0,
            int plantsOnLeft = 0,
            int herbivoresOnRight = 0,
            int carnivoresOnRight = 0,
            int plantsOnRight = 0,
            int herbivoresOnProximity = 0,
            int carnivoresOnProximity = 0,
            int plantsOnProximity = 0)
        {
            var inputs = new int[Agent.MaxInputs];
            inputs[0] = herbivoresOnFront;
            inputs[1] = carnivoresOnFront;
            inputs[2] = plantsOnFront;
            inputs[3] = herbivoresOnLeft;
            inputs[4] = carnivoresOnLeft;
            inputs[5] = plantsOnLeft;
            inputs[6] = herbivoresOnRight;
            inputs[7] = carnivoresOnRight;
            inputs[8] = plantsOnRight;
            inputs[9] = herbivoresOnProximity;
            inputs[10] = carnivoresOnProximity;
            inputs[11] = plantsOnProximity;

            return inputs;
        }
    }
}