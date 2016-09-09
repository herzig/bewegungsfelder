/*
Part of Bewegungsfelder 

MIT-License 
(C) 2016 Ivo Herzig

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using Bewegungsfelder.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.Core
{
    public class Sensor
    {
        // TODO: Bad magic number depending on sample rate. 25Hz * 10 ~= 10 sec worth of data kept in buffer
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
