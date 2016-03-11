using HelixToolkit.Wpf;
using Mocap.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.VM
{
    public class BoneVM : INotifyPropertyChanged
    {
        // visuals to connect to child bones
        private Dictionary<BoneVM, LinesVisual3D> childLinkVisual = new Dictionary<BoneVM, LinesVisual3D>();

        public Bone Model { get; set; }

        /// <summary>
        /// this instances parent node. null means that this is the root node
        /// </summary>
        public BoneVM Parent { get; }

        /// <summary>
        /// the name of this bone
        /// </summary>
        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (Model.Name != value)
                {
                    Model.Name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        /// <summary>
        /// the offset to the parent node.
        /// </summary>
        public Vector3D Offset
        {
            get { return Model.Offset; }
            set
            {
                Model.Offset = value;
                Parent?.UpdateLinkVisual(this);
            }
        }

        /// <summary>
        /// the local orientation of this bone
        /// </summary>
        public Quaternion LocalRotation { get { return Model.JointRotation; } set { Model.JointRotation = value; } }

        public Quaternion BaseSensorRotation { get; set; }

        /// <summary>
        /// this bones associated sensor
        /// </summary>
        public SensorVM Sensor { get; set; }

        public Quaternion SensorToLocalTransform { get; set; }

        public ModelVisual3D Visual { get; }
        public ModelVisual3D WorldVisual { get; }

        public ObservableCollection<BoneVM> Children { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BoneVM(Bone model, BoneVM parent)
        {
            Model = model;
            Parent = parent;

            Visual = new ModelVisual3D();
            Visual.Children.Add(new CoordinateSystemVisual3D());

            WorldVisual = new ModelVisual3D();

            // create child bones
            Children = new ObservableCollection<BoneVM>();
            Children.CollectionChanged += OnChildrenChanged;
            foreach (var item in model.Children)
            {
                Children.Add(new BoneVM(item, this));
            }
        }

        private void OnChildrenChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (BoneVM child in e.NewItems)
                {
                    Visual.Children.Add(child.Visual);
                    LinesVisual3D linkVisual = new LinesVisual3D
                    {
                        Thickness = 3,
                        Points = new Point3DCollection(new[] { new Point3D(0, 0, 0), child.Offset.ToPoint3D() })
                    };
                    Visual.Children.Add(linkVisual);
                    childLinkVisual.Add(child, linkVisual);

                    WorldVisual.Children.Add(child.WorldVisual);
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (BoneVM child in e.NewItems)
                {
                    Visual.Children.Remove(child.Visual);
                    Visual.Children.Remove(childLinkVisual[child]);
                    WorldVisual.Children.Remove(child.WorldVisual);
                }
            }
        }

        private void UpdateLinkVisual(BoneVM child)
        {
            childLinkVisual[child].Points[1] = child.Offset.ToPoint3D();
        }


        public void Refresh()
        {
            Visual.Transform = new MatrixTransform3D(Model.LocalTransform);
            foreach (var item in Children)
            {
                item.Refresh();
            }
        }

        public void Traverse(Action<BoneVM> action)
        {
            action(this);

            foreach (var item in Children)
                item.Traverse(action);
        }
    }
}
