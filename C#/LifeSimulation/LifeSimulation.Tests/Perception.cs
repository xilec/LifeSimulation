using FluentAssert;
using NUnit.Framework;

namespace LifeSimulation.Tests
{
    public class Perception
    {
        [Test]
        public void EmptyEnvironment()
        {
            var landscape = Landscape.CreateTest();
            
            var agent = new Agent(AgentType.Herbivore);
            landscape.Agents[0] = agent;

            landscape.UpdatePerception(agent);

            foreach (var input in agent.Inputs)
            {
                input.ShouldBeEqualTo(0);
            }
        }
    }
}