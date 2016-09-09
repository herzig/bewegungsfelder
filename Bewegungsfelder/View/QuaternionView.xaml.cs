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
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Bewegungsfelder.View
{
    /// <summary>
    /// Interaction logic for QuaternionView.xaml
    /// </summary>
    public partial class QuaternionView : UserControl
    {
        // Using a DependencyProperty as the backing store for Quaternion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QuaternionProperty =
            DependencyProperty.Register(nameof(Quaternion), typeof(Quaternion), typeof(QuaternionView),
                new FrameworkPropertyMetadata(Quaternion.Identity, OnQuaternionChanged) { BindsTwoWayByDefault = true });

        // Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle), typeof(double), typeof(QuaternionView),
                new FrameworkPropertyMetadata(0.0, OnAngleChanged) { BindsTwoWayByDefault = true });

        // Using a DependencyProperty as the backing store for Axis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AxisProperty =
            DependencyProperty.Register(nameof(Axis), typeof(Vector3D), typeof(QuaternionView),
                new FrameworkPropertyMetadata(default(Vector3D), OnAxisChanged) { BindsTwoWayByDefault = true });

        // used to avoid premature update of the quaternion during initialisation
        private bool suppressQuaternionUpdate = false;

        public Vector3D Axis
        {
            get { return (Vector3D)GetValue(AxisProperty); }
            set { SetValue(AxisProperty, value); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public Quaternion Quaternion
        {
            get { return (Quaternion)GetValue(QuaternionProperty); }
            set { SetValue(QuaternionProperty, value); }
        }

        public QuaternionView()
        {
            InitializeComponent();
        }

        private static void OnQuaternionChanged(DependencyObject instance, DependencyPropertyChangedEventArgs e)
        {
            var view = (QuaternionView)instance;
            view.suppressQuaternionUpdate = true;
            view.SetValue(AxisProperty, view.Quaternion.Axis);
            view.SetValue(AngleProperty, view.Quaternion.Angle);
            view.suppressQuaternionUpdate = false;
        }
        private static void OnAxisChanged(DependencyObject instance, DependencyPropertyChangedEventArgs e)
        {
            var view = (QuaternionView)instance;
            if (!view.suppressQuaternionUpdate)
                view.SetValue(QuaternionProperty, new Quaternion(view.Axis, view.Angle));
        }
        private static void OnAngleChanged(DependencyObject instance, DependencyPropertyChangedEventArgs e)
        {
            var view = (QuaternionView)instance;

            var axis = view.Axis;
            if (axis == new Vector3D(0, 0, 0))
                axis = new Vector3D(1, 0, 0);

            if (!view.suppressQuaternionUpdate)
                view.SetValue(QuaternionProperty, new Quaternion(axis, view.Angle));
        }

        private void OnAxisDefinitionLostFocus(object sender, RoutedEventArgs e)
        {
            SetValue(AxisProperty, new Vector3D(Double.Parse(tb_x.Text), Double.Parse(tb_y.Text), Double.Parse(tb_z.Text)));
        }
    }
}
