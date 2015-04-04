using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Catel.Collections;
using Catel.MVVM;
using LifeSimulation;
using Nito.AsyncEx;
using Visualizer.CommonWpf;
using Visualizer.Views;
using CommandManager = System.Windows.Input.CommandManager;
using ViewModelBase = Visualizer.CommonWpf.ViewModelBase;

namespace Visualizer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int SimulationIterationCount = 50000;
        private OptimizedFastObservableCollection<AgeViewModel> _ages;
        private AgeViewModel _selectedAge;
        private FastObservableCollection<AgentViewModel> _selectedAgeCells;
        private int[] _horizontalCoordinates;
        private int[] _verticalCoordinates;
        private Simulation _simulation;
        private INotifyTaskCompletion _simulationComplition;
        private Dispatcher _currentDispatcher = Application.Current.Dispatcher;

        /// <summary>
        /// Contstructor for designer
        /// </summary>
        /// <param name="serializedLandscapes"></param>
        public MainViewModel(List<string> serializedLandscapes)
        {
            Initialization();

            Ages = new OptimizedFastObservableCollection<AgeViewModel>(serializedLandscapes.Select((x, i) => new AgeViewModel(i, x)));
            SelectedAge = Ages.First();
        }

        public MainViewModel()
        {
            Initialization();

            Ages = new OptimizedFastObservableCollection<AgeViewModel>(SimulationIterationCount);

            _simulationComplition = NotifyTaskCompletion.Create(DoSimulationAsync());
            _simulationComplition.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Exception")
                {
                    var completion = (INotifyTaskCompletion)sender;
                    MessageBox.Show(completion.Exception.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }

        private async Task DoSimulationAsync()
        {
            await Task.Run(() =>
            {
                const int FirstBatchCount = 30;
                var batchAges = new List<AgeViewModel>(FirstBatchCount);

                for (int batchIndex = 0; batchIndex < SimulationIterationCount && batchIndex < FirstBatchCount; batchIndex++)
                {
                    _simulation.EstimateState();

                    var ageViewModel = new AgeViewModel(batchIndex, LandscapeSerializer.Serialize(_simulation.Landscape));
                    batchAges.Add(ageViewModel);

                    _simulation.UpdateState();
                }
                _currentDispatcher.BeginInvoke(new Action<MainViewModel, List<AgeViewModel>>(
                    (main, batch) =>
                    {
                        main.Ages.AddItems(batch);
                        main.SelectedAge = main.Ages.First();
                    }), this, batchAges);

                for (int i = FirstBatchCount; i < SimulationIterationCount; i++)
                {
                    _simulation.EstimateState();

                    var ageViewModel = new AgeViewModel(i, LandscapeSerializer.Serialize(_simulation.Landscape));
                    _currentDispatcher.BeginInvoke(new Action<MainViewModel, AgeViewModel>((main, age) => main.Ages.Add(age)), this, ageViewModel);

                    _simulation.UpdateState();
                }
            });
        }

        private void Initialization()
        {
            // Set pseudo random for tests
            // Rand.ReinitializeRandom(1);
            _simulation = new Simulation();

            GridRows = _simulation.RowsCount;
            GridColumns = _simulation.ColumnsCount;

            Ages = new OptimizedFastObservableCollection<AgeViewModel>();

            HorizontalCoordinates = Enumerable.Range(0, GridColumns).ToArray();
            VerticalCoordinates = Enumerable.Range(0, GridRows).ToArray();

            ShowBestHerbivoreCommand = new Command(ShowBestHerbivoreExecute);
            ShowBestCarnivoreCommand = new Command(ShowBestCarnivoreExecute);
        }

        private void ShowBestAgent(AgentType agentType)
        {
            if (SelectedAge == null)
            {
                MessageBox.Show("Age should be selected", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var bestAgent = SelectedAge.GetLandscape().Statistics.GetMaxAgeAgent(agentType);
            if (bestAgent == null)
            {
                MessageBox.Show("The best " + agentType + " is not determined yet", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var viewModel = new AgentBrainViewModel(bestAgent);
            var window = new BrainWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        public ICommand ShowBestHerbivoreCommand { get; private set; }

        private void ShowBestHerbivoreExecute()
        {
            ShowBestAgent(AgentType.Herbivore);
        }

        public ICommand ShowBestCarnivoreCommand { get; private set; }

        private void ShowBestCarnivoreExecute()
        {
            ShowBestAgent(AgentType.Carnivore);
        }

        public int[] HorizontalCoordinates
        {
            get { return _horizontalCoordinates; }
            private set
            {
                if (Equals(value, _horizontalCoordinates))
                {
                    return;
                }

                _horizontalCoordinates = value;
                OnPropertyChanged();
            }
        }

        public int[] VerticalCoordinates
        {
            get { return _verticalCoordinates; }
            private set
            {
                if (Equals(value, _verticalCoordinates))
                {
                    return;
                }

                _verticalCoordinates = value;
                OnPropertyChanged();
            }
        }

        public OptimizedFastObservableCollection<AgeViewModel> Ages
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

                if (SelectedAge == null)
                {
                    return;
                }

                if (_selectedAgeCells == null)
                {
                    OnPropertyChanged("SelectedAgeCells");
                    CommandManager.InvalidateRequerySuggested();
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

    class DesignMainViewModel : MainViewModel
    {
        public DesignMainViewModel()
            : base(GetTestLandscapes())
        {
        }

        public static List<string> GetTestLandscapes()
        {
            Rand.ReinitializeRandom(1);
            var list = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                var landscape = Landscape.Create();
                list.Add(LandscapeSerializer.Serialize(landscape));
            }

            return list;
        }
    }
}