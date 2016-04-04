using Mocap.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mocap.View
{
    /// <summary>
    /// Interaction logic for SensorDetailsWindow.xaml
    /// </summary>
    public partial class SensorDetailsWindow : Window
    {


        public SensorVM Sensor
        {
            get { return (SensorVM)GetValue(SensorProperty); }
            set { SetValue(SensorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sensor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SensorProperty =
            DependencyProperty.Register(nameof(Sensor), typeof(SensorVM), typeof(SensorDetailsWindow), new FrameworkPropertyMetadata(OnSensorPropertyChanged));


        public SensorDetailsWindow()
        {
            InitializeComponent();
            Closing += SensorDetailsWindow_Closing;
        }

        private void SensorDetailsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private static void OnSensorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var inst = (SensorDetailsWindow)obj;

            inst.view.Sensor = e.NewValue as SensorVM;
        }
    }
}
