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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.Core
{
    public class KinematicStructure
    {
        public Bone Root { get; }

        public KinematicStructure(Bone root = null)
        {
            if (root == null)
                Root = new Bone(parent: null) { Name = "Root" };
            else
                Root = root;
        }

        public void ApplyWorldRotations(Dictionary<Bone, Quaternion> jointRotations)
        {
            Root.Traverse((bone, worldRotation) =>
            {
                if (jointRotations.ContainsKey(bone))
                {
                    bone.JointRotation = worldRotation.Inverted() * jointRotations[bone];
                }
            }, Quaternion.Identity);
        }

        public void ApplyLocalRotation(Dictionary<Bone, Quaternion> jointRotations)
        {
            Root.Traverse((bone) =>
            {
                if (jointRotations.ContainsKey(bone))
                {
                    bone.JointRotation = jointRotations[bone];
                }
            });
        }

        /// <summary>
        /// collects the local orientation values for all bones in the tree
        /// </summary>
        public Dictionary<Bone, Quaternion> CollectLocalOrientations()
        {
            var result = new Dictionary<Bone, Quaternion>();
            Root.Traverse(bone => result.Add(bone, bone.JointRotation));
            return result;
        }
    }
}
