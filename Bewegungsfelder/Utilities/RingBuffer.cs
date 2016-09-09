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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Bewegungsfelder.Utilities
{
    class RingBuffer<T>
    {
        private object padlock = new object();

        private T[] data;

        public int Count = 0;

        private int index = -1;

        public readonly int Capacity;

        public T Last { get { lock (padlock) { return data[index]; } } }

        public RingBuffer(int capacity)
        {
            this.Capacity = capacity;
            data = new T[capacity];
        }

        public void Push(T value)
        {
            lock (padlock)
            {
                index = (index + 1) % Capacity;
                data[index] = value;
            }

            if (Count < Capacity)
                ++Count;
        }

        public T[] Take()
        {
            return Take(Capacity);
        }

        public T[] Take(int count)
        {
            if (count < 1)
                throw new InvalidOperationException("Cant take less than one");

            T[] result = new T[count];

            lock (padlock)
            {
                int startIndex = index + 1 - count;
                if (startIndex < 0)
                {
                    Array.Copy(data, mod(startIndex, Capacity), result, 0, Math.Abs(startIndex));
                    Array.Copy(data, 0, result, Math.Abs(startIndex), index + 1);
                }
                else
                {
                    Array.Copy(data, startIndex, result, 0, count);
                }

                return result;
            }
        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}