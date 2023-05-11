namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     Provides a mechanism for mapping a platform relative path to a file system absolute path
    /// </summary>
    public class PathMapper : IService, IPathMapper
    {
        private const string PathMappingExtensionPath = "/Kernel/PathMapping";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PathNode _rootNode = new(string.Empty);

        private readonly ConcurrentDictionary<string, DirectoryInfo> _directories = new();

        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathMapper" /> class.
        /// </summary>
        public PathMapper()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathMapper" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        public PathMapper(IPropertiesManager properties)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public DirectoryInfo GetDirectory(string platformPath)
        {
            return _directories.GetOrAdd(platformPath, _ => InternalGet(platformPath));
        }

        /// <inheritdoc />
        public string Name => "Path Mapper";

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPathMapper) };

        /// <inheritdoc />
        public void Initialize()
        {
            var extensionNodeList = AddinManager.GetExtensionNodes(PathMappingExtensionPath);

            foreach (PathMappingNode node in extensionNodeList)
            {
                Logger.Info($"Adding mapping: {node.PlatformPath} => {node.FileSystemPath} / {node.RelativeTo}");

                AddPath(
                    node.PlatformPath,
                    node.FileSystemPath,
                    node.RelativeTo,
                    node.AbsolutePathName,
                    node.CreateIfNotExists,
                    _rootNode);
            }
        }

        private static List<string> VerifyPlatformPath(string platformPath)
        {
            if (platformPath[0] != '/')
            {
                throw new ArgumentException($"The platform path {platformPath} is invalid", nameof(platformPath));
            }

            platformPath = platformPath.Substring(1);

            if (platformPath[platformPath.Length - 1] == '/')
            {
                platformPath = platformPath.Substring(0, platformPath.Length - 1);
            }

            return new List<string>(platformPath.Split('/'));
        }

        private static bool FindLeaf(IEnumerator partsEnumerator, ref PathNode node)
        {
            // Point to the first element in the enumerator
            partsEnumerator.Reset();
            partsEnumerator.MoveNext();

            var partName = (string)partsEnumerator.Current;

            while (node.TryGetChildNode(partName, out var nextNode))
            {
                node = nextNode;

                if (partsEnumerator.MoveNext())
                {
                    // If the partsEnumerator can be advanced then we just keep going
                    partName = (string)partsEnumerator.Current;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private void AddPath(
            string platformPath,
            string fileSystemPath,
            string relativeTo,
            string absolutePathName,
            bool createIfNotExists,
            PathNode rootNode)
        {
            Logger.Info(
                $"Adding platform path: {platformPath}, with system path: {fileSystemPath}, relative path: {relativeTo}");

            PathNode node;

            if (rootNode == null)
            {
                throw new ArgumentException("Root node cannot be null", nameof(rootNode));
            }

            var haveRelativePath = !string.IsNullOrEmpty(relativeTo);
            var haveFileSystemPath = !string.IsNullOrEmpty(fileSystemPath);

            if (!string.IsNullOrEmpty(absolutePathName) &&
                Directory.Exists(_properties.GetValue(absolutePathName, string.Empty)))
            {
                fileSystemPath = _properties.GetValue(absolutePathName, string.Empty);
            }

            if (haveRelativePath && !haveFileSystemPath)
            {
                var referredParts = VerifyPlatformPath(relativeTo);
                IEnumerator referredEnumerator = referredParts.GetEnumerator();
                node = rootNode;

                // Copy system path from referred to referring
                if (FindLeaf(referredEnumerator, ref node))
                {
                    fileSystemPath = node.FileSystemPath;

                    var referrerParts = VerifyPlatformPath(platformPath);

                    foreach (var part in referrerParts)
                    {
                        fileSystemPath = Path.Combine(fileSystemPath, part);
                    }

                    if (!Directory.Exists(fileSystemPath))
                    {
                        if (createIfNotExists)
                        {
                            Logger.Info($"The {fileSystemPath} directory does not exist and will be created");
                            Directory.CreateDirectory(fileSystemPath);
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"File system path (after composition): '{fileSystemPath}' must be an existing directory");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException(
                        $"Platform path '{platformPath}' refers to a non-existing path: {relativeTo}");
                }
            }
            else if (haveFileSystemPath && createIfNotExists && !Directory.Exists(fileSystemPath))
            {
                Logger.Info($"The {fileSystemPath} directory does not exist and will be created");
                Directory.CreateDirectory(fileSystemPath);
            }
            else
            {
                if (haveFileSystemPath && haveRelativePath)
                {
                    throw new ArgumentException("File system path and relative path cannot both be defined");
                }

                if (!haveFileSystemPath)
                {
                    throw new ArgumentException("File system path and relative path cannot both be null or empty");
                }

                if (!Directory.Exists(fileSystemPath))
                {
                    throw new ArgumentException($"File system path must be an existing directory: {fileSystemPath}");
                }
            }

            var platformParts = VerifyPlatformPath(platformPath);
            IEnumerator partsEnumerator = platformParts.GetEnumerator();
            node = rootNode;

            // Start trying to find the deepest leaf node that matches our platform path
            if (FindLeaf(partsEnumerator, ref node))
            {
                // If we got to the end of the path (a leaf), then we need to assign the file system path to this node.
                // If it already has a name and it is different from the one we are trying to assign, then it is an error.
                if (string.IsNullOrEmpty(node.FileSystemPath))
                {
                    node.FileSystemPath = fileSystemPath;
                }
                else if (!node.FileSystemPath.Equals(fileSystemPath))
                {
                    throw new ArgumentException(
                        $"Node {node.Name} already has a file system path assigned to it: {node.FileSystemPath}",
                        nameof(platformPath));
                }
            }
            else
            {
                // If we got here we know two things are true.
                // 1. The current node we're evaluating did not have a child node that matched the current part of our platform path that we are evaluating
                // 2. It's also impossible that the node we are evaluating has the same name  as the platform path part we are evaluating because if it did, we would have successfully retrieved it from its parent and set the file system path in the test above. So, we must create this new node
                node = node.AddPathNode((string)partsEnumerator.Current);

                // Now we need to just iterate on the remaining parts of the platform path creating new nodes as we go and then assigning the last node we create to the file system path
                while (partsEnumerator.MoveNext())
                {
                    node = node.AddPathNode((string)partsEnumerator.Current);
                }

                node.FileSystemPath = fileSystemPath;
            }
        }

        private DirectoryInfo InternalGet(string path)
        {
            var node = _rootNode;

            var platformPathParts = VerifyPlatformPath(path);
            IEnumerator platformPathPartsEnumerator = platformPathParts.GetEnumerator();

            var actualPath = string.Empty;
            var lastPart = string.Empty;
            var brokeOut = false;

            while (platformPathPartsEnumerator.MoveNext())
            {
                lastPart = (string)platformPathPartsEnumerator.Current;
                if (node.TryGetChildNode(lastPart, out var nextNode))
                {
                    node = nextNode;
                }
                else
                {
                    brokeOut = true;
                    break;
                }
            }

            if (node.FileSystemPath != null)
            {
                actualPath = node.FileSystemPath;

                if (brokeOut)
                {
                    actualPath = $"{actualPath}{Path.DirectorySeparatorChar}{lastPart}";
                }

                while (platformPathPartsEnumerator.MoveNext())
                {
                    actualPath = $"{actualPath}{Path.DirectorySeparatorChar}{platformPathPartsEnumerator.Current}";
                }
            }

            if (string.IsNullOrEmpty(actualPath))
            {
                return null;
            }

            if (!Directory.Exists(actualPath))
            {
                Logger.Warn($"Path {actualPath} does not exist");

                return null;
            }

            Logger.Debug($"Path {actualPath} exists");

            return new DirectoryInfo(actualPath);
        }
    }
}