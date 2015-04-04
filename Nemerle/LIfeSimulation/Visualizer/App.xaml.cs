using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
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
            var window = new MainWindow();
            window.DataContext = new MainViewModel();
            Application.Current.MainWindow = window;
            window.ShowDialog();
        }
    }
}
