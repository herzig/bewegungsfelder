using Mocap.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.BVH
{
    public class BVHConverter
    {
        /// <summary>
        /// converts BVH hierarchical structure to the applications own kinematic model definition
        /// </summary>
        public static Bone ToBones(BVHNode bvhNode, Bone parent, Dictionary<BVHNode, List<Quaternion>> bvhMotionData,
            Dictionary<Bone, List<Quaternion>> resultMotionData)
        {
            Bone result = new Bone(parent, name: bvhNode.Name, offset: bvhNode.Offset);
            if (bvhNode.Type != BVHNodeTypes.EndSite)
                resultMotionData.Add(result, bvhMotionData[bvhNode].ToList());

            foreach (BVHNode item in bvhNode.Children)
            {
                result.Children.Add(ToBones(item, result, bvhMotionData, resultMotionData));
            }

            return result;
        }

    }
}
