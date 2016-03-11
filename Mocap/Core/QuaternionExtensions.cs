using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public static class QuaternionExtensions
    {
        public static Quaternion Inverted(this Quaternion quat)
        {
            var res = quat;
            quat.Invert();
            return quat;
        }
    }

    public static class Matrix3DExtensions
    {
        public static Quaternion GetRotation(this Matrix3D mat)
        {
            double trace = mat.M11 + mat.M22 + mat.M33;

            Quaternion result = new Quaternion();
            if (trace > 0)
            {
                double s = Math.Sqrt(trace + 1.0) * 2;
                result.W = 0.25 * s;
                result.X = (mat.M32 - mat.M23) / s;
                result.Y = (mat.M13 - mat.M31) / s;
                result.Z = (mat.M21 - mat.M12) / s;
            }
            else if ((mat.M11 > mat.M22) & (mat.M11 > mat.M33))
            {
                double s = Math.Sqrt(1.0 + mat.M11 - mat.M22 - mat.M33) * 2;
                result.W = (mat.M32 - mat.M23) / s;
                result.X = 0.25 * s;
                result.Y = (mat.M12 + mat.M21) / s;
                result.Z = (mat.M13 + mat.M31) / s;
            }
            else if (mat.M22 > mat.M33)
            {
                double s = Math.Sqrt(1.0 + mat.M22 - mat.M11 - mat.M33) * 2;
                result.W = (mat.M13 - mat.M31) / s;
                result.X = (mat.M12 + mat.M21) / s;
                result.Y = 0.25 * s;
                result.Z = (mat.M23 + mat.M32) / s;
            }
            else
            {
                double s = Math.Sqrt(1.0 + mat.M33 - mat.M11 - mat.M22) * 2;
                result.W = (mat.M21 - mat.M12) / s;
                result.X = (mat.M13 + mat.M31) / s;
                result.Y = (mat.M23 + mat.M32) / s;
                result.Z = 0.25 * s;
            }
            return result;
        }

    }
}
