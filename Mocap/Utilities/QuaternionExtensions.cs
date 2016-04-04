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

    public static class MatrixExtensions
    {
        public static  Vector3D GetOffset(this Matrix3D matrix)
        {
            return new Vector3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);
        }
    }
}
