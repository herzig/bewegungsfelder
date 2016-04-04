using GalaSoft.MvvmLight.CommandWpf;
using Mocap.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Mocap.VM
{
    public class KinematicAnimatorVM : INotifyPropertyChanged
    {
        public enum State
        {
            Paused = 0,
            Playback = 1,
        }

        private DispatcherTimer timer;

        private State PlaybackState = State.Paused;

        private int playbackPosition;

        public double FPS
        {
            get { return 1.0 / timer.Interval.TotalSeconds; }
            set
            {
                value = Math.Max(1, value);
                TimeSpan v = TimeSpan.FromSeconds(1.0 / value);
                if (timer.Interval != v)
                {
                    timer.Interval = v;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FPS)));
                }
            }
        }

        public int PlaybackPosition
        {
            get { return playbackPosition; }
            set
            {
                value = Math.Min(Length, Math.Max(1, value)); // clamp to allowed [1,Length]

                if (playbackPosition != value)
                {
                    playbackPosition = value;

                    PlaybackPositionChanged();

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaybackPosition)));
                }
            }
        }

        public int Length
        {
            get
            {
                if (MotionData.Count == 0)
                    return 0;

                return MotionData.First().Value.Count;
            }
        }

        public KinematicVM Kinematic { get; }

        public ICommand PlayCommand { get; }

        public ICommand PauseCommand { get; }

        public Dictionary<Bone, List<Quaternion>> MotionData { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public KinematicAnimatorVM(KinematicVM kinematic, Dictionary<Bone, List<Quaternion>> motionData)
        {
            this.Kinematic = kinematic;
            this.MotionData = motionData;

            // setup commands
            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Tick += OnTimerTick;
            FPS = 10;
        }

        private void PlaybackPositionChanged()
        {
            if (PlaybackPosition >= Length)
            {
                Pause();
            }

            Dictionary<Bone, Quaternion> currentFramePose = new Dictionary<Bone, Quaternion>();

            foreach (var item in MotionData)
            {
                currentFramePose.Add(item.Key, item.Value[playbackPosition - 1]);
            }

            Kinematic.Model.ApplyLocalRotation(currentFramePose);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (PlaybackState != State.Playback)
                throw new InvalidOperationException("Player state is not Playback");

            PlaybackPosition++;
        }

        private void Play()
        {
            PlaybackState = State.Playback;

            timer.Start();

        }

        private bool CanPlay()
        {
            return PlaybackState == State.Paused;
        }

        private void Pause()
        {
            PlaybackState = State.Paused;

            timer.Stop();
        }

        private bool CanPause()
        {
            return PlaybackState == State.Playback;
        }





    }
}
