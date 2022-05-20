namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Nested binary structure within HHR message SMessageGameOpen
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CRacePattern
    {
        /// <summary />
        public uint PrizeValue;

        /// <summary />
        public uint RaceGroup;

        /// <summary />
        public byte Pattern1;

        /// <summary />
        public byte Pattern2;

        /// <summary />
        public byte Pattern3;

        /// <summary />
        public byte Pattern4;

        /// <summary />
        public byte Pattern5;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PrizeLength)]
        public string Prize;
    }

    /// <summary>
    ///     Nested binary structure within HHR message SMessageGameOpen
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CRacePatterns
    {
        /// <summary />
        public uint Line;

        /// <summary />
        public uint Button;

        /// <summary />
        public uint Credits;

        /// <summary />
        public uint RaceTicketSetId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxNumPatterns)]
        public CRacePattern[] Pattern;
    }

    /// <summary>
    ///     Nested binary structure within HHR message SMessageGameOpen
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct CRaceTicketSets
    {
        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxNumTickets)]
        public CRacePatterns[] TicketSet;
    }

    /// <summary>
    ///     Binary structure for HHR message of type CMD_GAME_OPEN
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SMessageGameOpen
    {
        /// <summary />
        public uint GameId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.NameLength)]
        public string GameName;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.VersionLength)]
        public string GameVersion;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.DllLength)]
        public string GameDLL;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.DllLength)]
        public string GraphicsDLL;

        /// <summary />
        public uint Denomination;

        /// <summary />
        public uint ProgIdForLed;

        /// <summary />
        public uint MaxLines;

        /// <summary />
        public uint MaxCredits;

        /// <summary />
        public uint GameNumbersId;

        /// <summary />
        public uint RedLightPayout;

        /// <summary />
        public uint RingBellPayout;

        /// <summary />
        public uint MinimumBet;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.VersionLength)]
        public string GameNumbersVer;

        /// <summary />
        public uint ProgValueForLed;

        /// <summary />
        public byte GameType;

        /// <summary />
        public byte ScatterSymbol;

        /// <summary />
        public byte ScatterValue;

        /// <summary />
        public uint PayoutPercentage;

        /// <summary />
        public byte SymbolCount;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.PatternSetNormLength)]
        public uint[] PatternSetNorm;

        /// <summary />
        public uint GameFaceId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxNumProgs)]
        public uint[] ProgressiveIds;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.GameTriggerLength)]
        public byte[] GameTrigger;

        /// <summary />
        public uint progMaxCents;

        /// <summary />
        public uint Ante;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxNumProgs)]
        public uint[] SpecialSetId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerLine1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerActiveLine1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] ButtonActiveLine1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] PayoutLine1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerLine2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerActiveLine2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] ButtonActiveLine2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] PayoutLine2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerLine3;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerActiveLine3;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] ButtonActiveLine3;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] PayoutLine3;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerLine4;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerActiveLine4;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] ButtonActiveLine4;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] PayoutLine4;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerLine5;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] WagerActiveLine5;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] ButtonActiveLine5;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.WagerLength)]
        public uint[] PayoutLine5;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.EncMessagesLength)]
        public string EncMessages;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.EncTicketsLength)]
        public string EncTickets;

        /// <summary />
        public CRaceTicketSets RaceTicketSets;

        /// <summary />
        public uint RaceInformationButtonX;

        /// <summary />
        public uint RaceInformationButtonY;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.RaceInformationLength)]
        public string RaceInformationStr;

        /// <summary />
        public uint RaceInformationQuickStop;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxNumProgs)]
        public uint[] ProgCreditsBet;
    }
}