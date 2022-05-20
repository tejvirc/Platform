namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Nested binary structure within HHR message SMessageGameBonanza
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CRaceStat
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.NumStats)]
        public uint[] StatNum;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.DescriptionLength)]
        public string StatStr;
    }

    /// <summary>
    ///     Nested binary structure within HHR message SMessageGameBonanza
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CRaceData
    {
        /// <summary />
        public uint RaceId;

        /// <summary />
        public uint Runners;

        /// <summary />
        public uint RaceNumber;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.DescriptionLength)]
        public string TrackDescription;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.RaceDateLength)]
        public string RaceDate;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.HorseStringLength)]
        public string HorseActual;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.HorseStringLength)]
        public string HorseOdds;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.HorseStringLength)]
        public string HorseSelection;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.NumRaceStat)]
        public CRaceStat[] Racestat;
    }

    /// <summary>
    ///     Nested binary structure within HHR message SMessageGameBonanza
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CRaceInfo
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.NumRaceData)]
        public CRaceData[] RaceData;

        /// <summary />
        public uint HandicapData;

        /// <summary />
        public uint PariPrize1;

        /// <summary />
        public uint PariPrize2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PrizeLength)]
        public string PrizeRaceSet1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PrizeLength)]
        public string PrizeRaceSet2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.NumRaceData)]
        public uint[] ProgWon;
    }

    /// <summary>
    ///     Binary structure for HHR message of type CMD_GAME_BONANZA
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SMessageGameBonanza
    {
        /// <summary />
        public uint GameId;

        /// <summary />
        public uint GameNo;

        /// <summary />
        public uint SeqNo;

        /// <summary />
        public byte BOverride;

        /// <summary />
        public uint ScratchTicketSetId;

        /// <summary />
        public uint ScratchTicketSubSetId;

        /// <summary />
        public uint ScratchTicketId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string Prize;

        /// <summary />
        public CRaceInfo RaceInfo;

        /// <summary />
        public uint HandicapEnter;

        /// <summary />
        public uint LastGamePlayTime;
    }
}