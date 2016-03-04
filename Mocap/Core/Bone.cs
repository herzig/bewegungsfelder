using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public class Bone
    {
        private Quaternion localRotation;
        private Vector3D offset;

        public Matrix3D LocalTransform { get; private set; }

        public Quaternion LocalRotation
        {
            get { return localRotation; }
            set
            {
                localRotation = value;
                UpdateLocalTransform();
            }
        }

        public Vector3D Offset
        {
            get { return offset; }
            set
            {
                offset = value;
                UpdateLocalTransform();
            }
        }

        //public Quaternion GlobalRotation { get; set; }

        public string Name { get; set; }

        public Sensor Sensor { get; set; }

        public List<Bone> Children { get; } = new List<Bone>();

        public Bone(string name = "bone", Vector3D offset = default(Vector3D))
        {
            Name = name;
            Offset = offset;
        }

        private void UpdateLocalTransform()
        {
            var mat = Matrix3D.Identity;
            mat.Rotate(LocalRotation);
            mat.Translate(Offset);

            LocalTransform = mat;
        }

        public void ApplySensorData(Matrix3D globalToLocal)
        {
            if (Sensor == null)
                return;

            Matrix3D inverse = globalToLocal;
            globalToLocal.Invert();

            Matrix3D globalRotation = Matrix3D.Identity;
            globalRotation.Rotate(Sensor.LastValue.Orientation);

            Matrix3D local = globalRotation * globalToLocal;

            local.Translate(Offset);
            LocalTransform = local;

            globalToLocal.Append(local);

            foreach (var child in Children)
            {
                child.ApplySensorData(globalToLocal);
            }
        }
    }
}
