using System.Linq;
using NUnit.Framework;

namespace LifeSimulation.Tests
{
    public class Tests
    {
        [Test]
        public void Extinction()
        {
            var simulation = new Simulation();

            for (int i = 0; i <400; i++)
            {
                simulation.Simulate();
            }

            Assert.IsTrue(simulation.Landscape.Agents.Any(x => x != null), "After simulation should be alived agents");
            Assert.IsTrue(simulation.Landscape.Plants.Any(x => x != null), "After simulation should be alived plants");
        }
    }
}