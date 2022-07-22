namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using log4net;

    /// <summary>
    ///     Helps in the creation of Mini Memory Dumps
    /// </summary>
    public static class MiniDumpCreator
    {
        private const string Logs = @"/Logs";
        private const string DmpExt = ".dmp";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Creates the dump file
        /// </summary>
        /// <param name="process">The process to dump</param>
        public static void Create(Process process)
        {
            var dmpPath = Path.Combine(new[] { GetLogsDirectory(), process.ProcessName + DmpExt });

            Logger.Debug($"Creating dumpfile, process id: {process.Id}, file: {dmpPath}");

            using (var fs = new FileStream(dmpPath, FileMode.Create))
            {
                if (fs.SafeFileHandle != null)
                {
                    NativeMethods.MiniDumpWriteDump(
                        process.Handle,
                        process.Id,
                        fs.SafeFileHandle.DangerousGetHandle(),
                        NativeMethods.MiniDumpNormal,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
                else
                {
                    Logger.Error($"Failed to write dumpfile, process: {process.Id}, file: {dmpPath}");
                }
            }
        }

        private static string GetLogsDirectory()
        {
            var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();

            var logDir = pathMapper.GetDirectory(Logs).FullName;

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            return logDir;
        }
    }
}