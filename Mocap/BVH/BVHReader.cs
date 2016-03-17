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
        public static BVHNode ReadBvhHierarchy(string file)
        {
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine().ToLower().Trim();
                if (line != "hierarchy")
                    throw new FileFormatException("File has to start with HIERARCHY keyword");

                var root = ReadNode(reader, reader.ReadLine(), 0);

                return root;
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
