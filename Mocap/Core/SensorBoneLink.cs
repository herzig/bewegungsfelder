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

        public SensorBoneLink(Bone bone, Sensor sensor)
        {
            Bone = bone;
            Sensor = sensor;
        }
    }
}
