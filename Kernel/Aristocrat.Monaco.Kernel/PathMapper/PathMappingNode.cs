namespace Aristocrat.Monaco.Kernel
{
    using System;
    using Mono.Addins;

    /// <summary>
    ///     Definition of the PathMapping class.
    /// </summary>
    [CLSCompliant(false)]
    public class PathMappingNode : ExtensionNode
    {
        [NodeAttribute("createIfNotExists")] private readonly bool _createIfNotExists;

        [NodeAttribute("fileSystemPath")] private readonly string _fileSystemPath;

        [NodeAttribute("platformPath")] private readonly string _platformPath;

        [NodeAttribute("relativeTo")] private readonly string _relativeTo;

        [NodeAttribute("absolutePathName")] private readonly string _absolutePathName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathMappingNode" /> class.
        /// </summary>
        public PathMappingNode()
        {
            _platformPath = string.Empty;
            _fileSystemPath = string.Empty;
            _relativeTo = string.Empty;
            _createIfNotExists = false;
            _absolutePathName = string.Empty;
        }

        /// <summary>Gets the string of the platform path</summary>
        public string PlatformPath => _platformPath;

        /// <summary>Gets the string of the file system path</summary>
        public string FileSystemPath => _fileSystemPath;

        /// <summary>Gets the string of the "relative to" node</summary>
        public string RelativeTo => _relativeTo;

        /// <summary>Gets a value indicating whether the directory should be created if it does not exist</summary>
        public bool CreateIfNotExists => _createIfNotExists;

        /// <summary>The absolute path name can be used to override file system path or relative to with a name property</summary>
        public string AbsolutePathName => _absolutePathName;
    }
}