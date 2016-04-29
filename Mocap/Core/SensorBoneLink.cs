using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
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

        /// <summary>
        /// pairs of axes selected in the calibration process
        /// item1 is the source (in sensor frame). item2 is the target axis (in model frame)
        /// </summary>
        public List<Tuple<Vector3D, Vector3D>> CalibrationAxes { get; } = new List<Tuple<Vector3D, Vector3D>>();

        public SensorBoneLink(Bone bone, Sensor sensor)
        {
            Bone = bone;
            Sensor = sensor;
        }

        public void ClearCalibration()
        {
            CalibrationAxes.Clear();
            CalibrationTransform = Matrix3D.Identity;
        }

        public Quaternion GetCalibratedOrientation()
        {
            return (BaseOrientation * CalibrationRotation) * Sensor.LastValue.Orientation;
        }

        public Vector3D GetCalibratedAcceleration()
        {
            var m = Matrix3D.Identity;
            m.Rotate(GetCalibratedOrientation());

            return m.Transform(Sensor.LastValue.Acceleration);
        }

        public void SetBaseOrientation()
        {
            BaseOrientation = (CalibrationRotation * Sensor.LastValue.Orientation).Inverted();
        }

        public void CalculateCalibrationTransform()
        {
            if (CalibrationAxes.Count != 2)
                throw new InvalidOperationException("two axes needed for calibration");

            Vector3D target1 = CalibrationAxes[0].Item2;
            Vector3D target2 = CalibrationAxes[1].Item2;
            Vector3D target3 = Vector3D.CrossProduct(target1, target2);
            target3.Normalize();

            Vector3D source1 = CalibrationAxes[0].Item1;
            Vector3D source2 = CalibrationAxes[1].Item1;
            Vector3D source3 = Vector3D.CrossProduct(source1, source2);
            source3.Normalize();

            //Matrix3D target = new Matrix3D(
            //    target1.X, target1.Y, target1.Z, 0,
            //    target2.X, target2.Y, target2.Z, 0,
            //    target3.X, target3.Y, target3.Z, 0,
            //    0, 0, 0, 1);
            //Matrix3D source = new Matrix3D(
            //    source1.X, source1.Y, source1.Z, 0,
            //    source2.X, source2.Y, source2.Z, 0,
            //    source3.X, source3.Y, source3.Z, 0,
            //    0, 0, 0, 1);

            Matrix3D target = new Matrix3D(
                target1.X, target2.X, target3.X, 0,
                target1.Y, target2.Y, target3.Y, 0,
                target1.Z, target2.Z, target3.Z, 0,
                0, 0, 0, 1);
            Matrix3D source = new Matrix3D(
                source1.X, source2.X, source3.X, 0,
                source1.Y, source2.Y, source3.Y, 0,
                source1.Z, source2.Z, source3.Z, 0,
                0, 0, 0, 1);

            source.Invert();
            CalibrationTransform = target * source;
        }

        public void AddCalibrationAxisFromAccel(DateTime calibrationStartTime, Vector3D targetAxis)
        {
            if (CalibrationAxes.Count >= 2)
                throw new InvalidOperationException("Can't define more than two axes for calibration");

            // get sensor readings for calibration
            SensorValue[] values = Sensor.GetDataSince(calibrationStartTime);

            // sum gyro readings to identify the principal rotation axis
            Vector3D axis = new Vector3D();
            for (int i = 0; i < values.Length; i++)
            {
                axis += values[i].Acceleration;
            }
            axis.Normalize();

            if (CalibrationAxes.Count == 1)
            {
                // Check if axes are more or less perpendicular.
                double angleTolerance = 5;
                var angle = Vector3D.AngleBetween(CalibrationAxes.First().Item1, axis);
                if (angle < 90 + angleTolerance && angle > 90 - angleTolerance)
                {
                    CalibrationAxes.Add(new Tuple<Vector3D, Vector3D>(axis, targetAxis));
                }
            }

            if (CalibrationAxes.Count == 2)
                CalculateCalibrationTransform();
        }

        public void AddCalibrationAxisFromGyro(DateTime calibrationStartTime, Vector3D targetAxis)
        {
            if (CalibrationAxes.Count >= 2)
                throw new InvalidOperationException("Can't define more than two axes for calibration");

            // get sensor readings for calibration
            SensorValue[] values = Sensor.GetDataSince(calibrationStartTime);

            // sum gyro readings to identify the principal rotation axis
            Vector3D axis = new Vector3D();
            for (int i = 0; i < values.Length; i++)
            {
                axis.X += Math.Abs(values[i].Gyro.X);
                axis.Y += Math.Abs(values[i].Gyro.Y);
                axis.Z += Math.Abs(values[i].Gyro.Z);
            }
            axis.Normalize();

            if (CalibrationAxes.Count == 1)
            {
                // Check if axes are more or less perpendicular.
                double angleTolerance = 20;
                var angle = Vector3D.AngleBetween(CalibrationAxes.First().Item1, axis);
                if (angle < 90 + angleTolerance && angle > 90 - angleTolerance)
                {
                    Vector3D firstAxis = CalibrationAxes[0].Item1;
                    Vector3D tmp = Vector3D.CrossProduct(axis, firstAxis);
                    Vector3D newAxis = Vector3D.CrossProduct(firstAxis, tmp);

                    CalibrationAxes.Add(new Tuple<Vector3D, Vector3D>(newAxis, targetAxis));
                }
            }
            else
            {
                CalibrationAxes.Add(new Tuple<Vector3D, Vector3D>(axis, targetAxis));
            }

            if (CalibrationAxes.Count == 2)
                CalculateCalibrationTransform();
        }
    }
}
