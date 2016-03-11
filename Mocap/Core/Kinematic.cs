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

        public Kinematic()
        {
        }

        public void ApplySensorData()
        {
            Root.ApplySensorData(Matrix3D.Identity);
        }
    }
}
