using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public class Sensor
    {
        private bool recordPosition = false;

        public int Id { get; }

        public IPAddress SourceIp { get; }

        private ConcurrentStack<SensorValue> data { get; }

        public int Count { get { return data.Count; } }

        public Point3D Position { get; private set; }
        public Vector3D Velocity { get; private set; }
        public Vector3D Gravity { get; private set; }

        /// <summary>
        /// the last sensor value received.
        /// returns a default SensorValue if no data is recorded yet
        /// </summary>
        public SensorValue LastValue
        {
            get
            {
                SensorValue value;
                if (data.TryPeek(out value))
                    return value;
                else
                    return default(SensorValue);
            }
        }

        public void PushValue(SensorValue value)
        {
            data.Push(value);

            if (recordPosition)
            {
                Velocity += (value.Acceleration - Gravity)*(1/10.0);
                Position += Velocity*(1/10.0);
            }
        }

        public Sensor(IPAddress source, int id)
        {
            this.Id = id;
            this.SourceIp = source;
            this.data = new ConcurrentStack<SensorValue>();
        }

        public Vector3D GetAverageAcceleration(int numSamples = 30)
        {
            Vector3D sum = new Vector3D();
            foreach (var item in data.Take(numSamples))
            {
                sum += item.Acceleration;
            }
            return sum / numSamples;
        }

        public void StartPositionIntegration()
        {
            Velocity = new Vector3D();
            Position = new Point3D();
            Gravity = GetAverageAcceleration(numSamples: 50);
            recordPosition = true;
        }
    }
}
