namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_HEARTBEAT_CLOSE_TRAN
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageCloseTranHeartbeat
    {
        /// <summary />
        public ushort Status;

        /// <summary />
        public byte EnabledFlag;

        /// <summary />
        public byte DebugFlag;
    }
}