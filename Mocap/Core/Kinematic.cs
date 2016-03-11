using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public class Kinematic
    {
        public Bone Root { get; } = new Bone(parent: null) { Name = "Root" };

        public Kinematic() { }

        public void ApplyRotations(Dictionary<Bone, Quaternion> jointRotations)
        {
            Root.Traverse((bone, worldRotation) =>
            {
                if (jointRotations.ContainsKey(bone))
                {
                    bone.JointRotation = worldRotation.Inverted() * jointRotations[bone];
                }
            }, Quaternion.Identity);
        }
    }
}
