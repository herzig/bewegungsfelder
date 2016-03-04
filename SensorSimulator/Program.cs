using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SensorSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            int hz = 60;

            Random random = new Random();

            UdpClient client = new UdpClient();

            int count = 2;
            int[] ids = new int[count];
            double[] deg = new double[count];
            double[] delta = { 0.0, 0.2 };
            Vector3D[] axes = { new Vector3D(1, 0, 0), new Vector3D(0, 0, 1) };

            for (int i = 0; i < count; i++)
                ids[i] = i;


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

                    byte[] bytes = Enumerable.Concat(idBytes, quatBytes).ToArray();

                    client.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, 5555));
                }

                Thread.Sleep(1000 / hz);
            }
        }
    }
}
