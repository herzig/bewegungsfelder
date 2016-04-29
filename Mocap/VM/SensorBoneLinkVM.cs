using HelixToolkit.Wpf;
using Mocap.Core;
using Mocap.Utilities;
using Mocap.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Mocap.VM
{
    public class SensorBoneLinkVM: INotifyPropertyChanged
    {
        private CSysVisual3D csysVisual;
        private LinesVisual3D accelerationVisual;

        public bool IsCalibrated { get { return Model.CalibrationTransform != Matrix3D.Identity; } }
        public bool IsNotCalibrated { get { return !IsCalibrated; } }

        public int CalibrationAxesCount { get { return Model.CalibrationAxes.Count; } }

        public CSysBuilder SensorFrameDefinition { get; } = new CSysBuilder();

        /// <summary>
        /// the underlying model
        /// </summary>
        public SensorBoneLink Model { get; }

        /// <summary>
        /// model display on main 3d viewport
        /// </summary>
        public ModelVisual3D Visual { get; }

        /// <summary>
        /// the linked sensor
        /// </summary>
        public SensorVM Sensor{ get; }

        /// <summary>
        /// the linked bone
        /// </summary>
        public BoneVM Bone{ get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public SensorBoneLinkVM(SensorBoneLink model, SensorVM sensor, BoneVM bone)
        {
            this.Model = model;
            this.Sensor = sensor;
            this.Bone = bone;

            // setup commands

            // setup visual
            Visual = new ModelVisual3D();

            csysVisual = new CSysVisual3D();
            csysVisual.XColor = csysVisual.XColor.ChangeSaturationValue(0.3, 0.7);
            csysVisual.YColor = csysVisual.YColor.ChangeSaturationValue(0.3, 0.7);
            csysVisual.ZColor = csysVisual.ZColor.ChangeSaturationValue(0.3, 0.7);
            Visual.Children.Add(csysVisual);

            accelerationVisual = new LinesVisual3D();
            accelerationVisual.Color = Colors.Purple;
            accelerationVisual.Points.Add(new Point3D(0, 0, 0));
            accelerationVisual.Points.Add(new Point3D(0, 0, 0));
            Visual.Children.Add(accelerationVisual);
        }

        public void ClearCalibration()
        {
            Model.ClearCalibration();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCalibrated)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotCalibrated)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalibrationAxesCount)));
        }

        public void AddCalibrationAxisFromGyro(DateTime calibrationStart, Vector3D targetAxis)
        {
            Model.AddCalibrationAxisFromGyro(calibrationStart, targetAxis);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CalibrationAxesCount)));
        }

        public void CalculateCalibrationTransform()
        {
            Model.CalculateCalibrationTransform();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCalibrated)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotCalibrated)));
        }

        public void Refresh()
        {
            var boneTransform = Model.Bone.GetRootTransform();

            Matrix3D visualTransform = Matrix3D.Identity;
            visualTransform.Rotate(Model.GetCalibratedOrientation());
            visualTransform.Translate(boneTransform.GetOffset());

            csysVisual.Transform = new MatrixTransform3D(visualTransform);
            csysVisual.Length = DisplaySettings.Get.CSysSize;

            accelerationVisual.Points.RemoveAt(1);
            accelerationVisual.Points.Add(Model.GetCalibratedAcceleration().ToPoint3D());
            accelerationVisual.Transform = new TranslateTransform3D(boneTransform.GetOffset());
        }
    }
}
