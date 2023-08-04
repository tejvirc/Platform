namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Contracts;
    using Hardware.Contracts.VHD;
    using Kernel;
    using log4net;

    public class RuntimeProvider : IRuntimeProvider, IDisposable
    {
        // This is only to support a mode where there is no runtime ISO - Development only
        private const string FallbackDirectory = @"bin";

        private const string RuntimeWildcardSearch =
            GamingConstants.RuntimePackagePrefix + @"*." + GamingConstants.PackageExtension;

        private static readonly string RuntimeFullPrefix = $"{GamingConstants.RuntimePackagePrefix}_";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, (VirtualDiskHandle mount, string version)> _mounts =
            new Dictionary<string, (VirtualDiskHandle mount, string version)>();

        private readonly IPathMapper _pathMapper;
        private readonly IVirtualDisk _virtualDisk;

        private bool _disposed;

        public RuntimeProvider(IVirtualDisk virtualDisk, IPathMapper pathMapper)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _virtualDisk = virtualDisk ?? throw new ArgumentNullException(nameof(virtualDisk));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public string DefaultInstance { get; private set; } = FallbackDirectory;

        public string FindTargetRuntime(Regex pattern)
        {
            return _mounts.OrderByDescending(m => m.Value.version, new VersionComparer())
                .FirstOrDefault(m => pattern.IsMatch(m.Value.version)).Key;
        }

        public void Load()
        {
            var packages = _pathMapper.GetDirectory(GamingConstants.PackagesPath);

            var files = packages.GetFiles(RuntimeWildcardSearch, SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                Logger.Error($"Failed to find runtime packages in {packages.FullName}");
                return;
            }

            foreach (var runtime in files)
            {
                Mount(runtime);
            }

            SetDefaultRuntime();
        }

        public void Unload()
        {
            var mounts = new Dictionary<string, (VirtualDiskHandle mount, string version)>(_mounts);

            foreach (var instance in mounts)
            {
                UnMount(instance.Key, instance.Value.mount);
            }

            _mounts.Clear();
        }

        public void Unload(string runtimeId)
        {
            var runtime = _mounts.FirstOrDefault(
                m => m.Key.EndsWith(runtimeId, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(runtime.Key))
            {
                UnMount(runtime.Key, runtime.Value.mount);
            }

            SetDefaultRuntime();
        }

        public string GetRuntimeHostFilename(string runtimeFolder)
        {
            var filename = Directory.GetFiles(
                runtimeFolder,
                $"*{GamingConstants.RuntimeHost}",
                SearchOption.AllDirectories)
                .FirstOrDefault();
            return Path.Combine(runtimeFolder, filename);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Unload();
            }

            _disposed = true;
        }

        private void Mount(FileSystemInfo runtime)
        {
            var name = Path.GetFileNameWithoutExtension(runtime.Name);

            var runtimePath = Path.Combine(_pathMapper.GetDirectory(GamingConstants.RuntimePath).FullName, name);

            SafeDirectoryDelete(runtimePath);

            Directory.CreateDirectory(runtimePath);

            var handle = _virtualDisk.AttachImage(runtime.FullName, runtimePath);
            if (handle.IsInvalid)
            {
                Logger.Error($"Failed to mount {runtime.FullName} at {runtimePath}");
                return;
            }

            // Defaults to the version from the file name like (ATI_Runtime_)1.0.0.0 or similar
            //  This will allow for pattern matching when can't read the file version
            var version = name.Replace(RuntimeFullPrefix, string.Empty);

            var runtimeFile = GetRuntimeHostFilename(runtimePath);

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(runtimeFile);

                if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                {
                    version = versionInfo.FileVersion;
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Failed to get file version info from: {runtimeFile}", e);
            }

            _mounts.Add(runtimePath, (handle, version));

            Logger.Debug($"Mounted runtime {runtime.FullName} at {runtimePath} with version {version}");
        }

        private void UnMount(string path, VirtualDiskHandle handle)
        {
            _virtualDisk.DetachImage(handle, path);
            handle.Dispose();

            SafeDirectoryDelete(path);

            _mounts.Remove(path);

            Logger.Debug($"Unmounted runtime from {path}");
        }

        private static void SafeDirectoryDelete(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Unable to delete directory {path} - {e.Message}");
            }
        }

        private class VersionComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var xVersion = GetVersion(x);
                var yVersion = GetVersion(y);

                if (xVersion != null && yVersion != null)
                {
                    return xVersion.CompareTo(yVersion);
                }

                return string.Compare(x, y, StringComparison.InvariantCulture);

                Version GetVersion(string text)
                {
                    return !Version.TryParse(text, out var version) ? null : version;
                }
            }
        }

        private void SetDefaultRuntime()
        {
            Version version = null;

            // This will essentially make the most current runtime with the oldest major.minor version the default
            foreach (var mount in _mounts.OrderBy(f => f.Value.version, new VersionComparer()))
            {
                var runtimeVersion = mount.Value.version;

                if (!Version.TryParse(runtimeVersion, out var current))
                {
                    continue;
                }

                if (version == null || version.Major == current.Major && version.Minor == current.Minor)
                {
                    version = current;
                    DefaultInstance = mount.Key;

                    continue;
                }

                break;
            }
        }
    }
}