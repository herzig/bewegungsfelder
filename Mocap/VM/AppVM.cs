using Mocap.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Mocap.VM
{
    public class AppVM : INotifyPropertyChanged
    {
        private object detailsItem;

        private DispatcherTimer refreshTimer;

        private ObservableCollection<SensorVM> sensors;
        private DataCollector dataCollector;

        public ReadOnlyObservableCollection<SensorVM> Sensors { get; }

        public ObservableCollection<BoneVM> Bones { get; } = new ObservableCollection<BoneVM>();

        public KinematicVM Kinematic { get; }

        public ModelVisual3D RootVisual3D { get; }

        public object DetailsItem
        {
            get { return detailsItem; }
            set
            {
                if (detailsItem != value)
                {
                    detailsItem = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DetailsItem)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppVM(DataCollector collector, Kinematic kinematic)
        {
            dataCollector = collector;
            dataCollector.SensorAdded += OnSensorAdded;

            sensors = new ObservableCollection<SensorVM>();
            Sensors = new ReadOnlyObservableCollection<SensorVM>(sensors);

            foreach (var item in dataCollector.Sensors.Values)
                sensors.Add(new SensorVM(item));

            Kinematic = new KinematicVM(kinematic, Sensors);
            Kinematic.SetDetailItemRequested += OnSetDetailItemRequested;

            RootVisual3D = new ModelVisual3D();
            RootVisual3D.Children.Add(Kinematic.Root.Visual);
            RootVisual3D.Children.Add(Kinematic.Root.WorldVisual);

            refreshTimer = new DispatcherTimer(DispatcherPriority.Background);
            refreshTimer.Interval = TimeSpan.FromMilliseconds(20);
            refreshTimer.Start();
            refreshTimer.Tick += OnRefreshTick;
        }

        public void StartServer()
        {
            dataCollector.Start();
        }

        private void OnRefreshTick(object sender, EventArgs e)
        {
            foreach (var item in Sensors)
            {
                item.RaisePropertyChanged();
            }

            Kinematic.Model.ApplySensorData();
            Kinematic.Refresh();
        }

        private void OnSensorAdded(Sensor model)
        {
            sensors.Add(new SensorVM(model));
        }

        private void OnSetDetailItemRequested(object item)
        {
            DetailsItem = item;
        }
    }
}
