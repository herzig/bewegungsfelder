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
        public Bone Root { get; }

        public Kinematic(Bone root = null)
        {
            if (root == null)
                Root = new Bone(parent: null) { Name = "Root" };
            else
                Root = root;
        }

        public void ApplyWorldRotations(Dictionary<Bone, Quaternion> jointRotations)
        {
            Root.Traverse((bone, worldRotation) =>
            {
                if (jointRotations.ContainsKey(bone))
                {
                    bone.JointRotation = worldRotation.Inverted() * jointRotations[bone];
                }
            }, Quaternion.Identity);
        }

        public void ApplyLocalRotation(Dictionary<Bone,Quaternion> jointRotations)
        {
            Root.Traverse((bone) =>
            {
                if (jointRotations.ContainsKey(bone))
                {
                    bone.JointRotation = jointRotations[bone];
                }
            });
        }
    }
}
