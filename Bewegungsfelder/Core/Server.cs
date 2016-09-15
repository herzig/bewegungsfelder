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
using System.Diagnostics;
using System.Web.Http.SelfHost;
using Fleck;
using System.Web.Http.Routing;
using System.Web.Http;
using System.Net;

namespace Bewegungsfelder.Core
{
    public class Server
    {
        // used for both udp and tcp (websocket) server.
        public const int DATA_PORT = 5555;

        public ConcurrentDictionary<int, Sensor> Sensors { get; } = new ConcurrentDictionary<int, Sensor>();

        // the synchronisation context that was used when the server was started.
        // used to invoke events on the main thread
        private Dispatcher startedDispatcher;

        private Task udpListenerTask;

        private WebSocketServer webSocketServer;
        private HttpSelfHostServer httpServer;

        public event Action<Sensor> SensorAdded;

        public void Start()
        {
            if (udpListenerTask != null)
                throw new InvalidOperationException("Task is already running");

            // start udp listener
            startedDispatcher = Dispatcher.CurrentDispatcher;
            udpListenerTask = UdpListenAsync();

            // start http server 
            var httpConfig = new HttpSelfHostConfiguration("http://0.0.0.0:8080");
            httpConfig.MessageHandlers.Add(new StaticServeHandler());

            var route = new HttpRoute("");
            httpConfig.Routes.Add("DefaultAPI", route);
            httpServer = new HttpSelfHostServer(httpConfig);
            httpServer.OpenAsync().Wait();

            // start websocket server
            webSocketServer = new WebSocketServer($"ws://0.0.0.0:{DATA_PORT}");
            webSocketServer.Start(OnWebsocketConnection);
        }

        private void OnWebsocketConnection(IWebSocketConnection socket)
        {
            socket.OnOpen = () => Debug.WriteLine(
                $"Websocket client {socket.ConnectionInfo.ClientIpAddress} connected.");
            socket.OnClose = () => Debug.WriteLine(
                $"Websocket client {socket.ConnectionInfo.ClientIpAddress} disconnected.");

            socket.OnMessage = msg =>
            {
                Debug.WriteLine($"Websocket msg from {socket.ConnectionInfo.ClientIpAddress}: {msg}");

                var tokens = msg.Split(',');

                int sensorId = Int32.Parse(tokens[0]);
                var quat = new Quaternion();
                var accel = new Vector3D();
                var gyro = new Vector3D();

                quat.W = float.Parse(tokens[1]);
                quat.X = float.Parse(tokens[2]);
                quat.Y = float.Parse(tokens[3]);
                quat.Z = float.Parse(tokens[4]);
                accel.X = float.Parse(tokens[5]);
                accel.Y = float.Parse(tokens[6]);
                accel.Z = float.Parse(tokens[7]);
                gyro.X = float.Parse(tokens[8]);
                gyro.Y = float.Parse(tokens[9]);
                gyro.Z = float.Parse(tokens[10]);
                uint timestamp = (uint)ulong.Parse(tokens[11]);

                var value = new SensorValue(quat, accel, gyro, DateTime.Now, timestamp);
                var sourceAddr = IPAddress.Parse(socket.ConnectionInfo.ClientIpAddress);

                // TODO: check performance. this creates a closure on every iteration?
                var sensor = Sensors.GetOrAdd(sensorId, (id) =>
                {
                    var newSensor = new Sensor(sourceAddr, id);

                    // raises the sensor added event on the main thread
                    startedDispatcher.BeginInvoke(SensorAdded, newSensor);
                    return newSensor;
                });

                sensor.PushValue(value);
            };

        }

        private Task UdpListenAsync()
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

                    // TODO: check performance. this creates a closure on every iteration?
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
