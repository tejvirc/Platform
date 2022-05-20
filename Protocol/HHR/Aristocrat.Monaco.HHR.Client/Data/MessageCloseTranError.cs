namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_CLOSE_TRAN_ERROR
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageCloseTranError
    {
        /// <summary />
        public ushort Status;

        /// <summary />
        public uint RetryTime;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string ErrorText;

        /// <summary />
        public uint ErrorCode;
    }
}