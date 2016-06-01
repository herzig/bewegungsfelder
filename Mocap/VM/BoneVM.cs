/*
Part of Bewegungsfelder 
(C) 2016 Ivo Herzig

[[LICENSE]]
*/

using HelixToolkit.Wpf;
using Mocap.Core;
using Mocap.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections;
using Mocap.Utilities;
using System.Collections.Specialized;

namespace Mocap.VM
{
    public class BoneVM : INotifyPropertyChanged, IEnumerable<BoneVM>
    {
        public static double LinkThickness = 3;
        public static double SelectedLinkThickness = 5;

        public static Color LinkColor = Colors.DarkGray;
        public static Color SelectedLinkColor = Colors.Black;

        private bool isSelected;

        private SensorBoneLinkVM sensorBoneLink;

        private CSysVisual3D coordinateSystemVisual;

        // visuals to connect to child bones
        private Dictionary<BoneVM, LinesVisual3D> childLinkVisualMap = new Dictionary<BoneVM, LinesVisual3D>();

        /// <summary>
        /// the underlying Bone model instance
        /// </summary>
        public Bone Model { get; set; }

        /// <summary>
        /// this instances parent node. null means that this is the root node
        /// </summary>
        public BoneVM Parent { get; }

        /// <summary>
        /// true if the current bone is selected in UI
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;

                    UpdateVisuals();

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

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

        /// <summary>
        /// this bones associated sensor.
        /// read from SensorBoneLink property
        /// </summary>
        public SensorVM Sensor { get { return SensorBoneLink?.Sensor; } }

        /// <summary>
        /// this bones sensor link. Is null if the bone is not linked to any sensor
        /// </summary>
        public SensorBoneLinkVM SensorBoneLink
        {
            get { return sensorBoneLink; }
            set
            {
                if (sensorBoneLink != value)
                {
                    sensorBoneLink = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SensorBoneLink)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLinkedToSensor)));
                }
            }
        }

        /// <summary>
        /// indicates if there is a sensor associated with this bone.
        /// </summary>
        public bool IsLinkedToSensor { get { return SensorBoneLink != null; } }

        public ModelVisual3D Visual { get; }

        public ObservableCollection<BoneVM> Children { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BoneVM(Bone model, BoneVM parent)
        {
            Model = model;
            Parent = parent;

            DisplaySettings.Get.PropertyChanged += OnDisplaySettingsPropertyChanged;

            Visual = new ModelVisual3D();
            coordinateSystemVisual = new CSysVisual3D();
            coordinateSystemVisual.Length = DisplaySettings.Get.CSysSize;

            Visual.Children.Add(coordinateSystemVisual);

            // create child bones
            Children = new ObservableCollection<BoneVM>();
            Children.CollectionChanged += OnChildrenChanged;
            foreach (var item in model.Children)
            {
                Children.Add(new BoneVM(item, this));
            }
        }

        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (BoneVM child in e.NewItems)
                {
                    Visual.Children.Add(child.Visual);
                    LinesVisual3D linkVisual = new LinesVisual3D();
                    linkVisual.Points = new Point3DCollection(new[] { new Point3D(0, 0, 0), child.Offset.ToPoint3D() });
                    childLinkVisualMap.Add(child, linkVisual);

                    Visual.Children.Add(linkVisual);
                    UpdateLinkVisual(child);

                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (BoneVM child in e.OldItems)
                {
                    Model.Children.Remove(child.Model);

                    Visual.Children.Remove(child.Visual);
                    Visual.Children.Remove(childLinkVisualMap[child]);
                }
            }
        }

        private void OnDisplaySettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateVisuals();
        }

        private void UpdateLinkVisual(BoneVM child)
        {
            var lineVisual = childLinkVisualMap[child];

            lineVisual.Points[1] = child.Offset.ToPoint3D();

            if (IsSelected)
            {
                lineVisual.Thickness = SelectedLinkThickness;
                lineVisual.Color = SelectedLinkColor;
            }
            else
            {
                lineVisual.Thickness = LinkThickness;
                lineVisual.Color = LinkColor;
            }
        }

        private void UpdateVisuals()
        {
            coordinateSystemVisual.Length = DisplaySettings.Get.CSysSize;
            foreach (var item in childLinkVisualMap)
            {
                UpdateLinkVisual(item.Key);
            }
        }

        public void Refresh()
        {
            Visual.Transform = new MatrixTransform3D(Model.LocalTransform);
            foreach (var item in Children)
            {
                item.Refresh();
            }
        }

        /// <summary>
        /// traverses the tree and flattens all nodes into a list
        /// </summary>
        /// <param name="items">a list that will be populated with a flat list of all items in the subtree</param>
        private void Collect(List<BoneVM> items)
        {
            items.Add(this);
            foreach (var item in Children)
            {
                item.Collect(items);
            }
        }

        /// <summary>
        /// enumerates all tree items starting with the current node
        /// </summary>
        /// <returns>an enumerating for this subtree</returns>
        public IEnumerator<BoneVM> GetEnumerator()
        {
            List<BoneVM> items = new List<BoneVM>();
            Collect(items);
            return EnumerableEx.Concat(items).GetEnumerator();
        }

        /// <summary>
        /// enumerates all tree items starting with the current node
        /// </summary>
        /// <returns>an enumerating for this subtree</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
