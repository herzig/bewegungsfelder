using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mocap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RotateTransform3D RotateTransform { get; }
        public QuaternionRotation3D Rotation { get; } = new QuaternionRotation3D();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            DataContext = this;

            RotateTransform = new RotateTransform3D(Rotation);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UdpClient listener = new UdpClient(5555);

            while (true)
            {
                UdpReceiveResult result = await listener.ReceiveAsync();

                int sensorId = BitConverter.ToInt32(result.Buffer, 0);

                var quat = new Quaternion();
                quat.W = BitConverter.ToInt32(result.Buffer, 4);
                quat.X = BitConverter.ToInt32(result.Buffer, 8);
                quat.Y = BitConverter.ToInt32(result.Buffer, 12);
                quat.Z = BitConverter.ToInt32(result.Buffer, 16);
                quat.Normalize();

                Rotation.Quaternion = quat;
            }
        }
    }
}
