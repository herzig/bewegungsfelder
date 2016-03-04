using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public class SensorValue
    {
        public Quaternion Orientation { get; }
        public DateTime ArrivalTime { get; }

        public SensorValue(Quaternion orientation, DateTime arrivalTime)
        {
            Orientation = orientation;
            ArrivalTime = arrivalTime;
        }
    }
}
