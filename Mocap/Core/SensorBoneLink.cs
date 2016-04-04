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
        public Bone Bone { get; }

        public Sensor Sensor { get; }

        public Quaternion BaseRotation { get; private set; }

        public SensorBoneLink(Bone bone, Sensor sensor)
        {
            Bone = bone;
            Sensor = sensor;
        }

        public Quaternion GetCalibratedOrientation()
        {
            return BaseRotation * Sensor.LastValue.Orientation;
        }

        public Vector3D GetCalibratedAcceleration()
        {
            var m =Matrix3D.Identity;
            m.Rotate(GetCalibratedOrientation());

            //return Sensor.LastValue.Acceleration;
            return m.Transform(Sensor.LastValue.Acceleration);
        }

        public void SetBaseRotation()
        {
            var gravity = Sensor.GetAverageAcceleration(30);
            gravity.Normalize();

            //Vector3D upvector = new Vector3D(0,1,0);
            //Vector3D axis = Vector3D.CrossProduct(gravity, upvector);
            //double angle = Vector3D.AngleBetween(gravity, upvector);

            //Quaternion rotation = new Quaternion(axis, angle);

            BaseRotation = /*rotation **/ Sensor.LastValue.Orientation.Inverted() * Bone.GetRootRotation();
        }
    }
}
