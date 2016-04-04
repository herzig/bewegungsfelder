using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mocap.Utilities
{
    public static class EnumerableEx
    {
        /// <summary>
        /// Concatenate a number IEnumerable items into a single IEnumerable 
        /// </summary>
        /// <typeparam name="T">item type</typeparam>
        /// <param name="items">the IEnumerabel objects to concatenate</param>
        /// <returns>a comination of all items</returns>
        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] items)
        {
            IEnumerable<T> result = items.First();

            foreach (var item in items.Skip(1))
                result = result.Concat(item);

            return result;
        }
    }
}
