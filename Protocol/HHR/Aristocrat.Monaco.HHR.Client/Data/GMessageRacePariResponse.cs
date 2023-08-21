namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Nested binary structure within HHR message GMessageRacePariResponse
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CTemplatePool
    {
        /// <summary />
        public uint LastPariPrize1;

        /// <summary />
        public uint LastPariPrize2;

        /// <summary />
        public ulong PariMutualPoolRace1;

        /// <summary />
        public ulong PariMutualPoolRace2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.NumPrizeDataRace)]
        public int[] PrizeDataRace;

        /// <summary />
        public long PariBalance;

        /// <summary />
        public long MathPoolGuaranteedBalance;
    }

    /// <summary>
    ///     Binary structure for HHR message of type CMD_RACE_PARI
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageRacePariResponse
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.NumTemplatePool)]
        public CTemplatePool[] Data;
    }
}