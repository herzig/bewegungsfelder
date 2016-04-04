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
        private KinematicVM liveKinematic;

        private KinematicAnimatorVM animator;

        private Dictionary<SensorBoneLink, SensorBoneLinkVM> LinkVMs;

        /// <summary>
        /// collection of all registered sensors
        /// </summary>
        public ReadOnlyObservableCollection<SensorVM> Sensors { get; }

        public ObservableCollection<BoneVM> Bones { get; } = new ObservableCollection<BoneVM>();

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
                    RootVisual3D.Children.Remove(liveKinematic.Root.WorldVisual);
                }

                liveKinematic = value;

                if (liveKinematic != null)
                {
                    RootVisual3D.Children.Add(liveKinematic.Root.Visual);
                    RootVisual3D.Children.Add(liveKinematic.Root.WorldVisual);
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

        public ICommand AssignSensorToBoneCommand { get; }

        public ICommand StartCaptureCommand { get; }

        public ICommand StopCaptureCommand { get; }

        public ICommand SetBaseRotationCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppVM()
        {
            dataCollector = new DataCollector();
            dataCollector.SensorAdded += OnSensorAdded;

            // setup sensor-bone links
            SensorBoneMap = new SensorBoneMap();
            LinkVMs = new Dictionary<SensorBoneLink, SensorBoneLinkVM>();
            SensorBoneMap.LinkAdded += (link) =>
            {
                LinkVMs.Add(link, new SensorBoneLinkVM(link));
                RootVisual3D.Children.Add(LinkVMs[link].Visual);
            };
            SensorBoneMap.LinkRemoved += (link) =>
                {
                    RootVisual3D.Children.Remove(LinkVMs[link].Visual);
                    LinkVMs.Remove(link);
                };

            // setup sensors collection
            sensors = new ObservableCollection<SensorVM>();
            Sensors = new ReadOnlyObservableCollection<SensorVM>(sensors);
            foreach (var item in dataCollector.Sensors.Values)
                sensors.Add(new SensorVM(item));

            // setup kinematic chain
            BaseKinematic = new KinematicVM(new Kinematic());

            // setup animator
            Animator = new KinematicAnimatorVM(BaseKinematic, new Dictionary<Bone, List<Quaternion>>());

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

            Dictionary<BVHNode, List<Quaternion>> bvhMotionData;
            BVHNode bvhroot = BVHReader.ReadBvhHierarchy(file, out bvhMotionData);

            Dictionary<Bone, List<Quaternion>> newMotionData = new Dictionary<Bone, List<Quaternion>>();
            Bone newRoot = BVHConverter.ToBones(bvhroot, null, bvhMotionData, newMotionData);

            // create & assign a new kinematic view model. BoneVMs are also created in this process.
            BaseKinematic = new KinematicVM(new Core.Kinematic(newRoot));


            Animator = new KinematicAnimatorVM(BaseKinematic, newMotionData);
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
                SensorBoneMap.CreateLink(bone.Model, sensor.Model);
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

            foreach (var item in LinkVMs.Values)
            {
                item.Refresh();
            }

            if (currentCaptureState == CaptureState.Running)
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
