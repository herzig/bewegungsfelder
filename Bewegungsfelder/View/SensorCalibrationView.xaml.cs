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

using Bewegungsfelder.Core;
using Bewegungsfelder.VM;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Bewegungsfelder.View
{
    public partial class SensorCalibrationView : UserControl
    {
        public const int CalibrationDuration = 5;

        public static readonly DependencyProperty SensorBoneLinkProperty =
            DependencyProperty.Register("SensorBoneLink", typeof(SensorBoneLinkVM), typeof(SensorCalibrationView), new PropertyMetadata(null));

        public static readonly DependencyProperty VxCheckedProperty =
            DependencyProperty.Register("VxChecked", typeof(bool), typeof(SensorCalibrationView), new PropertyMetadata(true));

        public static readonly DependencyProperty VyCheckedProperty =
            DependencyProperty.Register("VyChecked", typeof(bool), typeof(SensorCalibrationView), new PropertyMetadata(false));

        public static readonly DependencyProperty VzCheckedProperty =
            DependencyProperty.Register("VzChecked", typeof(bool), typeof(SensorCalibrationView), new PropertyMetadata(true));

        public SensorBoneLinkVM SensorBoneLink
        {
            get { return (SensorBoneLinkVM)GetValue(SensorBoneLinkProperty); }
            set { SetValue(SensorBoneLinkProperty, value); }
        }

        public bool VxChecked
        {
            get { return (bool)GetValue(VxCheckedProperty); }
            set { SetValue(VxCheckedProperty, value); }
        }

        public bool VyChecked
        {
            get { return (bool)GetValue(VyCheckedProperty); }
            set { SetValue(VyCheckedProperty, value); }
        }

        public bool VzChecked
        {
            get { return (bool)GetValue(VzCheckedProperty); }
            set { SetValue(VzCheckedProperty, value); }
        }

        private Action calibrationCompletedAction;

        private Storyboard calibrationAnimation;

        public SensorCalibrationView()
        {
            InitializeComponent();

            calibrationAnimation = (Storyboard)FindResource("anim_calibration");
            calibrationAnimation.Duration = new Duration(TimeSpan.FromSeconds(CalibrationDuration));

        }


        private void OnProgressCompleted(object sender, EventArgs e)
        {
            tb_calibmsg.Visibility = Visibility.Hidden;
            calibrationCompletedAction();
        }

        private void OnRefreshSensorFrameClick(object sender, RoutedEventArgs e)
        {
            SensorBoneLink.SensorFrameDefinition.CalculateVectors();
            SensorBoneLink.Model.CalculateCalibrationTransform();
        }

        private void OnDefineZByAccel(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            tb_calibmsg.Visibility = Visibility.Visible;
            tb_calibmsg.Text = "Accelerate in selected direction";

            calibrationCompletedAction = () =>
            {
                SensorBoneLink.SensorFrameDefinition.Row3 = SensorBoneLink.Model.Sensor.AxisFromAcceleration(startTime);
            };
            calibrationAnimation.Begin();
        }

        private void OnDefineZByGyro(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            tb_calibmsg.Visibility = Visibility.Visible;
            tb_calibmsg.Text = "Rotate around selected axis";

            calibrationCompletedAction = () =>
            {
                SensorBoneLink.SensorFrameDefinition.Row3 = SensorBoneLink.Model.Sensor.AxisFromGyro(startTime);
            };
            calibrationAnimation.Begin();
        }

        private void OnDefineYByAccel(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            tb_calibmsg.Visibility = Visibility.Visible;
            tb_calibmsg.Text = "Accelerate in selected direction";

            calibrationCompletedAction = () =>
            {
                SensorBoneLink.SensorFrameDefinition.Row2 = SensorBoneLink.Model.Sensor.AxisFromAcceleration(startTime);
            };
            calibrationAnimation.Begin();
        }

        private void OnDefineYByGyro(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            tb_calibmsg.Visibility = Visibility.Visible;
            tb_calibmsg.Text = "Rotate around selected axis";

            calibrationCompletedAction = () =>
            {
                SensorBoneLink.SensorFrameDefinition.Row2 = SensorBoneLink.Model.Sensor.AxisFromGyro(startTime);
            };
            calibrationAnimation.Begin();
        }

        private void OnDefineXByAccel(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            tb_calibmsg.Visibility = Visibility.Visible;
            tb_calibmsg.Text = "Accelerate in selected direction";

            calibrationCompletedAction = () =>
            {
                SensorBoneLink.SensorFrameDefinition.Row1 = SensorBoneLink.Model.Sensor.AxisFromAcceleration(startTime);
            };
            calibrationAnimation.Begin();
        }

        private void OnDefineXByGyro(object sender, RoutedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            tb_calibmsg.Visibility = Visibility.Visible;
            tb_calibmsg.Text = "Rotate around selected axis";

            calibrationCompletedAction = () =>
            {
                SensorBoneLink.SensorFrameDefinition.Row1 = SensorBoneLink.Model.Sensor.AxisFromGyro(startTime);
            };
            calibrationAnimation.Begin();
        }
    }
}
