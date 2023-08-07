namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using log4net;

    /// <summary>
    ///     This class handles resolving an assembly when the runtime cannot find it
    /// </summary>
    public class AssemblyResolver : IService, IDisposable
    {
        private const string AssemblyResolverPathExtensionPoint = "/Kernel/AssemblyResolver/FileSystemPath";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();

        private bool _disposed;

        /// <inheritdoc />
        /// >
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => nameof(AssemblyResolver);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(AssemblyResolver) };

        /// <inheritdoc />
        public void Initialize()
        {
            IndexFiles(new DirectoryInfo(Directory.GetCurrentDirectory()), true);

            // Load the directories to be recursively searched
            LoadPathsToScan();

            AssemblyLoadContext.Default.Resolving += AssemblyResolveHandler;
        }

        /// <summary>
        ///     Disposes of resources
        /// </summary>
        /// <param name="disposing">true, if disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                AssemblyLoadContext.Default.Resolving -= AssemblyResolveHandler;
            }

            _disposed = true;
        }

        private void LoadPathsToScan()
        {
            var selectedNodes =
                MonoAddinsHelper.GetSelectedNodes<FileSystemPathNode>(AssemblyResolverPathExtensionPoint);
            foreach (var selectedNode in selectedNodes)
            {
                if (!string.IsNullOrEmpty(selectedNode.FileSystemPath))
                {
                    if (Directory.Exists(selectedNode.FileSystemPath))
                    {
                        IndexFiles(new DirectoryInfo(selectedNode.FileSystemPath), selectedNode.Recursive);
                    }
                    else
                    {
                        Logger.Warn(
                            $"Path {selectedNode.Addin.Id} in addin {AssemblyResolverPathExtensionPoint} for extension point {selectedNode.FileSystemPath} is not valid");
                    }
                }
                else
                {
                    Logger.Warn(
                        $"Path in addin {selectedNode.Addin.Id} for extension point {AssemblyResolverPathExtensionPoint} was empty or null");
                }
            }
        }

        private Assembly AssemblyResolveHandler(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (!_files.TryGetValue(assemblyName.Name, out var fullPath))
            {
                return null;
            }

            return IsAssembly(fullPath) ? Assembly.LoadFrom(fullPath) : null;

            bool IsAssembly(string path)
            {
                var extension = Path.GetExtension(path);

                return extension == ".dll" || extension == ".exe";
            }
        }

        private void IndexFiles(DirectoryInfo directory, bool recursive)
        {
            var files = Directory.GetFiles(
                directory.FullName,
                "*",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);

                if (!_files.ContainsKey(key))
                {
                    _files.Add(key, file);
                }
            }
        }
    }
}