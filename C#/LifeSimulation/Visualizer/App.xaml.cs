using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LifeSimulation;
using Newtonsoft.Json;
using Visualizer.ViewModels;
using Visualizer.Views;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Set pseudo random for tests
            // Rand.ReinitializeRandom(1);

            var simulation = new Simulation();

            var serializedLandscapes = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                simulation.EstimateState();
                serializedLandscapes.Add(LandscapeSerializer.Serialize(simulation.Landscape));
                simulation.UpdateState();
            }

            var window = new MainWindow();
            window.DataContext = new MainViewModel(serializedLandscapes, simulation.RowsCount, simulation.ColumnsCount);
            Application.Current.MainWindow = window;
            window.ShowDialog();
        }
    }
}
