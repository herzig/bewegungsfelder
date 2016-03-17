using GalaSoft.MvvmLight.CommandWpf;
using Mocap.BVH;
using Mocap.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Mocap.VM
{
    public class AppVM : INotifyPropertyChanged
    {
        public enum CaptureState
        {
            Stopped,
            Running
        }

        private CaptureState currentCaptureState = CaptureState.Stopped;

        private object detailsItem;

        private DispatcherTimer refreshTimer;

        private ObservableCollection<SensorVM> sensors;
        private DataCollector dataCollector;

        private KinematicVM kinematic;

        /// <summary>
        /// collection of all registered sensors
        /// </summary>
        public ReadOnlyObservableCollection<SensorVM> Sensors { get; }

        public ObservableCollection<BoneVM> Bones { get; } = new ObservableCollection<BoneVM>();

        public SensorBoneMap SensorBoneMap = new SensorBoneMap();

        public KinematicVM Kinematic
        {
            get { return kinematic; }
            set
            {
                if (kinematic != value)
                {
                    if (kinematic != null)
                    {
                        RootVisual3D.Children.Remove(kinematic.Root.Visual);
                        RootVisual3D.Children.Remove(kinematic.Root.WorldVisual);
                    }

                    kinematic = value;
                    kinematic.SetDetailItemRequested += OnSetDetailItemRequested;

                    SensorBoneMap.Clear();

                    if (kinematic != null)
                    {
                        RootVisual3D.Children.Add(kinematic.Root.Visual);
                        RootVisual3D.Children.Add(kinematic.Root.WorldVisual);
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Kinematic)));
                }
            }
        }

        public ModelVisual3D RootVisual3D { get; } = new ModelVisual3D();

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

        public ICommand LoadBVHFileCommand { get; }

        public ICommand AssignSensorToBoneCommand { get; }

        public ICommand StartCaptureCommand { get; }

        public ICommand StopCaptureCommand { get; }

        public ICommand SetBaseRotationCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppVM()
        {
            dataCollector = new DataCollector();
            dataCollector.SensorAdded += OnSensorAdded;

            // setup sensors collection
            sensors = new ObservableCollection<SensorVM>();
            Sensors = new ReadOnlyObservableCollection<SensorVM>(sensors);
            foreach (var item in dataCollector.Sensors.Values)
                sensors.Add(new SensorVM(item));

            // setup kinematic chain
            Kinematic = new KinematicVM(new Kinematic());

            // setup commands
            LoadBVHFileCommand = new RelayCommand<string>(LoadBVHFile);
            AssignSensorToBoneCommand = new RelayCommand<Tuple<BoneVM, SensorVM>>(AssignSensorToBone);
            StartCaptureCommand = new RelayCommand(StartCapture, CanStartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture, CanStopCapture);
            SetBaseRotationCommand = new RelayCommand(SetBaseRotation);

            // ui update timer
            refreshTimer = new DispatcherTimer(DispatcherPriority.Background);
            refreshTimer.Interval = TimeSpan.FromMilliseconds(20);
            refreshTimer.Start();
            refreshTimer.Tick += OnRefreshTick;
        }

        private void LoadBVHFile(string file)
        {
            if (StopCaptureCommand.CanExecute(null))
            {
                StopCaptureCommand.Execute(null);
            }

            BVHNode bvhroot = BVHReader.ReadBvhHierarchy(file);
            Bone newRoot = BVHConverter.ToBones(bvhroot, null);

            Kinematic = new KinematicVM(new Core.Kinematic(newRoot));
        }

        private void StartCapture()
        {
            if (currentCaptureState != CaptureState.Running)
            {
                currentCaptureState = CaptureState.Running;
            }
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanStartCapture()
        {
            return currentCaptureState != CaptureState.Running;
        }

        private void StopCapture()
        {
            if (currentCaptureState == CaptureState.Running)
            {
                currentCaptureState = CaptureState.Stopped;
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanStopCapture()
        {
            return currentCaptureState == CaptureState.Running;
        }

        /// <summary>
        /// assign a sensor to a bone
        /// </summary>
        private void AssignSensorToBone(Tuple<BoneVM, SensorVM> pair)
        {
            var bone = pair.Item1;
            var sensor = pair.Item2;

            if (sensor == null)
            {
                SensorBoneMap.RemoveLink(bone.Model);
            }
            else
            {
                SensorBoneMap.SetLink(bone.Model, sensor.Model);
            }

            // add new link
            bone.Sensor = sensor;
        }

        /// <summary>
        /// start sensor data colelctor server
        /// </summary>
        public void StartServer()
        {
            dataCollector.Start();
        }

        /// <summary>
        /// is called by the refreshTimer. Raises UI updates
        /// </summary>
        private void OnRefreshTick(object sender, EventArgs e)
        {
            foreach (var item in Sensors)
            {
                item.RaisePropertyChanged();
            }

            if (currentCaptureState == CaptureState.Running)
            {
                var orientations = SensorBoneMap.GetSensorOrientations();
                Kinematic.Model.ApplyRotations(orientations);
            }

            Kinematic.Refresh();
        }

        /// <summary>
        /// called when a new sensor is registered.
        /// Creates the sensor view model an adds it to the public collection
        /// </summary>
        private void OnSensorAdded(Sensor model)
        {
            sensors.Add(new SensorVM(model));
        }

        /// <summary>
        /// called when any child view model requests to set the item shown in the details view
        /// </summary>
        private void OnSetDetailItemRequested(object item)
        {
            DetailsItem = item;
        }

        /// <summary>
        /// updates the base rotation for all sensor-bone links
        /// </summary>
        private void SetBaseRotation()
        {
            foreach (var item in SensorBoneMap.Links)
            {
                item.SetBaseRotation();
            }
        }

        /// <summary>
        /// finds the current Applications' ViewModel instance and returns it
        /// </summary>
        /// <returns></returns>
        public static AppVM GetCurrent()
        {
            return (AppVM)Application.Current.FindResource("AppVM");
        }
    }
}
