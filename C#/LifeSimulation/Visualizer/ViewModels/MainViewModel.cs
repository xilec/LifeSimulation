﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using Catel.Collections;
using LifeSimulation;

namespace Visualizer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<AgeViewModel> _ages;
        private AgeViewModel _selectedAge;
        private FastObservableCollection<AgentViewModel> _selectedAgeCells;

        public MainViewModel(List<string> serializedLandscapes, int rowsCount, int columnsCount)
        {
            GridRows = rowsCount;
            GridColumns = columnsCount;

            Ages = new ObservableCollection<AgeViewModel>(serializedLandscapes.Select((x, i) => new AgeViewModel(i, x)));

            if (Ages.Any())
            {
                SelectedAge = Ages.First();
            }
        }

        public ObservableCollection<AgeViewModel> Ages
        {
            get { return _ages; }
            set
            {
                if (Equals(value, _ages)) return;
                _ages = value;
                OnPropertyChanged();
            }
        }

        public AgeViewModel SelectedAge
        {
            get { return _selectedAge; }
            set
            {
                if (Equals(value, _selectedAge)) return;
                _selectedAge = value;

                OnPropertyChanged();

                if (_selectedAgeCells == null)
                {
                    OnPropertyChanged("SelectedAgeCells");                    
                }
                else
                {
                    UpdateCells();
                }
            }
        }

        private void UpdateCells()
        {
            var cells = _selectedAgeCells;
            var newCells = SelectedAge.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var newCell = newCells[i];

                if (cell.Type != newCell.Type)
                {
                    cells[i] = newCell;
                    continue;
                }

                switch (cell.Type)
                {
                    case VisualAgentType.None:

                        break;
                    case VisualAgentType.Plant:
                    case VisualAgentType.Herbivore:
                    case VisualAgentType.Carnivore:
                        cells[i] = newCell;
                        break;
                }
            }
        }

        public FastObservableCollection<AgentViewModel> SelectedAgeCells
        {
            get
            {
                if (SelectedAge == null)
                {
                    return null;
                }

                return _selectedAgeCells = SelectedAge.Cells;
            }
        }

        public int GridColumns { get; protected set; }
        public int GridRows { get; protected set; }
    }

    class TestMainViewModel : MainViewModel
    {
        public const int GridSize = 10;

        public TestMainViewModel() : base(GetTestLandscapes(), GridSize, GridSize)
        {
        }

        public static List<string> GetTestLandscapes()
        {
            var list = new List<string>();
            for (int i = 0; i < 15; i++)
            {
                var landscape = new Landscape();
                list.Add(LandscapeSerializer.Serialize(landscape));
            }

            return list;
        }
    }
}