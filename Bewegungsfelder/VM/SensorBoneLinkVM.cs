/*
Part of Bewegungsfelder

MIT-License
(C) 2016 Ivo Herzig

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using HelixToolkit.Wpf;
using Bewegungsfelder.Core;
using Bewegungsfelder.Utilities;
using Bewegungsfelder.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.VM
{
    public class SensorBoneLinkVM
    {
        private CSysVisual3D csysVisual;
        // private LinesVisual3D accelerationVisual;

        public bool IsCalibrated { get { return Model.CalibrationTransform != Matrix3D.Identity; } }
        public bool IsNotCalibrated { get { return !IsCalibrated; } }

        public CSysBuilder SensorFrameDefinition { get { return Model.SensorFrameDefinition; } }

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

        public SensorBoneLinkVM(SensorBoneLink model, SensorVM sensor, BoneVM bone)
        {
            this.Model = model;
            this.Sensor = sensor;
            this.Bone = bone;

            // setup visuals
            Visual = new ModelVisual3D();

            csysVisual = new CSysVisual3D();
            csysVisual.XColor = csysVisual.XColor.ChangeSaturationValue(0.3, 0.7);
            csysVisual.YColor = csysVisual.YColor.ChangeSaturationValue(0.3, 0.7);
            csysVisual.ZColor = csysVisual.ZColor.ChangeSaturationValue(0.3, 0.7);
            Visual.Children.Add(csysVisual);

            //accelerationVisual = new LinesVisual3D();
            //accelerationVisual.Color = Colors.Purple;
            //accelerationVisual.Points.Add(new Point3D(0, 0, 0));
            //accelerationVisual.Points.Add(new Point3D(0, 0, 0));
            //Visual.Children.Add(accelerationVisual);
        }

        public void Refresh()
        {
            var boneTransform = Model.Bone.GetRootTransform();

            Matrix3D visualTransform = Matrix3D.Identity;
            visualTransform.Rotate(Model.GetCalibratedOrientation());
            visualTransform.Translate(boneTransform.GetOffset());

            csysVisual.Transform = new MatrixTransform3D(visualTransform);
            csysVisual.Length = DisplaySettings.Get.CSysSize;

            //accelerationVisual.Points.RemoveAt(1);
            //accelerationVisual.Points.Add(Model.GetCalibratedAcceleration().ToPoint3D());
            //accelerationVisual.Transform = new TranslateTransform3D(boneTransform.GetOffset());
        }
    }
}
