using Mocap.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mocap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppVM appViewModel;

        public MainWindow()
        {
            InitializeComponent();

            appViewModel = new AppVM(new Core.DataCollector(), new Core.Kinematic());

            appViewModel.StartServer();

            DataContext = appViewModel;

            viewport.Children.Add(appViewModel.RootVisual3D);
        }

        private void OnCaptureClick(object sender, RoutedEventArgs e)
        {
            appViewModel.Kinematic.StartCapture();
        }
    }
}
