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
using HelixToolkit.Wpf;

namespace Mocap.VM
{
    public class AppVM : INotifyPropertyChanged
    {
        public enum AppState
        {
            Default,
            Running,
            Calibration
        }

        private AppState state = AppState.Default;

        private object detailsItem;

        private DispatcherTimer refreshTimer;

        private ObservableCollection<SensorVM> sensors;
        private DataCollector dataCollector;

        private KinematicVM kinematic;
        private KinematicVM liveKinematic;

        private KinematicAnimatorVM animator;

        private Dictionary<Sensor, SensorVM> sensorVMs;
        private Dictionary<SensorBoneLink, SensorBoneLinkVM> sensorBoneLinkVMs;

        /// <summary>
        /// the current state of the application
        /// </summary>
        public AppState State
        {
            get { return state; }
            set
            {
                if (State != value)
                {
                    state = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInCalibrationState)));
                }
            }
        }

        public bool IsInCalibrationState { get { return State == AppState.Calibration; } }

        private SensorBoneLinkVM calibrationBoneLink;
        public SensorBoneLinkVM CalibrationBoneLink
        {
            get { return calibrationBoneLink; }
            set
            {
                if (calibrationBoneLink != value)
                {
                    calibrationBoneLink = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalibrationBoneLink)));
                }
            }
        }

        /// <summary>
        /// collection of all registered sensors
        /// </summary>
        public ReadOnlyObservableCollection<SensorVM> Sensors { get; }

        public SensorBoneMap SensorBoneMap { get; }

        public KinematicVM BaseKinematic
        {
            get { return kinematic; }
            set
            {
                if (kinematic != value)
                {
                    if (kinematic != null)
                    {
                        RootVisual3D.Children.Remove(kinematic.Root.Visual);
                    }

                    kinematic = value;
                    kinematic.SetDetailItemRequested += OnSetDetailItemRequested;

                    SensorBoneMap.Clear();

                    if (kinematic != null)
                    {
                        RootVisual3D.Children.Add(kinematic.Root.Visual);
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BaseKinematic)));
                }
            }
        }

        public KinematicVM LiveKinematic
        {
            get { return liveKinematic; }
            set
            {
                if (liveKinematic != null)
                {
                    RootVisual3D.Children.Remove(liveKinematic.Root.Visual);
                }

                liveKinematic = value;

                if (liveKinematic != null)
                {
                    RootVisual3D.Children.Add(liveKinematic.Root.Visual);
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LiveKinematic)));
            }
        }

        public KinematicAnimatorVM Animator
        {
            get { return animator; }
            set
            {
                if (animator != value)
                {
                    animator = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Animator)));
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

        public ICommand SaveBVHFileCommand { get; }

        public ICommand AssignSensorToBoneCommand { get; }

        public ICommand StartCaptureCommand { get; }

        public ICommand StopCaptureCommand { get; }

        public ICommand SetBaseRotationCommand { get; }

        public ICommand StartSensorCalibrationCommand { get; }

        public ICommand StopSensorCalibrationCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppVM()
        {
            dataCollector = new DataCollector();
            dataCollector.SensorAdded += OnSensorAdded;

            // setup sensor-bone links
            SensorBoneMap = new SensorBoneMap();
            sensorBoneLinkVMs = new Dictionary<SensorBoneLink, SensorBoneLinkVM>();
            SensorBoneMap.LinkAdded += (link) =>
            {
                sensorBoneLinkVMs.Add(link, new SensorBoneLinkVM(link, sensorVMs[link.Sensor], BaseKinematic.BoneVMMap[link.Bone]));
                RootVisual3D.Children.Add(sensorBoneLinkVMs[link].Visual);
            };
            SensorBoneMap.LinkRemoved += (link) =>
                {
                    RootVisual3D.Children.Remove(sensorBoneLinkVMs[link].Visual);
                    sensorBoneLinkVMs.Remove(link);
                };

            // setup sensors collection
            sensors = new ObservableCollection<SensorVM>();
            sensorVMs = new Dictionary<Sensor, SensorVM>();
            Sensors = new ReadOnlyObservableCollection<SensorVM>(sensors);
            foreach (var item in dataCollector.Sensors.Values)
            {
                sensors.Add(new SensorVM(item));
                sensorVMs.Add(item, sensors.Last());
            }

            // setup kinematic chain
            BaseKinematic = new KinematicVM(new Kinematic());

            // setup animator
            Animator = new KinematicAnimatorVM(BaseKinematic, new MotionData());

            // setup commands
            LoadBVHFileCommand = new RelayCommand<string>(LoadBVHFile);
            SaveBVHFileCommand = new RelayCommand<string>(SaveBVHFile);
            AssignSensorToBoneCommand = new RelayCommand<Tuple<BoneVM, SensorVM>>(AssignSensorToBone);
            StartCaptureCommand = new RelayCommand(StartCapture, CanStartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture, CanStopCapture);
            SetBaseRotationCommand = new RelayCommand(SetBaseRotation);
            StartSensorCalibrationCommand = new RelayCommand<SensorBoneLinkVM>(StartSensorCalibration, CanStartSensorCalibration);
            StopSensorCalibrationCommand = new RelayCommand(StopAxisCalibration);

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

            BVHMotionData bvhMotionData;
            BVHNode bvhroot = BVHReaderWriter.ReadBvh(file, out bvhMotionData);

            MotionData newMotionData = new MotionData();
            newMotionData.FPS = 1.0 / bvhMotionData.FrameTime;
            Bone newRoot = BVHConverter.ToBones(bvhroot, null, bvhMotionData, newMotionData);

            // create & assign a new kinematic view model. BoneVMs are also created in this process.
            BaseKinematic = new KinematicVM(new Core.Kinematic(newRoot));
            Animator = new KinematicAnimatorVM(BaseKinematic, newMotionData);
        }

        /// <summary>
        /// write the kinematic structure and any recorded motion data to a BVH file
        /// </summary>
        private void SaveBVHFile(string file)
        {
            BVHMotionData motionData;
            var root = BVHConverter.ToBVHData(BaseKinematic.Root.Model, Animator.MotionData, out motionData);
            BVHReaderWriter.WriteBvh(file, root, motionData);
        }

        private void StartCapture()
        {
            if (State != AppState.Default)
                throw new InvalidOperationException("not allowed when not in idle state");

            State = AppState.Running;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanStartCapture()
        {
            return State == AppState.Default;
        }

        private void StopCapture()
        {
            if (State == AppState.Running)
            {
                State = AppState.Default;
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanStopCapture()
        {
            return State == AppState.Running;
        }

        /// <summary>
        /// assign a sensor to a bone
        /// </summary>
        private void AssignSensorToBone(Tuple<BoneVM, SensorVM> pair)
        {
            var bone = pair.Item1;
            var sensor = pair.Item2;

            SensorBoneLink newLink = null;
            if (sensor == null)
            { //remove existing links
                SensorBoneMap.RemoveLink(bone.Model);
            }
            else
            { //add new link
                newLink = SensorBoneMap.CreateLink(bone.Model, sensor.Model);
            }

            if (newLink != null)
            {
                bone.SensorBoneLink = sensorBoneLinkVMs[newLink];
            }
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
                item.Refresh();
            }

            foreach (var item in sensorBoneLinkVMs.Values)
            {
                item.Refresh();
            }

            if (State == AppState.Running)
            {
                var orientations = SensorBoneMap.GetCalibratedSensorOrientations();
                BaseKinematic.Model.ApplyWorldRotations(orientations);
            }

            BaseKinematic.Refresh();
        }

        /// <summary>
        /// called when a new sensor is registered.
        /// Creates the sensor view model an adds it to the public collection
        /// </summary>
        private void OnSensorAdded(Sensor model)
        {
            sensors.Add(new SensorVM(model));
            sensorVMs.Add(model, sensors.Last());
        }

        private void StartSensorCalibration(SensorBoneLinkVM link)
        {
            if (State != AppState.Default)
                throw new InvalidOperationException("Not allowed when not in idle state");

            CalibrationBoneLink = link;
            State = AppState.Calibration;
        }

        private bool CanStartSensorCalibration(SensorBoneLinkVM link)
        {
            return State == AppState.Default && link != null;
        }

        private void StopAxisCalibration()
        {
            if (State != AppState.Calibration)
                throw new InvalidOperationException("You shall not cancel something that you've never started");

            CalibrationBoneLink = null;
            State = AppState.Default;
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
                item.SetBaseOrientation();
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
