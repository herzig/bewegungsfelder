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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.Core
{
    public class SensorBoneLink
    {
        private Matrix3D calibrationTransform = Matrix3D.Identity;

        /// <summary>
        /// the bone that is linked to
        /// </summary>
        public Bone Bone { get; }

        /// <summary>
        /// the sensor that is associated with the bone
        /// </summary>
        public Sensor Sensor { get; }

        public CSysBuilder SensorFrameDefinition { get; } = new CSysBuilder();

        /// <summary>
        /// transformation from sensor frame to bone frame. set during the calibration process
        /// </summary>
        public Matrix3D CalibrationTransform
        {
            get { return calibrationTransform; }
            set
            {
                calibrationTransform = value;
                CalibrationRotation = calibrationTransform.ToQuaternion();
            }
        }

        /// <summary>
        ///  the rotation part of the CalibrationTransform matrix.
        /// </summary>
        public Quaternion CalibrationRotation { get; private set; }

        /// <summary>
        /// the  inverted (calibrated) orientation reading when the model is in its base pose.
        /// this rotation is 'subtracted' from the sensors readings to get the final orientation in bone frame.
        /// </summary>
        public Quaternion BaseOrientation { get; private set; }

        public SensorBoneLink(Bone bone, Sensor sensor)
        {
            Bone = bone;
            Sensor = sensor;
        }

        public Quaternion GetCalibratedOrientation()
        {
            return BaseOrientation * Sensor.LastValue.Orientation * CalibrationRotation;
        }

        public Vector3D GetCalibratedAcceleration()
        {
            var m = Matrix3D.Identity;
            m.Rotate(GetCalibratedOrientation());

            return m.Transform(Sensor.LastValue.Acceleration);
        }

        public void SetBaseOrientation()
        {
            BaseOrientation = (Sensor.LastValue.Orientation * CalibrationRotation).Inverted();
        }

        public void CalculateCalibrationTransform()
        {
            Matrix3D source = SensorFrameDefinition.GetMatrix();
            source = source.Transposed();
            Matrix3D target = new Matrix3D(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);

            CalibrationTransform = target * source.Transposed();
        }

    }
}
