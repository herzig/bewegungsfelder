using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.Core
{
    public class SensorBoneMap
    {
        private Dictionary<Bone, Sensor> boneSensorsMap = new Dictionary<Bone, Sensor>();
        private Dictionary<Bone, SensorBoneLink> links = new Dictionary<Bone, SensorBoneLink>();

        public IEnumerable<SensorBoneLink> Links { get { return links.Values; } }

        public void SetLink(Bone bone, Sensor sensor)
        {
            if (boneSensorsMap.ContainsKey(bone))
            {
                boneSensorsMap.Remove(bone);
                links.Remove(bone);
            }

            links.Add(bone, new SensorBoneLink(bone, sensor));
            boneSensorsMap.Add(bone, sensor);
        }

        public void RemoveLink(Bone bone)
        {
            if (boneSensorsMap.ContainsKey(bone))
            {
                links.Remove(bone);
            }
        }

        public Dictionary<Bone, Quaternion> GetSensorOrientations()
        {
            var result = new Dictionary<Bone, Quaternion>();
            foreach (var link in links.Values)
            {
                result.Add(link.Bone, link.GetCalibratedOrientation());
            }

            return result;
        }

        /// <summary>
        /// clear all links
        /// </summary>
        public void Clear()
        {
            foreach (var item in links.Keys)
            {
                RemoveLink(item);
            }
        }
    }

}
