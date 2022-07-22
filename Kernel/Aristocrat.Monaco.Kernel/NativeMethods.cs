namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        #region Defines types of dumps that can be generated
        /// <summary>
        ///     Include just the information necessary to capture stack traces for all existing threads in a process
        /// </summary>
        internal const int MiniDumpNormal = 0x00000000;

        /// <summary>
        ///     Include the data sections from all loaded modules. This results in the inclusion of global variables, which can
        ///     make the minidump
        ///     file significantly larger.For per-module control, use the ModuleWriteDataSeg enumeration value from
        ///     MODULE_WRITE_FLAGS.
        /// </summary>
        internal const int MiniDumpWithDataSegs = 0x00000001;

        /// <summary>
        ///     Include all accessible memory in the process. The raw memory data is included at the end, so that the initial
        ///     structures can be
        ///     mapped directly without the raw memory information.This option can result in a very large file
        /// </summary>
        internal const int MiniDumpWithFullMemory = 0x00000002;

        /// <summary>
        ///     Include high-level information about the operating system handles that are active when the minidump is made
        /// </summary>
        internal const int MiniDumpWithHandleData = 0x00000004;

        /// <summary>
        ///     Stack and backing store memory written to the minidump file should be filtered to remove all but the
        ///     pointer values necessary to reconstruct a stack trace
        /// </summary>
        internal const int MiniDumpFilterMemory = 0x00000008;

        /// <summary>
        ///     Stack and backing store memory should be scanned for pointer references to modules in the module list. If a module
        ///     is referenced by
        ///     stack or backing store memory, the ModuleWriteFlags member of the MINIDUMP_CALLBACK_OUTPUT structure is set to
        ///     ModuleReferencedByMemory.
        /// </summary>
        internal const int MiniDumpScanMemory = 0x00000010;

        /// <summary>
        ///     Include information from the list of modules that were recently unloaded, if this information is maintained by the
        ///     operating system.
        /// </summary>
        internal const int MiniDumpWithUnloadedModules = 0x00000020;

        /// <summary>
        ///     Include pages with data referenced by locals or other stack memory. This option can increase the size of the
        ///     minidump file significantly.
        /// </summary>
        internal const int MiniDumpWithIndirectlyReferencedMemory = 0x00000040;

        /// <summary>
        ///     Filter module paths for information such as user names or important directories. This option may prevent the system
        ///     from locating the image
        ///     file and should be used only in special situations.
        /// </summary>
        internal const int MiniDumpFilterModulePaths = 0x00000080;

        /// <summary>
        ///     Include complete per-process and per-thread information from the operating system.
        /// </summary>
        internal const int MiniDumpWithProcessThreadData = 0x00000100;

        /// <summary>
        ///     Scan the virtual address space for PAGE_READWRITE memory to be included.
        /// </summary>
        internal const int MiniDumpWithPrivateReadWriteMemory = 0x00000200;

        /// <summary>
        ///     Reduce the data that is dumped by eliminating memory regions that are not essential to meet criteria specified for
        ///     the dump.This can
        ///     avoid dumping memory that may contain data that is private to the user. However, it is not a guarantee that no
        ///     private information will be present.
        /// </summary>
        internal const int MiniDumpWithoutOptionalData = 0x00000400;

        /// <summary>
        ///     Include memory region information. For more information, see MINIDUMP_MEMORY_INFO_LIST.
        /// </summary>
        internal const int MiniDumpWithFullMemoryInfo = 0x00000800;

        /// <summary>
        ///     Include thread state information. For more information, see MINIDUMP_THREAD_INFO_LIST.
        /// </summary>
        internal const int MiniDumpWithThreadInfo = 0x00001000;

        /// <summary>
        ///     Include all code and code-related sections from loaded modules to capture executable content. For per-module
        ///     control, use the
        ///     ModuleWriteCodeSegs enumeration value from MODULE_WRITE_FLAGS.
        /// </summary>
        internal const int MiniDumpWithCodeSegs = 0x00002000;

        #endregion

        /// <summary>
        ///     Signature of native method that will generate the mini dump
        /// </summary>
        /// <param name="hProcess">A handle to the process for which the information is to be generated</param>
        /// <param name="processId">The identifier of the process for which the information is to be generated.</param>
        /// <param name="hFile">A handle to the file in which the information is to be written</param>
        /// <param name="dumpType">
        ///     The type of information to be generated. This parameter can be one or more of the values from
        ///     the MINIDUMP_TYPE enumeration
        /// </param>
        /// <param name="exceptionParam">
        ///     A pointer to a MINIDUMP_EXCEPTION_INFORMATION structure describing the client exception
        ///     that caused the minidump to be generated. If the value of this parameter is NULL, no exception information is
        ///     included in the minidump file
        /// </param>
        /// <param name="userStreamParam">
        ///     A pointer to a MINIDUMP_USER_STREAM_INFORMATION structure. If the value of this parameter
        ///     is NULL, no user-defined information is included in the minidump file
        /// </param>
        /// <param name="callackParam">
        ///     A pointer to a MINIDUMP_CALLBACK_INFORMATION structure that specifies a callback routine
        ///     which is to receive extended minidump information. If the value of this parameter is NULL, no callbacks are
        ///     performed
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is TRUE; otherwise, the return value is FALSE. To retrieve extended
        ///     error information, call GetLastError. Note that the last error will be an HRESULT value.
        ///     If the operation is canceled, the last error code is HRESULT_FROM_WIN32(ERROR_CANCELLED).
        /// </returns>
        [DllImport("dbghelp.dll")]
        public static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            int processId,
            IntPtr hFile,
            int dumpType,
            IntPtr exceptionParam,
            IntPtr userStreamParam,
            IntPtr callackParam);
    }
}