namespace Aristocrat.Monaco.Kernel
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Definition of the PathNode class.
    /// </summary>
    internal class PathNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PathNode" /> class.
        /// </summary>
        /// <param name="name">The name of the PathNode</param>
        public PathNode(string name)
        {
            FileSystemPath = string.Empty;
            Name = name;
        }

        /// <summary>Gets or sets the Name of the PathNode</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the FileSystemPath associated with this platform PathNode if it is a leaf</summary>
        public string FileSystemPath { get; set; }

        /// <summary>Gets the Children PathNodes</summary>
        public List<PathNode> Children { get; } = new List<PathNode>();

        /// <summary>Add a PathNode by name to this PathNode</summary>
        /// <param name="name">The name of the node to add</param>
        /// <returns>The newly created node that was added</returns>
        public PathNode AddPathNode(string name)
        {
            var node = new PathNode(name);

            Children.Add(node);

            return node;
        }

        /// <summary>Gets the child PathNode with the specified name if it exists</summary>
        /// <param name="childName">The name of the child node</param>
        /// <param name="childNode">The PathNode reference to set</param>
        /// <returns>True if the child node was found, false otherwise</returns>
        public bool TryGetChildNode(string childName, out PathNode childNode)
        {
            var children = Children.Where(pathNode => pathNode.Name == childName).ToList();
            childNode = null;
            if (children.Count == 1)
            {
                childNode = children.ElementAt(0);
                return true;
            }

            return false;
        }
    }
}