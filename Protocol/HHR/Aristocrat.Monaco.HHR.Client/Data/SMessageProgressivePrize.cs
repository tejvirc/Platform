namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_PROGRESSIVE_PRIZE
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SMessageProgressivePrize
    {
        // NOTE: This message is not encrypted and does not have an EncMsgHdr

        /// <summary />
        public uint Id;

        /// <summary />
        public uint Amount;

        /// <summary />
        public uint Status;

        /// <summary />
        public uint CreditsBet;
    }
}