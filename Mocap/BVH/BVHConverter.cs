using Mocap.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mocap.BVH
{
    public class BVHConverter
    {
        /// <summary>
        /// converts BVH hierarchical structure to the applications own kinematic model definition
        /// </summary>
        public static Bone ToBones(BVHNode root, Bone parent)
        {
            Bone result = new Bone(parent, name: root.Name, offset: root.Offset);

            foreach (BVHNode item in root.Children)
            {
                result.Children.Add(ToBones(item, result));
            }

            return result;
        }
    }
}
