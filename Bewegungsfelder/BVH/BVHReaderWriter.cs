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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Bewegungsfelder.Utilities;

namespace Bewegungsfelder.BVH
{
    public class BVHReaderWriter
    {
        /// <summary>
        /// reads BVH hierarchical data from a BVH file
        /// </summary>
        public static BVHNode ReadBvh(string file, out BVHMotionData motionData)
        {
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine().ToLower().Trim();
                if (line != "hierarchy")
                    throw new FileFormatException("File has to start with HIERARCHY keyword");

                var root = ReadNode(reader, reader.ReadLine(), 0);
                motionData = ReadMotionData(reader, root);

                return root;
            }
        }

        /// <summary>
        /// read motion data from a bvh file, starting with the MOTION keyword
        /// </summary>
        public static BVHMotionData ReadMotionData(StreamReader reader, BVHNode root)
        {
            var line = reader.ReadLine().ToLower().Trim();
            if (line != "motion")
                throw new FileFormatException("Expected MOTION keyword");

            // read number of frames
            line = reader.ReadLine().ToLower().Trim();
            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            int numFrames;
            if (!int.TryParse(tokens[1], out numFrames))
            {
                throw new FileFormatException("Could not read number of frames");
            }

            //read frame time
            line = reader.ReadLine().ToLower().Trim();
            tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            double frameTime;
            if (!double.TryParse(tokens[2], out frameTime))
            {
                throw new FileFormatException("Could not read frame time");
            }

            // read all frame data
            Dictionary<BVHNode, List<Quaternion>> motionData = new Dictionary<BVHNode, List<Quaternion>>();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().ToLower().Trim();
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                double[] frameData = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(t => double.Parse(t)).ToArray();

                // interpret frame-by-frame
                int offset = 0;
                ReadFrameData(root, motionData, frameData, ref offset);
            }

            return new BVHMotionData(frameTime, motionData);
        }

        /// <summary>
        /// interprets motion data for a single frame
        /// </summary>
        private static void ReadFrameData(BVHNode node, Dictionary<BVHNode, List<Quaternion>> motionData, double[] values, ref int offset)
        {
            if (node.Type == BVHNodeTypes.EndSite)
                return;

            double[] nodevalues = new double[node.Channels.Length];
            Array.Copy(values, offset, nodevalues, 0, node.Channels.Length);
            offset += node.Channels.Length;

            if (!motionData.ContainsKey(node))
            {
                motionData.Add(node, new List<Quaternion>());
            }

            // TODO: add support for position in Bones/check how many channels really have to be skipped
            int ignoredOffset = 0;
            if (node.Channels[0] == BVHChannels.Xposition)
                ignoredOffset += 3;

            // convert rotation to quaternion
            var q1 = new Quaternion(GetAxisFromChannelType(node.Channels[ignoredOffset]), nodevalues[ignoredOffset]);
            var q2 = new Quaternion(GetAxisFromChannelType(node.Channels[ignoredOffset + 1]), nodevalues[ignoredOffset + 1]);
            var q3 = new Quaternion(GetAxisFromChannelType(node.Channels[ignoredOffset + 2]), nodevalues[ignoredOffset + 2]);

            Quaternion quat = q1 * q2 * q3;

            motionData[node].Add(quat);

            foreach (var item in node.Children)
            {
                ReadFrameData(item, motionData, values, ref offset);
            }
        }

        /// <summary>
        /// returns the corresponding axis (x,y,z) for the given channel type
        /// </summary>
        private static Vector3D GetAxisFromChannelType(BVHChannels channel)
        {
            switch (channel)
            {
                case BVHChannels.Xrotation:
                    return new Vector3D(1, 0, 0);
                case BVHChannels.Yrotation:
                    return new Vector3D(0, 1, 0);
                case BVHChannels.Zrotation:
                    return new Vector3D(0, 0, 1);
                default:
                    throw new InvalidOperationException($"Channel type {channel} not supported");
            }
        }

        /// <summary>
        /// reads a bvh node from a given bvh reader
        /// </summary>
        /// <param name="reader">stream reader standing on the opening parantheses of a node definition</param>
        /// <param name="idLine">line containing the name of the node</param>
        /// <param name="depth">recursion depth of the current node</param>
        private static BVHNode ReadNode(StreamReader reader, string idLine, int depth)
        {
            BVHNode node = new BVHNode();

            // read node type and name
            var line = idLine.ToLower().Trim();
            string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string nodeType = tokens[0];
            string nodeName = tokens[1];

            BVHNodeTypes type;
            if (nodeType == "end" && nodeName == "site")
            {
                type = BVHNodeTypes.EndSite;
            }
            else
            {
                if (!Enum.TryParse<BVHNodeTypes>(nodeType, true, out type))
                    throw new FileFormatException($"Invalid Bvh Node Type: {nodeType}");
            }

            node.Type = type;
            node.Name = nodeName;

            // read starting curly brace {
            reader.ReadLine();

            node.Offset = ReadOffset(reader);

            if (node.Type != BVHNodeTypes.EndSite)
            {
                node.Channels = ReadChannels(reader);
            }


            while (true)
            {
                line = reader.ReadLine().ToLower().Trim();

                if (line == "}")
                {
                    return node;
                }
                else
                {
                    node.Children.Add(ReadNode(reader, line, depth + 1));
                }
            }
        }

        /// <summary>
        /// read BVH channels definition from the current line
        /// </summary>
        private static BVHChannels[] ReadChannels(StreamReader reader)
        {
            string line = reader.ReadLine().ToLower().Trim();
            string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens[0] != "channels")
                throw new FileFormatException("Expected CHANNELS keyword");

            int numChannels = Int32.Parse(tokens[1]);

            if (tokens.Length != numChannels + 2)
                throw new FileFormatException(
                    $"Invalid CHANNELs Definition: {numChannels} expected, but {tokens.Length - 2} found");

            BVHChannels[] channels = new BVHChannels[numChannels];
            for (int i = 0; i < numChannels; i++)
            {
                if (!Enum.TryParse<BVHChannels>(tokens[i + 2], true, out channels[i]))
                    throw new FileFormatException($"Invalid channel: {tokens[i + 2]}");
            }

            return channels;
        }

        /// <summary>
        /// reads the OFFSET definition line of a bvh file
        /// </summary>
        private static Vector3D ReadOffset(StreamReader reader)
        {
            string line = reader.ReadLine().ToLower().Trim();
            string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens[0] != "offset")
                throw new FileFormatException("Expected OFFSET keyword");
            if (tokens.Length != 4)
                throw new FileFormatException("OFFSET Definiton: Invalid number of values");

            double x, y, z;
            if (!Double.TryParse(tokens[1], out x))
                throw new FileFormatException("Could not parse OFFSET definition x-component");
            if (!Double.TryParse(tokens[2], out y))
                throw new FileFormatException("Could not parse OFFSET definition y-component");
            if (!Double.TryParse(tokens[3], out z))
                throw new FileFormatException("Could not parse OFFSET definition z-component");


            return new Vector3D(x, y, z);
        }

        /// <summary>
        /// Writes a BVH motion data file
        /// </summary>
        public static void WriteBvh(string file, BVHNode root, BVHMotionData motionData)
        {
            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine("HIERARCHY");
                WriteBvhNode(root, writer, 0);
                writer.WriteLine("MOTION");

                int numFrames = motionData.Data.First().Value.Count;
                writer.WriteLine($"Frames: {numFrames}");
                writer.WriteLine($"Frame Time: {motionData.FrameTime}");

                // write frame data line by line
                for (int i = 0; i < numFrames; i++)
                {
                    foreach (var node in motionData.Data.Keys)
                    {
                        if (node.Type == BVHNodeTypes.EndSite)
                            continue;

                        double yaw, pitch, roll;
                        motionData.Data[node][i].ToYawPitchRoll(out yaw, out pitch, out roll);

                        writer.Write($"{yaw * 180 / Math.PI} {pitch * 180 / Math.PI} {roll * 180 / Math.PI} ");
                    }
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// recursively write a BVH node an all its children to .Type == BVHNodeTypes.EndSitedata file
        /// </summary>
        private static void WriteBvhNode(BVHNode node, StreamWriter writer, int level)
        {
            // name and type
            for (int i = 0; i < level - 1; ++i)
                writer.Write("\t");

            if (node.Type == BVHNodeTypes.EndSite) // end sites have no name
                writer.WriteLine($"{GetTypeString(node.Type)}");
            else
                writer.WriteLine($"{GetTypeString(node.Type)} {node.Name}");

            // open curly bracket
            for (int i = 0; i < level - 1; ++i)
                writer.Write("\t");
            writer.WriteLine("{");

            // node offset
            for (int i = 0; i < level; ++i)
                writer.Write("\t");
            writer.WriteLine($"OFFSET {node.Offset.X} {node.Offset.Y} {node.Offset.Z}");

            if (node.Type != BVHNodeTypes.EndSite)
            {
                // defined channels
                for (int i = 0; i < level; ++i)
                    writer.Write("\t");
                writer.Write($"CHANNELS {node.Channels.Length} ");

                foreach (var chan in node.Channels)
                    writer.Write(chan.ToString() + " ");
                writer.Write(Environment.NewLine);

                // child nodes
                foreach (var child in node.Children)
                    WriteBvhNode(child, writer, level + 1);
            }

            // closing curly bracket
            for (int i = 0; i < level - 1; ++i)
                writer.Write("\t");
            writer.WriteLine("}");
        }

        private static string GetTypeString(BVHNodeTypes type)
        {
            switch (type)
            {
                case BVHNodeTypes.Root:
                    return "ROOT";
                case BVHNodeTypes.Joint:
                    return "JOINT";
                case BVHNodeTypes.EndSite:
                    return "End Site";
                default:
                    throw new InvalidOperationException("Invalid node type");
            }
        }
    }
}
