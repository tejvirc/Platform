namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_PARAMETER_REQUEST
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageParameterRequest
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TypeLength)]
        public string DeviceType;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.SerialNumberLength)]
        public string SerialNumber;

        /// <summary>
        ///     Currently has an active player
        /// </summary>
        public byte ActivePlayer;

        /// <summary>
        ///     Currently has this card checked out
        /// </summary>
        public uint CardNo;
    }
}