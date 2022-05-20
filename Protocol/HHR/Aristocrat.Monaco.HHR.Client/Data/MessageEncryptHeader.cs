namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for encrypted HHR message header. This header wraps all TCP
    ///     messages to and from the server.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MessageEncryptHeader
    {
        /// <summary />
        public ushort EncryptionType;

        /// <summary />
        public int EncryptionLength;

        /// <summary />
        public ushort Crc;
    }
}