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
        private Quaternion localRotation = Quaternion.Identity;
        private Quaternion globalRotation = Quaternion.Identity;
        private Vector3D offset = new Vector3D();

        public Matrix3D LocalTransform { get; private set; }

        public Quaternion JointRotation
        {
            get { return localRotation; }
            set
            {
                localRotation = value;
                UpdateLocalTransform();
            }
        }

        public Quaternion GlobalRotation { get { return globalRotation; } set { globalRotation = value; } }

        public Vector3D Offset
        {
            get { return offset; }
            set
            {
                offset = value;
                UpdateLocalTransform();
            }
        }

        public string Name { get; set; }

        public Sensor Sensor { get; set; }

        public Bone Parent { get; }

        public List<Bone> Children { get; } = new List<Bone>();

        public Bone(Bone parent, string name = "bone", Vector3D offset = default(Vector3D))
        {
            Parent = parent;
            Name = name;
            Offset = offset;
        }

        private void UpdateLocalTransform()
        {
            var mat = Matrix3D.Identity;
            mat.Rotate(JointRotation);
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