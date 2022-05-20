namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_GAME_RECOVER_RESPONSE
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageGameRecoverResponse
    {
        /// <summary />
        public uint GameId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PlayerIdLength)]
        public string PlayerId;

        /// <summary />
        public uint RaceTicketSetId;

        /// <summary />
        public uint RaceTicketId;

        /// <summary />
        public uint PrizeLoc1;

        /// <summary />
        public uint PrizeLoc2;

        /// <summary />
        public uint LastGamePlayTime;

        /// <summary />
        public CRaceInfo RaceInfo;
    }
}