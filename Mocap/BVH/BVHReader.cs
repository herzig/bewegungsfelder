using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Mocap.BVH
{
    public class BVHReader
    {
        /// <summary>
        /// reads BVH hierarchical data from a BVH file
        /// </summary>
        public static BVHNode ReadBvhHierarchy(string file, out Dictionary<BVHNode, List<Quaternion>> motionData)
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

        public static Dictionary<BVHNode, List<Quaternion>> ReadMotionData(StreamReader reader, BVHNode root)
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

            Dictionary<BVHNode, List<Quaternion>> motionData = new Dictionary<BVHNode, List<Quaternion>>();
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().ToLower().Trim();
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                double[] frameData = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(t => double.Parse(t)).ToArray();

                int offset = 0;
                ReadFrameData(root, motionData, frameData, ref offset);
            }
            return motionData;
        }

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

            int ignoredOffset = 0;
            if (node.Channels[0] == BVHChannels.Xposition)
                ignoredOffset += 3;

            if (node.Channels[ignoredOffset] != BVHChannels.Zrotation
                || node.Channels[ignoredOffset + 1] != BVHChannels.Xrotation
                || node.Channels[ignoredOffset + 2] != BVHChannels.Yrotation)
            {
                throw new FileFormatException("Joint Channels have to be in ZXY order");
            }

            // convert rotation to quaternion
            var qz = new Quaternion(new Vector3D(0, 0, 1), nodevalues[ignoredOffset]);
            var qx = new Quaternion(new Vector3D(1, 0, 0), nodevalues[ignoredOffset + 1]);
            var qy = new Quaternion(new Vector3D(0, 1, 0), nodevalues[ignoredOffset + 2]);

            Quaternion quat = qz * qx * qy;

            motionData[node].Add(quat);

            foreach (var item in node.Children)
            {
                ReadFrameData(item, motionData, values, ref offset);
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
    }
}
