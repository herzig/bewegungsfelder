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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.BVH
{
    public class BVHConverter
    {
        /// <summary>
        /// converts BVH hierarchical structure to the applications own kinematic model definition
        /// </summary>
        public static Bone ToBones(BVHNode bvhNode, Bone parent, BVHMotionData bvhMotionData,
            MotionData resultMotionData)
        {
            Bone result = new Bone(parent, name: bvhNode.Name, offset: bvhNode.Offset);
            if (bvhNode.Type != BVHNodeTypes.EndSite)
                resultMotionData.Data.Add(result, bvhMotionData.Data[bvhNode].ToList());

            foreach (BVHNode item in bvhNode.Children)
            {
                result.Children.Add(ToBones(item, result, bvhMotionData, resultMotionData));
            }

            return result;
        }

        public static BVHNode ToBVHData(Bone node, MotionData motionData, out BVHMotionData bvhMotionData)
        {
            Dictionary<BVHNode, List<Quaternion>> data = new Dictionary<BVHNode, List<Quaternion>>();
            var resultNode = ToBVHNode(node, motionData.Data, null, data);
            bvhMotionData = new BVHMotionData(1.0 / motionData.FPS, data);

            return resultNode;
        }

        /// <summary>
        /// converts a kinematic model to BVH structure for export.
        /// </summary>
        private static BVHNode ToBVHNode(Bone bone, Dictionary<Bone, List<Quaternion>> sourceMotionData, BVHNode parent, Dictionary<BVHNode, List<Quaternion>> motionData)
        {
            var result = new BVHNode();
            result.Name = bone.Name;
            result.Offset = bone.Offset;

            // determine type
            if (parent == null)
            {
                result.Type = BVHNodeTypes.Root;
                result.Channels = new[] { BVHChannels.Zrotation, BVHChannels.Yrotation, BVHChannels.Xrotation };
            }
            else if (bone.Children.Count == 0)
            {
                result.Type = BVHNodeTypes.EndSite;
            }
            else
            {
                result.Type = BVHNodeTypes.Joint;
                result.Channels = new[] { BVHChannels.Zrotation, BVHChannels.Yrotation, BVHChannels.Xrotation };
            }

            // populate motion data
            if (result.Type != BVHNodeTypes.EndSite && sourceMotionData.Count > 0
                && sourceMotionData.ContainsKey(bone))
                motionData.Add(result, sourceMotionData[bone]);

            // add child nodes
            foreach (var child in bone.Children)
            {
                result.Children.Add(ToBVHNode(child, sourceMotionData, result, motionData));
            }

            return result;
        }
    }
}
