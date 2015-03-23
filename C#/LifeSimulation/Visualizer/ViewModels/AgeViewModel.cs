﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using Catel.Collections;
using LifeSimulation;

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
        }

        public FastObservableCollection<AgentViewModel> Cells
        {
            get
            {
                var landscape = LandscapeSerializer.Deserialize(_serializedLandscape);
                var result = CreateSimulatedField(landscape);

                return result;
            }
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

        private FastObservableCollection<AgentViewModel> CreateSimulatedField(Landscape landscape)
        {
            // TODO заменить на обновление только агентов и расстений (для улучшения скорости обновления)
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

                cells[cellIndex] = new AgentViewModel(type, agent.Direction);
            }

            return cells;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCellIndex(Agent agent, int columns)
        {
            var location = agent.Location;
            var cellIndex = columns * location.Y + location.X;
            return cellIndex;
        }
    }
}