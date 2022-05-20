namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_CLOSE_TRAN
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageCloseTran
    {
        /// <summary />
        public ushort Status;
    }
}