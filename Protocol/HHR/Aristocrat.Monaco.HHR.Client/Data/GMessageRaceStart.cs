namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_RACE_START
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageRaceStart
    {
        /// <summary />
        public uint GameId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PlayerIdLength)]
        public string PlayerId;

        /// <summary />
        public ushort CreditsPlayed;

        /// <summary />
        public ushort LinesPlayed;

        /// <summary />
        public uint RaceTicketSetId;

        /// <summary />
        public uint RaceTicketId;

        /// <summary />
        public CRaceInfo RaceInfo;
    }
}