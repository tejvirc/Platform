namespace Aristocrat.Monaco.Kernel
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the FileSystemPathNode class.
    /// </summary>
    [CLSCompliant(false)]
    public class FileSystemPathNode : ExtensionNode
    {
        /// <summary>The path to be used for assembly resolving</summary>
        [NodeAttribute("fileSystemPath")] private readonly string _fileSystemPath;

        /// <summary>Whether the location should be recursively searched</summary>
        [NodeAttribute("recursive")] private readonly bool _recursive;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSystemPathNode" /> class.
        /// </summary>
        public FileSystemPathNode()
        {
            _fileSystemPath = string.Empty;
            _recursive = true;
        }

        /// <summary>Gets the string of the path</summary>
        public string FileSystemPath => _fileSystemPath;

        /// <summary>Gets a value indicating whether the location should be recursively searched</summary>
        public bool Recursive => _recursive;
    }
}