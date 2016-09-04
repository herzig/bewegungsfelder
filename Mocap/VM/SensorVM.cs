/*
Part of Bewegungsfelder 
(C) 2016 Ivo Herzig

[[LICENSE]]
*/

using Mocap.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.VM
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
