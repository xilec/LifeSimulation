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
            var simulation = new Simulation();

            var serializedLandscapes = new List<string>();
            serializedLandscapes.Add(LandscapeSerializer.Serialize(simulation.Landscape));
            for (int i = 0; i < 100; i++)
            {
                simulation.Simulate();
                serializedLandscapes.Add(LandscapeSerializer.Serialize(simulation.Landscape));
            }

            var window = new MainWindow();
            window.DataContext = new MainViewModel(serializedLandscapes, simulation.RowsCount, simulation.ColumnsCount);
            window.Show();

            //var window = new BrainWindow();
            //window.DataContext = new DesignAgentBrainViewModel();
            //window.Show();
        }
    }
}
