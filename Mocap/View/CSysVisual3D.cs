/*
Part of Bewegungsfelder 
(C) 2016 Ivo Herzig

[[LICENSE]]
*/
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Mocap.View
{
    class CSysVisual3D : ModelVisual3D
    {

        private LinesVisual3D XVisual { get; }
        private LinesVisual3D YVisual { get; }
        private LinesVisual3D ZVisual { get; }

        public Color XColor { get { return XVisual.Color; } set { XVisual.Color = value; } }

        public Color YColor { get { return YVisual.Color; } set { YVisual.Color = value; } }

        public Color ZColor { get { return ZVisual.Color; } set { ZVisual.Color = value; } }

        public double Thickness
        {
            get { return XVisual.Thickness; }
            set
            {
                XVisual.Thickness = value;
                YVisual.Thickness = value;
                ZVisual.Thickness = value;
            }
        }

        public double Length
        {
            get { return XVisual.Points[1].X; }
            set
            {
                XVisual.Points[1] = new Point3D(value, 0, 0);
                YVisual.Points[1] = new Point3D(0, value, 0);
                ZVisual.Points[1] = new Point3D(0, 0, value);
            }
        }


        public CSysVisual3D()
        {
            // set a random depth offset to avoid flickering (depth fighting)
            var rand = new Random();
            XVisual = new LinesVisual3D();
            XVisual.DepthOffset = 0.0001 * rand.NextDouble();
            XVisual.Points.Add(new Point3D());
            XVisual.Points.Add(new Point3D(1, 0, 0));
            Children.Add(XVisual);

            YVisual = new LinesVisual3D();
            YVisual.DepthOffset = 0.0001 * rand.NextDouble();
            YVisual.Points.Add(new Point3D());
            YVisual.Points.Add(new Point3D(0, 1, 0));
            Children.Add(YVisual);

            ZVisual = new LinesVisual3D();
            ZVisual.DepthOffset = 0.0001 * rand.NextDouble();
            ZVisual.Points.Add(new Point3D());
            ZVisual.Points.Add(new Point3D(0, 0, 1));
            Children.Add(ZVisual);

            Thickness = 4;
            XColor = Colors.Red;
            YColor = Colors.Green;
            ZColor = Colors.Blue;
            Length = 1;
        }
    }
}
