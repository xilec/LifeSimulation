using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Catel.Collections;
using LifeSimulation;
using Visualizer.CommonWpf;

namespace Visualizer.ViewModels
{
    public class AgeViewModel : ViewModelBase
    {
        private readonly string _serializedLandscape;
        private int _number;

        public AgeViewModel(int number, string serializedLandscape)
        {
            Number = number;
            _serializedLandscape = serializedLandscape;

            AliveAgents = new FastObservableCollection<AgentViewModel>();
        }

        public int Number
        {
            get { return _number; }
            set
            {
                if (value == _number) return;
                _number = value;
                OnPropertyChanged();
            }
        }

        public AgentStatisticsViewModel HerbivoreStats { get; private set; }

        public AgentStatisticsViewModel CarnivoreStats { get; private set; }

        public FastObservableCollection<AgentViewModel> Cells
        {
            get
            {
                var landscape = GetLandscape();
                var result = CreateSimulatedField(landscape);

                HerbivoreStats = FillStatistics(AgentType.Herbivore, landscape.Statistics);
                CarnivoreStats = FillStatistics(AgentType.Carnivore, landscape.Statistics);
                OnPropertyChanged("HerbivoreStats");
                OnPropertyChanged("CarnivoreStats");
                OnPropertyChanged("AliveAgents");

                return result;
            }
        }

        public Landscape GetLandscape()
        {
            return LandscapeSerializer.Deserialize(_serializedLandscape);
        }

        public FastObservableCollection<AgentViewModel> AliveAgents { get; private set; }

        private static AgentStatisticsViewModel FillStatistics(AgentType agentType, Statistics stats)
        {
            var count = stats.AgentTypeCounts[agentType];
            var reproductions = stats.AgentTypeReproductions[agentType];
            var deathes = stats.AgentTypeDeathes[agentType];
            var maxAgeAgent = stats.GetMaxAgeAgent(agentType);

            var viewModel = new AgentStatisticsViewModel(count, reproductions, deathes, maxAgeAgent != null ? maxAgeAgent.Age : 0);
            return viewModel;
        }


        private FastObservableCollection<AgentViewModel> CreateSimulatedField(Landscape landscape)
        {
            var columnsCount = landscape.GetColumnsCount();
            var rowsCount = landscape.GetRowsCount();

            var cells = new FastObservableCollection<AgentViewModel>();

            for (int i = 0; i < columnsCount * rowsCount; i++)
            {
                cells.Add(new AgentViewModel(VisualAgentType.None));
            }


            foreach (var plant in landscape.Plants)
            {
                if (plant == null)
                {
                    continue;
                }

                var cellIndex = GetCellIndex(plant, columnsCount);
                cells[cellIndex] = new AgentViewModel(VisualAgentType.Plant);
            }

            var agents = new List<AgentViewModel>(landscape.Agents.Length);
            foreach (var agent in landscape.Agents)
            {
                if (agent == null)
                {
                    continue;
                }

                var cellIndex = GetCellIndex(agent, columnsCount);
                var type = VisualAgentType.None;
                switch (agent.Type)
                {
                    case AgentType.Herbivore:
                        type = VisualAgentType.Herbivore;
                        break;
                    case AgentType.Carnivore:
                        type = VisualAgentType.Carnivore;
                        break;
                }

                var agentViewModel = new AgentViewModel(type, agent);
                cells[cellIndex] = agentViewModel;
                agents.Add(agentViewModel);
            }

            AliveAgents.ReplaceRange(agents);

            return cells;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndex(ISimulationObject simulationObject, int columns)
        {
            var location = simulationObject.Location;
            var cellIndex = columns * location.Y + location.X;
            return cellIndex;
        }
    }
}