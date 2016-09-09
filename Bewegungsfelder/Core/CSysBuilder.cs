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
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.Core
{
    public class CSysBuilder : INotifyPropertyChanged
    {
        public readonly double AngleTolerance = 360;

        private Vector3D[] vectors = new Vector3D[] { new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1) };
        private bool[] userDefined = new bool[] { true, true, false };

        public Vector3D Row1
        {
            get { return vectors[0]; }
            set
            {
                vectors[0] = value.Normalized();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row1)));
            }
        }
        public Vector3D Row2
        {
            get { return vectors[1]; }
            set
            {
                vectors[1] = value.Normalized();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row2)));
            }
        }
        public Vector3D Row3
        {
            get { return vectors[2]; }
            set
            {
                vectors[2] = value.Normalized();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row3)));
            }
        }

        public bool Row1UserDefined
        {
            get { return userDefined[0]; }
            set
            {
                if (value)
                {
                    if (userDefined.Count(i => i) < 2)
                    {
                        userDefined[0] = value;
                    }
                }
                else
                {
                    userDefined[0] = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row1UserDefined)));
            }
        }
        public bool Row2UserDefined
        {
            get { return userDefined[1]; }
            set
            {
                if (value)
                {
                    if (userDefined.Count(i => i) < 2)
                        userDefined[1] = value;
                }
                else
                {
                    userDefined[1] = value;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row2UserDefined)));
            }
        }
        public bool Row3UserDefined
        {
            get { return userDefined[2]; }
            set
            {
                if (value)
                {
                    if (userDefined.Count(i => i) < 2)
                        userDefined[2] = value;
                }
                else
                {
                    userDefined[2] = value;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row3UserDefined)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CSysBuilder() { }

        public void CalculateVectors()
        {
            int[] userDefIndices = Enumerable.Range(0, 3).Where(i => userDefined[i]).ToArray();

            if (userDefIndices.Length > 2)
                throw new InvalidOperationException("Can't define all three vectors");
            else if (userDefIndices.Length < 2)
                throw new InvalidOperationException("Need at least two vectors defined");

            int calcIndex = 3 - (userDefIndices[0] + userDefIndices[1]);

            // check if vectors are orthogonal.
            Vector3D a = vectors[userDefIndices[0]];
            Vector3D b = vectors[userDefIndices[1]];

            var angle = Vector3D.AngleBetween(a, b);
            var err = Math.Abs(angle - 90);
            if (err < AngleTolerance)
            {
                // automatically correct second vector if within tolerance
                var tmp = Vector3D.CrossProduct(a, b);
                b = Vector3D.CrossProduct(a, tmp).Normalized();
                vectors[userDefIndices[1]] = b;

                // calculate third vector
                vectors[calcIndex] = Vector3D.CrossProduct(a, b).Normalized();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row1)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row2)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Row3)));
            }
            else
            {
                throw new InvalidOperationException("Vectors have to be orthogonal");
            }
        }

        public Matrix3D GetMatrix()
        {
            return new Matrix3D(
                Row1.X, Row1.Y, Row1.Z, 0,
                Row2.X, Row2.Y, Row2.Z, 0,
                Row3.X, Row3.Y, Row3.Z, 0,
                0, 0, 0, 1 );
        }
    }
}
