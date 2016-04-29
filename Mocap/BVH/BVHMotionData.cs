using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.BVH
{
    public class BVHMotionData
    {
        public double FrameTime { get; }
        public Dictionary<BVHNode, List<Quaternion>> Data { get; }

        public BVHMotionData(double frameTime, Dictionary<BVHNode, List<Quaternion>> motionData)
        {
            this.FrameTime = frameTime;
            this.Data = motionData;
        }
    }
}
