using Microsoft.Win32;
using Mocap.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using HelixToolkit.Wpf;
using Mocap.View;

namespace Mocap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppVM ViewModel { get { return (AppVM)DataContext; } }

        public MainWindow()
        {
            InitializeComponent();

            viewport.Children.Add(ViewModel.RootVisual3D);

            ViewModel.StartServer();
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CalibrationBoneLink))
            { // centers & zooms the viewport to show show the selected bone (and children)
                if (ViewModel.CalibrationBoneLink != null)
                {
                    var link = ViewModel.CalibrationBoneLink;
                    Matrix3D boneTransform = link.Bone.Visual.GetTransform();

                    var bounds = link.Bone.Visual.FindBounds(new MatrixTransform3D(boneTransform));
                    viewport.ZoomExtents(bounds, 500);
                }
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnLoadBVHClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog(this) == true)
            {
                ViewModel.LoadBVHFileCommand.Execute(fileDialog.FileName);
            }
        }

        private void OnSaveBVHClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            if (fileDialog.ShowDialog(this) == true)
            {
                ViewModel.SaveBVHFileCommand.Execute(fileDialog.FileName);
            }
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }
    }
}
