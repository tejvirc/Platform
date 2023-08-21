namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_PROG_INFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SMessageProgressiveInfo
    {
        /// <summary />
        public uint ProgressiveId;

        /// <summary />
        public uint ProgLevel;

        /// <summary />
        public uint ProgCurrentValue;

        /// <summary />
        public uint ProgResetValue;

        /// <summary />
        public uint ProgContribPercent;

        /// <summary />
        public uint ProgReservePercent;

        /// <summary />
        public uint ProgMaximum;

        /// <summary />
        public uint ProgCreditsBet;
    }
}