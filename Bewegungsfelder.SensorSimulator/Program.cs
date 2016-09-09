using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.SensorSimulator
{
    /*
     * simple exe to send mock sensor values to the server.
    */
    class Program
    {
        static void Main(string[] args)
        {
            int hz = 25;

            Random random = new Random();

            UdpClient client = new UdpClient();

            // setup simualted sensors
            int count = 2; 
            int[] ids = Enumerable.Range(0, count).ToArray();
            double[] deg = new double[count];
            double[] delta = { 0.0, 0.2 };
            Vector3D[] axes = { new Vector3D(1, 0, 0), new Vector3D(0, 0, 1) };

            Stopwatch watch = Stopwatch.StartNew();

            while (true)
            {
                for (int i = 0; i < count; i++)
                {
                    // apply rotations
                    deg[i] += delta[i];
                    Quaternion quat = new Quaternion(axes[i], deg[i]);

                    // datagram format: id,w,x,y,z 
                    byte[] idBytes = BitConverter.GetBytes(ids[i]);

                    var w = BitConverter.GetBytes((int)(quat.W * int.MaxValue));
                    var x = BitConverter.GetBytes((int)(quat.X * int.MaxValue));
                    var y = BitConverter.GetBytes((int)(quat.Y * int.MaxValue));
                    var z = BitConverter.GetBytes((int)(quat.Z * int.MaxValue));

                    byte[] quatBytes = Enumerable.Concat(w, x).Concat(y).Concat(z).ToArray();

                    // 2 * x,y,z for gyro and accelerometer values
                    byte[] gyroAccelBytes = new byte[6 * sizeof(int)];

                    byte[] bytes = Enumerable.Concat(idBytes, quatBytes)
                        .Concat(gyroAccelBytes)
                        .Concat(BitConverter.GetBytes(watch.Elapsed.TotalMilliseconds * 1000)).ToArray();

                    client.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, 5555));
                }

                Thread.Sleep(1000 / hz);
            }
        }
    }
}
