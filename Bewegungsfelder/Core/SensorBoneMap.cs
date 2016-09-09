/*
Part of Bewegungsfelder 

MIT-License 
(C) 2016 Ivo Herzig

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bewegungsfelder.Core
{
    public class SensorBoneMap
    {
        private Dictionary<Bone, SensorBoneLink> links = new Dictionary<Bone, SensorBoneLink>();

        public IEnumerable<SensorBoneLink> Links { get { return links.Values; } }

        public event Action<SensorBoneLink> LinkAdded;

        public event Action<SensorBoneLink> LinkRemoved;

        public SensorBoneLink CreateLink(Bone bone, Sensor sensor)
        {
            if (links.ContainsKey(bone))
            {
                var existingLink = links[bone];
                if (existingLink.Sensor == sensor)
                { // link already exists
                    return null;
                }
                else
                { // remove existing link
                    links.Remove(bone);
                    LinkRemoved?.Invoke(existingLink);
                }
            }

            // create new link
            var link = new SensorBoneLink(bone, sensor);
            links.Add(bone, link);
            LinkAdded?.Invoke(link);

            return link;
        }

        public void RemoveLink(Bone bone)
        {
            if (links.ContainsKey(bone))
            {
                var link = links[bone];
                links.Remove(bone);
                LinkRemoved?.Invoke(link);
            }
        }

        public Dictionary<Bone, Quaternion> GetCalibratedSensorOrientations()
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
            links.Clear();
        }
    }

}
