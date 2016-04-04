using LiveCharts;
using Mocap.VM;
using OxyPlot;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mocap.View
{
    /// <summary>
    /// Interaction logic for SensorDetailsView.xaml
    /// </summary>
    public partial class SensorDetailsView : UserControl
    {
        DispatcherTimer refreshTimer;

        private List<DataPoint> xRaw = new List<DataPoint>();
        private List<DataPoint> yRaw = new List<DataPoint>();
        private List<DataPoint> zRaw = new List<DataPoint>();

        private List<DataPoint> xFiltered = new List<DataPoint>();
        private List<DataPoint> yFiltered = new List<DataPoint>();
        private List<DataPoint> zFiltered = new List<DataPoint>();

        public SensorVM Sensor
        {
            get { return (SensorVM)GetValue(SensorProperty); }
            set { SetValue(SensorProperty, value); }
        }

        public static readonly DependencyProperty SensorProperty =
            DependencyProperty.Register(nameof(Sensor), typeof(SensorVM), typeof(SensorDetailsView),
                new FrameworkPropertyMetadata(OnSensorPropertyChanged));


        public SensorDetailsView()
        {
            InitializeComponent();

            refreshTimer = new DispatcherTimer(DispatcherPriority.Background);
            refreshTimer.Interval = TimeSpan.FromMilliseconds(100);
            refreshTimer.Tick += OnRefreshTimerTick;

            xRawSeries.ItemsSource = xRaw;
            yRawSeries.ItemsSource = yRaw;
            zRawSeries.ItemsSource = zRaw;

            xFilteredSeries.ItemsSource = xFiltered;
            yFilteredSeries.ItemsSource = yFiltered;
            zFilteredSeries.ItemsSource = zFiltered;
        }

        private void OnRefreshTimerTick(object sender, EventArgs e)
        {
            xRaw.Add(new DataPoint(Sensor.LastValue.SensorTimestamp, Sensor.LastValue.Acceleration.X));
            yRaw.Add(new DataPoint(Sensor.LastValue.SensorTimestamp, Sensor.LastValue.Acceleration.Y));
            zRaw.Add(new DataPoint(Sensor.LastValue.SensorTimestamp, Sensor.LastValue.Acceleration.Z));

            xFiltered.Add(new DataPoint(Sensor.LastValue.SensorTimestamp, Sensor.LastValue.Acceleration.X - Sensor.Model.Gravity.X));
            yFiltered.Add(new DataPoint(Sensor.LastValue.SensorTimestamp, Sensor.LastValue.Acceleration.Y - Sensor.Model.Gravity.Y));
            zFiltered.Add(new DataPoint(Sensor.LastValue.SensorTimestamp, Sensor.LastValue.Acceleration.Z - Sensor.Model.Gravity.Z));

            if (xRaw.Count > 40)
                xRaw.RemoveRange(0, xRaw.Count - 40);
            if (yRaw.Count > 40)
                yRaw.RemoveRange(0, yRaw.Count - 40);
            if (zRaw.Count > 40)
                zRaw.RemoveRange(0, zRaw.Count - 40);

            if (xFiltered.Count > 40)
                xFiltered.RemoveRange(0, xFiltered.Count - 40);
            if (yFiltered.Count > 40)
                yFiltered.RemoveRange(0, yFiltered.Count - 40);
            if (zFiltered.Count > 40)
                zFiltered.RemoveRange(0, zFiltered.Count - 40);

            plot_raw.InvalidatePlot();
            plot_filtered.InvalidatePlot();
        }

        private static void OnSensorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var view = (SensorDetailsView)obj;
            view.xRaw.Clear();
            view.yRaw.Clear();
            view.zRaw.Clear();
            view.plot_raw.InvalidatePlot();

            if (e.NewValue == null)
                view.refreshTimer.Stop();
            else
                view.refreshTimer.Start();

            view.DataContext = e.NewValue;
        }

        private void OnSampleGravityClick(object sender, RoutedEventArgs e)
        {
            Sensor.Model.StartPositionIntegration();
        }
    }
}
