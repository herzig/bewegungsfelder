using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mocap.BVH
{
    // all possible bvh channels ( mod 3 to get 0,1,2 (xyz) indices)
    public enum BVHChannels
    {
        Xposition = 0,
        Yposition = 1,
        Zposition = 2,

        Xrotation = 3,
        Yrotation = 4,
        Zrotation = 5,
    }

    public enum BVHNodeTypes
    {
        Root,
        Joint,
        EndSite,
    }
}
