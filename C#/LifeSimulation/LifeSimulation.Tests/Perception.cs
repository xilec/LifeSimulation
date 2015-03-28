﻿using FluentAssert;
using LifeSimulation.Training;
using NUnit.Framework;

namespace LifeSimulation.Tests
{
    public class Perception
    {
        [Test]
        public void EmptyEnvironment()
        {
            var expectedInputs = TrainingCamp.CreateInputs();

            var landscape = Landscape.CreateTest();
            
            var agent = new Agent(AgentType.Herbivore);
            agent.Location = new Location(10, 10);
            landscape.Agents[0] = agent;
            landscape.SetAgentInPosition(agent);

            landscape.UpdatePerception(agent);

            agent.Inputs.ShouldBeEqualTo(expectedInputs);
        }

        [Test]
        public void AgentOnFront()
        {
            var expectedCarnivoreInputs = TrainingCamp.CreateInputs(herbivoresOnFront: 1);
            var expectedHerbivoreInputs = TrainingCamp.CreateInputs(carnivoresOnFront: 1);

            var landscape = Landscape.CreateTest();

            var herbivore = new Agent(AgentType.Herbivore);
            herbivore.Direction = Direction.East;
            herbivore.Location = new Location(0, 4);
            landscape.Agents[0] = herbivore;
            landscape.SetAgentInPosition(herbivore);

            var carnivore = new Agent(AgentType.Carnivore);
            carnivore.Direction = Direction.West;
            carnivore.Location = new Location(2, 2);
            landscape.Agents[1] = carnivore;
            landscape.SetAgentInPosition(carnivore);

            landscape.UpdatePerception(herbivore);
            landscape.UpdatePerception(carnivore);

            herbivore.Inputs.ShouldBeEqualTo(expectedHerbivoreInputs);
            carnivore.Inputs.ShouldBeEqualTo(expectedCarnivoreInputs);
        }
    }
}