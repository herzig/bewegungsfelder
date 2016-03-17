using Mocap.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

namespace Mocap.VM
{
    public class KinematicVM
    {
        private BoneVM selectedItem;

        /// <summary>
        /// the underlying model for this view model
        /// </summary>
        public Kinematic Model { get; }

        /// <summary>
        /// roots collection is always just a single entry, mainly used to be able to bind to collection views
        /// </summary>
        public BoneVM[] Roots { get; } = new BoneVM[1];

        /// <summary>
        /// the root node for this kinematic chain 
        /// </summary>
        public BoneVM Root { get { return Roots[0]; } }

        /// <summary>
        /// the currently selected bone
        /// </summary>
        public BoneVM SelectedItem
        {
            get { return selectedItem; }
            private set
            {
                if (selectedItem != value)
                {
                    if (selectedItem != null)
                    {
                        selectedItem.IsSelected = false;
                    }

                    selectedItem = value;

                    if (selectedItem != null)
                    {
                        selectedItem.IsSelected = true;
                    }

                    // HACK: this updates all commands. maybe we could be more selective
                    CommandManager.InvalidateRequerySuggested();
                    SetDetailItemRequested?.Invoke(selectedItem);
                }
            }
        }

        /// <summary>
        /// adds a new child to the currently selected node
        /// </summary>
        public ICommand AddBoneCommand { get; }

        /// <summary>
        /// remove the currently selected bone and all its children.
        /// </summary>
        public ICommand RemoveBoneCommand { get; }

        /// <summary>
        /// sets the currently selected item
        /// </summary>
        public ICommand ChangeSelectedCommand { get; }

        /// <summary>
        /// occurs when this instance tries to set the app-wide details item
        /// </summary>
        public event Action<object> SetDetailItemRequested;

        /// <summary>
        /// creates new kinematic instance
        /// </summary>
        /// <param name="model">the underlying model</param>
        /// <param name="sensors">collection of registered sensors.</param>
        public KinematicVM(Kinematic model)
        {
            Model = model;

            // create the bone ViewModel tree
            Roots[0] = new BoneVM(model.Root, null);

            // setup commands
            AddBoneCommand = new RelayCommand(AddBone, CanAddBone);
            RemoveBoneCommand = new RelayCommand(RemoveBone, CanRemoveBone);
            ChangeSelectedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>(ChangeSelected);
        }

        /// <summary>
        /// sets the SelectedItem property called by the <see cref="ChangeSelectedCommand"/>
        /// </summary>
        private void ChangeSelected(RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = (BoneVM)e.NewValue;
        }

        /// <summary>
        /// creates and adds a new bone to the currently selected item
        /// </summary>
        private void AddBone()
        {
            if (SelectedItem == null)
                throw new InvalidOperationException("No bone selected!");

            var model = new Bone(parent: SelectedItem.Model, offset: new Vector3D(1,0,0));
            var vm = new BoneVM(model, parent: SelectedItem);
            SelectedItem.Children.Add(vm);
            SelectedItem.Model.Children.Add(model);
        }

        /// <summary>
        ///  can only add bones if the SelectedItem is set
        /// </summary>
        private bool CanAddBone()
        {
            return SelectedItem != null;
        }

        /// <summary>
        /// remove the currently selected bone
        /// </summary>
        private void RemoveBone()
        {
            if (SelectedItem == null)
                throw new InvalidOperationException("No bone selected!");
            if (SelectedItem.Parent == null)
                throw new InvalidOperationException("Can't remove root node");

            SelectedItem.Parent.Children.Remove(SelectedItem);
        }

        /// <summary>
        /// checks if the RemoveBone command can be executed
        /// </summary>
        private bool CanRemoveBone()
        {
            return SelectedItem != null && SelectedItem.Parent != null;
        }

        /// <summary>
        /// Notifies the ui of changes in all child items
        /// </summary>
        public void Refresh()
        {
            foreach (var item in Roots)
                item.Refresh();
        }
    }
}
