namespace Aristocrat.Monaco.Bootstrap
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public delegate void Logger(string message);

        [Flags]
        public enum ErrorModes : uint
        {
            SystemDefault = 0x0,
            SemFailCriticalErrors = 0x0001,
            SemNoAlignmentFaultExcept = 0x0004,
            SemNoGpFaultErrorBox = 0x0002,
            SemNoOpenFileErrorBox = 0x8000
        }

        public enum RetOnSuccess : long
        {
            ExceptionExecuteHandler = 1,
            ExceptionContinueSearch = 0,
            ExceptionContinueExecution = -1
        }

        [DllImport("CrashHandler.dll", CharSet = CharSet.Unicode)]
        public static extern void RegisterCrashHandler(
            string name,
            string folder = "",
            int maxCallstack = -1,
            long retOnSuccess = (long)RetOnSuccess.ExceptionExecuteHandler);

        [DllImport("CrashHandler.dll")]
        public static extern void UnRegisterCrashHandler();

        [DllImport("CrashHandler.dll")]
        public static extern void SetCrashHandlerLogger(Logger logger);

        [DllImport("CrashHandler.dll")]
        public static extern void DoCrash();

        [DllImport("CrashHandler.dll")]
        public static extern void DoCrashInAnotherThread();

        [DllImport("user32.dll")]
        public static extern void DisableProcessWindowsGhosting();

        [DllImport("kernel32.dll")]
        public static extern ErrorModes SetErrorMode(ErrorModes uMode);
    }
}