using Mocap.Core;
using Mocap.VM;
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

namespace Mocap.View
{
    /// <summary>
    /// Interaction logic for CalibrationAxesSelector.xaml
    /// </summary>
    public partial class CalibrationAxesSelectorView : UserControl
    {
        public static readonly DependencyProperty SensorBoneLinkProperty =
            DependencyProperty.Register("SensorBoneLink", typeof(SensorBoneLinkVM), typeof(CalibrationAxesSelectorView), new PropertyMetadata(null));

        public static readonly DependencyProperty VxCheckedProperty =
            DependencyProperty.Register("VxChecked", typeof(bool), typeof(CalibrationAxesSelectorView), new PropertyMetadata(true));

        public static readonly DependencyProperty VyCheckedProperty =
            DependencyProperty.Register("VyChecked", typeof(bool), typeof(CalibrationAxesSelectorView), new PropertyMetadata(false));

        public static readonly DependencyProperty VzCheckedProperty =
            DependencyProperty.Register("VzChecked", typeof(bool), typeof(CalibrationAxesSelectorView), new PropertyMetadata(true));

        private DateTime startTime;

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

        public CalibrationAxesSelectorView()
        {
            InitializeComponent();
        }

        private Vector3D GetTargetAxis()
        {
            if (VxChecked)
                return new Vector3D(1, 0, 0);
            if (VyChecked)
                return new Vector3D(0, 1, 0);
            if (VzChecked)
                return new Vector3D(0, 0, 1);

            throw new InvalidOperationException("No axis selected");
        }

        private void OnProgressCompleted(object sender, EventArgs e)
        {
            tb_calibmsg.Visibility = Visibility.Hidden;
            bu_calibrate.IsEnabled = true;

            SensorBoneLink.AddCalibrationAxisFromGyro(startTime, GetTargetAxis());
        }

        private void OnCalibrateButtonClick(object sender, RoutedEventArgs e)
        {
            startTime = DateTime.Now;
            var animation = (Storyboard)FindResource("anim_calibration");
            bu_calibrate.IsEnabled = false;

            tb_calibmsg.Visibility = Visibility.Visible;
            animation.Begin();
        }

        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            SensorBoneLink.ClearCalibration();
        }

        private void OnCalculateTransformationClick(object sender, RoutedEventArgs e)
        {
            SensorBoneLink.CalculateCalibrationTransform();
        }
    }
}
