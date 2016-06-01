/*
Part of Bewegungsfelder 
(C) 2016 Ivo Herzig

[[LICENSE]]
*/

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
            Recording = 2,
        }

        private DispatcherTimer timer;

        private State animatorState = State.Paused;

        private int playbackPosition;

        public double FPS
        {
            get { return MotionData.FPS; }
            set
            {
                if (MotionData.FPS != value)
                {
                    MotionData.FPS = value;

                    value = Math.Max(1, value);
                    TimeSpan interval = TimeSpan.FromSeconds(1.0 / value);
                    timer.Interval = interval;

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
                if (MotionData.Data.Count == 0)
                    return 0;

                return MotionData.FrameCount;
            }
        }

        public KinematicVM Kinematic { get; }

        public ICommand PlayCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand RecordCommand { get; }

        public ICommand ClearCommand { get; }

        public State AnimatorState
        {
            get { return animatorState; }
            set
            {
                if (animatorState != value)
                {
                    animatorState = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnimatorState)));
                }
            }

        }

        public MotionData MotionData { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public KinematicAnimatorVM(KinematicVM kinematic, MotionData motionData)
        {
            this.Kinematic = kinematic;
            this.MotionData = motionData;

            // setup commands
            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            RecordCommand = new RelayCommand(Record, CanRecord);
            ClearCommand = new RelayCommand(ClearData, CanClear);

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            TimeSpan interval = TimeSpan.FromSeconds(1.0 / motionData.FPS);
            timer.Interval = interval;
            timer.Tick += OnTimerTick;
        }


        private void PlaybackPositionChanged()
        {
            if (PlaybackPosition >= Length)
            {
                Pause();
            }

            Dictionary<Bone, Quaternion> currentFramePose = new Dictionary<Bone, Quaternion>();
            foreach (var item in MotionData.Data)
            {
                currentFramePose.Add(item.Key, item.Value[playbackPosition - 1]);
            }

            Kinematic.Model.ApplyLocalRotation(currentFramePose);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (AnimatorState != State.Playback && AnimatorState != State.Recording)
                throw new InvalidOperationException("Player state is not Playback/Recording");

            if (AnimatorState == State.Recording)
            {
                Kinematic.Model.Root.Traverse(bone =>
                {
                    if (bone.Children.Count == 0)
                        return; // skip end nodes

                    if (!MotionData.Data.ContainsKey(bone))
                    {
                        MotionData.Data.Add(bone, new List<Quaternion>());
                    }
                    MotionData.Data[bone].Add(bone.JointRotation);

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
                });
            }
            else
            {
                ++PlaybackPosition;
            }
        }

        private void Play()
        {
            AnimatorState = State.Playback;

            timer.Start();
        }

        private bool CanPlay()
        {
            return AnimatorState == State.Paused;
        }

        private void Pause()
        {
            AnimatorState = State.Paused;

            timer.Stop();
        }

        private bool CanPause()
        {
            return AnimatorState == State.Playback;
        }

        private void Record()
        {
            if (AnimatorState == State.Recording)
            { // stop recording
                AnimatorState = State.Paused;
                timer.Stop();
            }
            else
            { // start recording
                AnimatorState = State.Recording;
                timer.Start();
            }
        }


        private void ClearData()
        {
            MotionData.Data.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
        }

        private bool CanClear()
        {
            return AnimatorState == State.Paused;
        }

        private bool CanRecord()
        {
            return AnimatorState == State.Paused || AnimatorState == State.Recording;
        }
    }
}
