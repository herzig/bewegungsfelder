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
    public class Bone
    {
        private Quaternion jointRotation = Quaternion.Identity;
        private Vector3D offset = new Vector3D();

        private string name;

        /// <summary>
        /// combined offset and joint rotation matrix
        /// </summary>
        public Matrix3D LocalTransform { get; private set; }

        /// <summary>
        /// local joint rotation
        /// </summary>
        public Quaternion JointRotation
        {
            get { return jointRotation; }
            set
            {
                jointRotation = value;
                UpdateLocalTransform();
            }
        }

        /// <summary>
        /// offset to the parent joint
        /// </summary>
        public Vector3D Offset
        {
            get { return offset; }
            set
            {
                offset = value;
                UpdateLocalTransform();
            }
        }

        /// <summary>
        /// a name identifiying this bone
        /// </summary>
        public string Name
        {
            get
            {
                if (IsEndSite)
                    return "End Site";
                else
                    return name;
            }
            set { name = value; }
        }

        /// <summary>
        /// this node is at the end of the kinematic chain. there are no children.
        /// </summary>
        public bool IsEndSite { get { return Children.Count == 0; } }

        /// <summary>
        /// the parent joint. if null this is a root bone
        /// </summary>
        public Bone Parent { get; private set; }

        /// <summary>
        /// all child bones
        /// </summary>
        public List<Bone> Children { get; } = new List<Bone>();

        /// <summary>
        /// initializes a new instance.
        /// </summary>
        /// <param name="parent">the paren bone if any</param>
        /// <param name="name">a name for this bone</param>
        /// <param name="offset">the offset to the parent bone</param>
        public Bone(Bone parent, string name = "bone", Vector3D offset = default(Vector3D))
        {
            Parent = parent;
            Name = name;
            Offset = offset;
        }

        /// <summary>
        /// update the LocalTransformation matrix by combining the current offset and rotation values
        /// </summary>
        private void UpdateLocalTransform()
        {
            var mat = Matrix3D.Identity;
            mat.Rotate(JointRotation);
            mat.Translate(Offset);

            LocalTransform = mat;
        }

        /// <summary>
        ///  builds the combined rotation from the root bone to this bone
        /// </summary>
        /// <returns>a quaternion representing the root-to-bone transformation for this bone</returns>
        public Quaternion GetRootOrientation()
        {
            Quaternion quat = Quaternion.Identity;
            var current = this;
            do
            {
                quat *= current.JointRotation;
                current = current.Parent;
            } while (current != null);

            return quat;
        }

        /// <summary>
        /// builds & return the transformation from the root bone to this bone.
        /// </summary>
        public Matrix3D GetRootTransform()
        {
            Matrix3D transform = Matrix3D.Identity;
            var current = this;
            do
            {
                transform.Append(current.LocalTransform);
                current = current.Parent;
            } while (current != null);

            return transform;
        }

        /// <summary>
        /// creates a clone of this bone and all its children
        /// </summary>
        /// <param name="parentClone">the cloned parent bone</param>
        public Bone Clone(Bone parentClone)
        {
            Bone clone = new Bone(parentClone, Name, Offset);
            clone.JointRotation = JointRotation;

            foreach (var child in Children)
                clone.Children.Add(child.Clone(this));

            return clone;
        }

        /// <summary>
        /// traverses the tree and applies an action to all bones. 
        /// Also builds the world rotation as the tree is traversed.
        /// </summary>
        /// <param name="action">the action to execute on each bone</param>
        /// <param name="worldTransformation">world transformation for the bone</param>
        public void Traverse(Action<Bone, Quaternion> action, Quaternion worldRotation)
        {
            action(this, worldRotation);
            worldRotation *= jointRotation;

            foreach (var child in Children)
            {
                child.Traverse(action, worldRotation);
            }
        }

        /// <summary>
        /// taverses the tree and applies an action on all bones
        /// </summary>
        public void Traverse(Action<Bone> action)
        {
            action(this);

            foreach (var child in Children)
            {
                child.Traverse(action);
            }
        }
    }
}