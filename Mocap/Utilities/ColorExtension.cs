using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using HelixToolkit.Wpf;

namespace Mocap.Utilities
{
    public static class ColorExtension
    {
        public static Color ChangeSaturation(this Color color, double saturation)
        {
            //hue, saturation, value
            double[] hsv = ColorHelper.ColorToHsv(color);
            hsv[1] = saturation;
            return ColorHelper.HsvToColor(hsv);
        }

        public static Color ChangeSaturationValue(this Color color, double saturation, double value)
        {
            //hue, saturation, value
            double[] hsv = ColorHelper.ColorToHsv(color);
            hsv[1] = saturation;
            hsv[2] = value;
            return ColorHelper.HsvToColor(hsv);
        }
    }
}
