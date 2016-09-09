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
using Bewegungsfelder;

namespace Bewegungsfelder.View
{
    /// <summary>
    /// Interaction logic for Vector3DEditor.xaml
    /// </summary>
    public partial class Vector3DEditor : UserControl
    {
        public static readonly DependencyProperty VectorProperty =
            DependencyProperty.Register(nameof(Vector), typeof(Vector3D), typeof(Vector3DEditor), 
                new FrameworkPropertyMetadata(new Vector3D(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVectorPropertyChanged));

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

        private void OnSetVxClick(object sender, RoutedEventArgs e) { SetValue(VectorProperty, new Vector3D(1, 0, 0)); }
        private void OnSetVyClick(object sender, RoutedEventArgs e) { SetValue(VectorProperty, new Vector3D(0, 1, 0)); }
        private void OnSetVzClick(object sender, RoutedEventArgs e) { SetValue(VectorProperty, new Vector3D(0, 0, 1)); }
    }
}