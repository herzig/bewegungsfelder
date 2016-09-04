using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mocap.Utilities
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