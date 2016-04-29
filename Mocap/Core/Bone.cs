using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public class Bone
    {
        private Quaternion jointRotation = Quaternion.Identity;
        private Vector3D offset = new Vector3D();

        private Quaternion baseRotation = Quaternion.Identity;
        private Vector3D baseOffset = new Vector3D();

        /// <summary>
        /// combined offset and joint rotation in the base position
        /// </summary>>
        public Matrix3D BaseLocalTransform { get; private set; }

        /// <summary>
        /// local joint rotation in the base position
        /// </summary>
        public Quaternion BaseJointRotation
        {
            get { return baseRotation; }
            set
            {
                baseRotation = value;
                UpdateLocalTransform();
            }
        }

        /// <summary>
        /// offset to the parent joint in the base position
        /// </summary>
        public Vector3D BaseOffset
        {
            get { return baseOffset; }
            set
            {
                baseOffset = value;
                UpdateLocalTransform();
            }
        }

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
        public string Name { get; set; }

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
        /// update the LocalTransformation matrix by combining the current offset and rotation values
        /// </summary>
        private void UpdateBaseTransform()
        {
            var mat = Matrix3D.Identity;
            mat.Rotate(BaseJointRotation);
            mat.Translate(BaseOffset);

            BaseLocalTransform = mat;
        }

        /// <summary>
        ///  builds the combined rotation from the root bone to this bone
        /// </summary>
        /// <returns>a quaternion representing the root-to-bone transformation for this bone</returns>
        public Quaternion GetRootRotation()
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