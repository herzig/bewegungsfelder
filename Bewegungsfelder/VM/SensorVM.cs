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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.VM
{
    public class SensorVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The underlying sensor model
        /// </summary>
        public Sensor Model { get; }

        /// <summary>
        /// a calculated sample rate for this sensor.
        /// this is recalculated on Refresh();
        /// </summary>
        public double SampleRate { get; set; }

        /// <summary>
        /// returns the orientation from the last received value 
        /// </summary>
        public Quaternion CurrentOrientation { get { return LastValue.Orientation; } }

        /// <summary>
        /// the last sensor value received.
        /// returns a default SensorValue if no data recorded yet
        /// </summary>
        public SensorValue LastValue { get { return Model.LastValue; } }

        /// <summary>
        /// Raised when any property of this instances changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a Sensor unit view model
        /// </summary>
        /// <param name="model">the model to use for this instance</param>
        public SensorVM(Sensor model)
        {
            this.Model = model;
        }

        /// <summary>
        /// raise the property changed event to cause view updates
        /// </summary>
        public void Refresh()
        {
            //SampleRate = Model.GetSampleRate(5);

            // empty property changed event is interpreted as a change on all properties
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

    }
}
