namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using log4net;
    using NativeOS.Services.OS;

    /// <summary>
    ///     Defines types of dumps that can be generated
    /// </summary>
    public static class MiniDumpType
    {
        /// <summary>
        ///     Include just the information necessary to capture stack traces for all existing threads in a process
        /// </summary>
        public const int MiniDumpNormal = 0x00000000;

        /// <summary>
        ///     Include the data sections from all loaded modules. This results in the inclusion of global variables, which can
        ///     make the minidump
        ///     file significantly larger.For per-module control, use the ModuleWriteDataSeg enumeration value from
        ///     MODULE_WRITE_FLAGS.
        /// </summary>
        public const int MiniDumpWithDataSegs = 0x00000001;

        /// <summary>
        ///     Include all accessible memory in the process. The raw memory data is included at the end, so that the initial
        ///     structures can be
        ///     mapped directly without the raw memory information.This option can result in a very large file
        /// </summary>
        public const int MiniDumpWithFullMemory = 0x00000002;

        /// <summary>
        ///     Include high-level information about the operating system handles that are active when the minidump is made
        /// </summary>
        public const int MiniDumpWithHandleData = 0x00000004;

        /// <summary>
        ///     Stack and backing store memory written to the minidump file should be filtered to remove all but the
        ///     pointer values necessary to reconstruct a stack trace
        /// </summary>
        public const int MiniDumpFilterMemory = 0x00000008;

        /// <summary>
        ///     Stack and backing store memory should be scanned for pointer references to modules in the module list. If a module
        ///     is referenced by
        ///     stack or backing store memory, the ModuleWriteFlags member of the MINIDUMP_CALLBACK_OUTPUT structure is set to
        ///     ModuleReferencedByMemory.
        /// </summary>
        public const int MiniDumpScanMemory = 0x00000010;

        /// <summary>
        ///     Include information from the list of modules that were recently unloaded, if this information is maintained by the
        ///     operating system.
        /// </summary>
        public const int MiniDumpWithUnloadedModules = 0x00000020;

        /// <summary>
        ///     Include pages with data referenced by locals or other stack memory. This option can increase the size of the
        ///     minidump file significantly.
        /// </summary>
        public const int MiniDumpWithIndirectlyReferencedMemory = 0x00000040;

        /// <summary>
        ///     Filter module paths for information such as user names or important directories. This option may prevent the system
        ///     from locating the image
        ///     file and should be used only in special situations.
        /// </summary>
        public const int MiniDumpFilterModulePaths = 0x00000080;

        /// <summary>
        ///     Include complete per-process and per-thread information from the operating system.
        /// </summary>
        public const int MiniDumpWithProcessThreadData = 0x00000100;

        /// <summary>
        ///     Scan the virtual address space for PAGE_READWRITE memory to be included.
        /// </summary>
        public const int MiniDumpWithPrivateReadWriteMemory = 0x00000200;

        /// <summary>
        ///     Reduce the data that is dumped by eliminating memory regions that are not essential to meet criteria specified for
        ///     the dump.This can
        ///     avoid dumping memory that may contain data that is private to the user. However, it is not a guarantee that no
        ///     private information will be present.
        /// </summary>
        public const int MiniDumpWithoutOptionalData = 0x00000400;

        /// <summary>
        ///     Include memory region information. For more information, see MINIDUMP_MEMORY_INFO_LIST.
        /// </summary>
        public const int MiniDumpWithFullMemoryInfo = 0x00000800;

        /// <summary>
        ///     Include thread state information. For more information, see MINIDUMP_THREAD_INFO_LIST.
        /// </summary>
        public const int MiniDumpWithThreadInfo = 0x00001000;

        /// <summary>
        ///     Include all code and code-related sections from loaded modules to capture executable content. For per-module
        ///     control, use the
        ///     ModuleWriteCodeSegs enumeration value from MODULE_WRITE_FLAGS.
        /// </summary>
        public const int MiniDumpWithCodeSegs = 0x00002000;
    }

    /// </summary>
    /// </summary>
    public class MiniDumpCreator
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
                    SystemMiniDumpCreator.MiniDumpWriteDump(
                        process.Handle,
                        process.Id,
                        fs.SafeFileHandle.DangerousGetHandle(),
                        MiniDumpType.MiniDumpNormal,
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