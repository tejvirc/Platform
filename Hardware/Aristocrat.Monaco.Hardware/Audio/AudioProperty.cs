namespace Aristocrat.Monaco.Hardware.Audio
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Specifies the FMTID/PID identifier that programmatically identifies a property.
    /// </summary>
    /// <remarks>
    ///     MSDN Reference: http://msdn.microsoft.com/en-us/library/bb773381.aspx
    ///     Note: This item is external to CoreAudio API, and is defined in the Windows Property System API.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioProperty
    {
        /// <summary>
        ///     A unique GUID for the property.
        /// </summary>
        public Guid fmtid;

        /// <summary>
        ///     A property identifier (PID).
        /// </summary>
        public int pid;
    }
}