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
        public Vector3D Acceleration { get; }
        public Vector3D Gyro { get; }
        public Quaternion Orientation { get; }
        public DateTime ArrivalTime { get; }

        /// <summary>
        /// the sensor timestamp in microseconds
        /// </summary>
        public uint SensorTimestamp { get; }

        public SensorValue(Quaternion orientation, Vector3D acceleration, Vector3D gyro, DateTime arrivalTime, uint sensorTime)
        {
            Orientation = orientation;
            ArrivalTime = arrivalTime;
            Acceleration = acceleration;
            Gyro = gyro;
            SensorTimestamp = sensorTime;
        }
    }
}
