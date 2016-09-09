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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Bewegungsfelder.Core
{
    public class Server
    {
        public const int DATA_PORT = 5555;

        public ConcurrentDictionary<int, Sensor> Sensors { get; } = new ConcurrentDictionary<int, Sensor>();

        // the synchronisation context that was used when the server was started.
        // used to invoke events on the main thread
        private Dispatcher startedDispatcher;
        private Task listenerTask;

        public event Action<Sensor> SensorAdded;

        public void Start()
        {
            if (listenerTask != null)
                throw new InvalidOperationException("Task is already running");

            startedDispatcher = Dispatcher.CurrentDispatcher;
            listenerTask = ListenAsync();
        }

        private Task ListenAsync()
        {
            var task = new Task(async () =>
            {
                UdpClient listener = new UdpClient(DATA_PORT);

                while (true)
                {
                    UdpReceiveResult result = await listener.ReceiveAsync();

                    int sensorId = BitConverter.ToInt32(result.Buffer, 0);

                    var accel = new Vector3D();
                    var gyro = new Vector3D();
                    var quat = new Quaternion();
                    int i = 1;
                    quat.W = BitConverter.ToInt32(result.Buffer, i++ * sizeof(int));
                    quat.X = BitConverter.ToInt32(result.Buffer, i++ * sizeof(int));
                    quat.Y = BitConverter.ToInt32(result.Buffer, i++ * sizeof(int));
                    quat.Z = BitConverter.ToInt32(result.Buffer, i++ * sizeof(int));
                    accel.X = BitConverter.ToInt16(result.Buffer, i++ * sizeof(int));
                    accel.Y = BitConverter.ToInt16(result.Buffer, i++ * sizeof(int));
                    accel.Z = BitConverter.ToInt16(result.Buffer, i++ * sizeof(int));
                    gyro.X = BitConverter.ToInt16(result.Buffer, i++ * sizeof(int));
                    gyro.Y = BitConverter.ToInt16(result.Buffer, i++ * sizeof(int));
                    gyro.Z = BitConverter.ToInt16(result.Buffer, i++ * sizeof(int));
                    uint timestamp = BitConverter.ToUInt32(result.Buffer, i++ * sizeof(int));

                    accel = accel / 8192;
                    gyro = gyro / 16.4;
                    quat.Normalize();

                    var value = new SensorValue(quat, accel, gyro, DateTime.Now, timestamp);

                    var sourceAddr = result.RemoteEndPoint.Address;

                    // TODO: check performance. this creates a closure on every iteration.
                    var sensor = Sensors.GetOrAdd(sensorId, (id) =>
                    {
                        var newSensor = new Sensor(sourceAddr, id);

                        // raises the sensor added event on the main thread
                        startedDispatcher.BeginInvoke(SensorAdded, newSensor);
                        return newSensor;
                    });

                    sensor.PushValue(value);
                }
            }, TaskCreationOptions.LongRunning);
            task.Start();

            return task;
        }
    }
}
