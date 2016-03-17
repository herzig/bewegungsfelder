using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.BVH
{
    public class BVHNode
    {
        public BVHNodeTypes Type { get; set; }

        public string Name { get; set; }

        public Vector3D Offset { get; set; }

        public BVHChannels[] Channels { get; set; }

        public List<BVHNode> Children { get; } = new List<BVHNode>();
    }
}
