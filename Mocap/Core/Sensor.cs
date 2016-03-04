using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mocap.Core
{
    public class Sensor
    {
        public int Id { get; }

        public IPAddress SourceIp { get; }

        public ConcurrentStack<SensorValue> Data { get; }

        /// <summary>
        /// the last sensor value received.
        /// returns a default SensorValue if no data recorded yet
        /// </summary>
        public SensorValue LastValue
        {
            get
            {
                SensorValue value;
                if (Data.TryPeek(out value))
                    return value;
                else
                    return default(SensorValue);
            }
        }

        public Sensor(IPAddress source, int id)
        {
            this.Id = id;
            this.SourceIp = source;
            this.Data = new ConcurrentStack<SensorValue>();
        }
    }
}
