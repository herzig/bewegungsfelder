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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bewegungsfelder.View
{
    /// <summary>
    /// Interaction logic for SensorDetailsWindow.xaml
    /// </summary>
    public partial class SensorDetailsWindow : Window
    {


        public SensorVM Sensor
        {
            get { return (SensorVM)GetValue(SensorProperty); }
            set { SetValue(SensorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sensor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SensorProperty =
            DependencyProperty.Register(nameof(Sensor), typeof(SensorVM), typeof(SensorDetailsWindow), new FrameworkPropertyMetadata(OnSensorPropertyChanged));


        public SensorDetailsWindow()
        {
            InitializeComponent();
            Closing += SensorDetailsWindow_Closing;
        }

        private void SensorDetailsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private static void OnSensorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var inst = (SensorDetailsWindow)obj;

            inst.view.Sensor = e.NewValue as SensorVM;
        }
    }
}
