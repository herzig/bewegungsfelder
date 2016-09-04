/*
Part of Bewegungsfelder 
(C) 2016 Ivo Herzig

[[LICENSE]]
*/
using Mocap.Utilities;
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
        public const int BUFFER_SIZE = 25 * 10;

        private RingBuffer<SensorValue> data { get; }

        public int Id { get; }

        public IPAddress SourceIp { get; }

        /// <summary>
        /// the last sensor value received.
        /// returns a default SensorValue if no data is recorded yet
        /// </summary>
        public SensorValue LastValue { get { return data.Last; } }

        public void PushValue(SensorValue value)
        {
            data.Push(value);
        }

        public Sensor(IPAddress source, int id)
        {
            this.Id = id;
            this.SourceIp = source;
            this.data = new RingBuffer<SensorValue>(BUFFER_SIZE);
        }

        public SensorValue[] GetDataSince(DateTime t)
        {
            var values = data.Take();
            return values.TakeWhile(v => v.ArrivalTime > t).ToArray();
        }

        public Vector3D AxisFromAcceleration(DateTime calibrationStartTime)
        {
            // get sensor readings for calibration
            SensorValue[] values = GetDataSince(calibrationStartTime);

            // sum gyro readings to identify the principal rotation axis
            Vector3D axis = new Vector3D();
            for (int i = 0; i < values.Length; i++)
            {
                axis += values[i].Acceleration;
            }
            axis.Normalize();
            return axis;
        }

        public Vector3D AxisFromGyro(DateTime calibrationStartTime)
        {
            // get sensor readings for calibration
            SensorValue[] values = GetDataSince(calibrationStartTime);

            // sum gyro readings to identify the principal rotation axis
            Vector3D axis = new Vector3D();
            for (int i = 0; i < values.Length; i++)
            {
                axis.X += Math.Abs(values[i].Gyro.X);
                axis.Y += Math.Abs(values[i].Gyro.Y);
                axis.Z += Math.Abs(values[i].Gyro.Z);
            }
            axis.Normalize();

            return axis;
        }
    }
}
