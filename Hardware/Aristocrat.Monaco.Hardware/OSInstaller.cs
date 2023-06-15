namespace Aristocrat.Monaco.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.VHD;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    public class OSInstaller : IOSInstaller
    {
        private const string TempPath = @"temp";
        private const string DownloadsPath = "/Downloads";
        private const string ToolsPath = @"/Tools";
        private const string OsInstaller = @"Upgrade_OS.cmd";
        private const string WinUpdateExtension = @"winUpdate";
        private const string IsoExtension = @"iso";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPathMapper _pathMapper;
        private readonly IVirtualDisk _virtualDisk;

        public OSInstaller()
            : this(
                ServiceManager.GetInstance().GetService<IVirtualDisk>(),
                ServiceManager.GetInstance().GetService<IPathMapper>())
        {
        }

        public OSInstaller(IVirtualDisk virtualDisk, IPathMapper pathMapper)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _virtualDisk = virtualDisk ?? throw new ArgumentNullException(nameof(virtualDisk));
        }

        public bool DeviceChanged => true;

        public ExitAction? ExitAction => Kernel.Contracts.ExitAction.Reboot;

        public event EventHandler UninstallStartedEventHandler;

        public bool Install(string packageId)
        {
            VirtualDiskHandle handle = null;
            bool success;

            Logger.Debug("OS Install started");

            // OS installs are handled in place

            var file = GetFileInfo(packageId);

            var root = Path.GetDirectoryName(file?.FullName);
            if (string.IsNullOrEmpty(root))
            {
                return false;
            }

            var tempPath = Path.Combine(root, TempPath);

            // The provided package is an iso with the winUpdate extension.
            //  It needs to be renamed because the virtual disk APIs don't support files with 
            var link = Path.Combine(tempPath, Path.ChangeExtension(file.Name, IsoExtension));
            if (string.IsNullOrEmpty(link))
            {
                return false;
            }

            var mountPath = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(link));

            try
            {
                SafeFileDelete(link);

                SafeDirectoryDelete(mountPath);

                Directory.CreateDirectory(tempPath);

                if (!NativeMethods.CreateHardLink(link, file.FullName, IntPtr.Zero))
                {
                    Logger.Info($"Failed to create hard link: {link}");
                    return false;
                }

                Directory.CreateDirectory(mountPath);

                handle = _virtualDisk.AttachImage(link, mountPath);
                if (handle.IsInvalid)
                {
                    return false;
                }

                var files = Directory.GetFiles(mountPath);
                foreach (var mountedFile in files)
                {
                    Logger.Debug($"Mount point contains the following: {mountedFile}");
                }

                var directory = _pathMapper.GetDirectory(ToolsPath);

                var osInstaller = Path.Combine(directory.FullName, OsInstaller);

                // This is a blocking call.  May need to handle cancelling due to the execution time
                success = ExecuteCommand(osInstaller, mountPath);
            }
            finally
            {
                if (handle != null && !handle.IsInvalid)
                {
                    _virtualDisk.DetachImage(handle, mountPath);

                    handle.Dispose();
                }

                SafeDirectoryDelete(mountPath);

                SafeFileDelete(link);
            }

            Logger.Info($"OS Install completed: {success}");

            return success;
        }

        public bool Uninstall(string packageId)
        {
            UninstallStartedEventHandler?.Invoke(this, EventArgs.Empty);

            return false;
        }

        public string Name => typeof(OSInstaller).Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IOSInstaller) };

        public void Initialize()
        {
        }

        private static bool ExecuteCommand(string fileName, string arguments)
        {
            Logger.Debug($"Preparing to run: ({fileName} {arguments})");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    RedirectStandardOutput = false
                }
            };

            process.Start();

            Logger.Info($"Running process: ({fileName} {arguments})");

            process.WaitForExit();

            Logger.Info($"Process ({fileName} {arguments}) returned with exit code: {process.ExitCode}");

            return process.ExitCode == 0;
        }

        private static void SafeDirectoryDelete(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void SafeFileDelete(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }

            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete - {file}", ex);
            }
        }

        private FileInfo GetFileInfo(string packageId)
        {
            var packages = _pathMapper.GetDirectory(DownloadsPath);

            return packages.GetFiles($"{packageId}.{WinUpdateExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        }
    }
}