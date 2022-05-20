namespace Platform.Launcher.Utilities
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ChildProcess
    {
        // Windows will automatically close any open job handles when our process terminates.
        //  This can be verified by using SysInternals' Handle utility. When the job handle
        //  is closed, the child processes will be killed.
        private static readonly IntPtr JobHandle;

        static ChildProcess()
        {
            if (Environment.OSVersion.Version < new Version(6, 2))
            {
                return;
            }

            var jobName = "ChildProcessTracker" + Process.GetCurrentProcess().Id;
            JobHandle = NativeMethods.CreateJobObject(IntPtr.Zero, jobName);

            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOBOBJECTLIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            };
            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION {BasicLimitInformation = info};

            var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            var extendedInfoPtr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                NativeMethods.SetInformationJobObject(JobHandle, JobObjectInfoType.ExtendedLimitInformation,
                    extendedInfoPtr, (uint) length);
            }
            finally
            {
                Marshal.FreeHGlobal(extendedInfoPtr);
            }
        }

        /// <summary>
        ///     Add the process to be tracked. If our current process is killed, the child processes
        ///     that we are tracking will be automatically killed, too. If the child process terminates
        ///     first, that's fine, too.
        /// </summary>
        /// <param name="process"></param>
        public static void Track(Process process)
        {
            if (JobHandle != IntPtr.Zero)
            {
                var success = NativeMethods.AssignProcessToJobObject(JobHandle, process.Handle);
                if (!success)
                {
                    throw new Win32Exception();
                }
            }
        }
    }
}