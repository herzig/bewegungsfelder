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
        public Bone Root { get; } = new Bone { Name = "Root" };

        public Kinematic()
        {
            Root.Children.Add(new Bone { Offset = new Vector3D(1, 0, 0) });
        }

        public void ApplySensorData()
        {
            Root.ApplySensorData(Matrix3D.Identity);
        }
    }
}
