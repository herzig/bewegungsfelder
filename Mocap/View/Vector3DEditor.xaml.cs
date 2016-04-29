using System;
using System.Collections.Generic;
using System.Linq;
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
using Mocap;

namespace Mocap.View
{
    /// <summary>
    /// Interaction logic for Vector3DEditor.xaml
    /// </summary>
    public partial class Vector3DEditor : UserControl
    {
        public static readonly DependencyProperty VectorProperty =
            DependencyProperty.Register(nameof(Vector), typeof(Vector3D), typeof(Vector3DEditor), new FrameworkPropertyMetadata(new Vector3D(), OnVectorPropertyChanged
                ));

        public Vector3D Vector
        {
            get { return (Vector3D)GetValue(VectorProperty); }
            set { SetValue(VectorProperty, value); }
        }

        public event Action VectorChanged;

        public Vector3DEditor()
        {
            InitializeComponent();
        }

        private static void OnVectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (Vector3DEditor)d;
            obj.VectorChanged?.Invoke();
        }

        private void OnSetVxClick(object sender, RoutedEventArgs e) { Vector = new Vector3D(1, 0, 0); }
        private void OnSetVyClick(object sender, RoutedEventArgs e) { Vector = new Vector3D(0, 1, 0); }
        private void OnSetVzClick(object sender, RoutedEventArgs e) { Vector = new Vector3D(0, 0, 1); }
    }
}