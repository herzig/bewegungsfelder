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

namespace Mocap.Core
{
    public class DataCollector
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
