namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_GT_READY_TO_PLAY
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageReadyToPlay
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.PinLength)]
        public byte[] Pin;
    }
}