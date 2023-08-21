namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_GAME_PLAY
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GMessageGamePlay
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PlayerIdLength)]
        public string PlayerId;

        /// <summary />
        public uint GameId;

        /// <summary />
        public ushort CreditsPlayed;

        /// <summary />
        public byte FreePlayMode;

        /// <summary />
        public uint ForcePattern;

        /// <summary />
        public uint ForceCheck;

        /// <summary />
        public ushort LinesPlayed;

        /// <summary />
        public ushort GameMode;

        /// <summary />
        public uint RaceTicketSetId;
    }
}