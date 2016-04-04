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
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Mocap.VM
{
    public class SensorBoneLinkVM
    {
        private CSysVisual3D csysVisual;
        private LinesVisual3D accelerationVisual;

        public SensorBoneLink Model { get; }

        public ModelVisual3D Visual { get; }

        public SensorBoneLinkVM(SensorBoneLink model)
        {
            this.Model = model;

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
            accelerationVisual.Transform = new TranslateTransform3D(boneTransform.OffsetX, boneTransform.OffsetY, boneTransform.OffsetZ);
        }
    }
}
