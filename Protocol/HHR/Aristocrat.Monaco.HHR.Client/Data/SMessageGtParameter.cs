namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using System.Runtime.InteropServices;

    /// <summary>
    ///     Binary structure for HHR message of type CMD_PARAMETER_GT
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SMessageGtParameter
    {
        /// <summary />
        public uint SiteId;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.ReservedLength)]
        public string ReservedStr;

        /// <summary>
        ///     Parameter Device Id
        /// </summary>
        public uint ParameterDeviceId;

        /// <summary />
        public byte EnabledFlag;

        /// <summary />
        public byte DebugFlag;

        /// <summary />
        public byte Reserved;

        /// <summary />
        public uint SnapTime;

        /// <summary />
        public uint SyncTime;

        /// <summary />
        public ushort HbInterval;

        /// <summary />
        public uint OverrideLimit;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.LocationLength)]
        public string Location;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.KeyLength)]
        public byte[] MessageKey;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.KeyLength)]
        public byte[] BarcodeKey;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.ProgramLength)]
        public string ProgramName;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.VersionLength)]
        public string ProgramVersion;

        /// <summary />
        public uint InputType;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.DeviceLength)]
        public string DeviceName;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.PhoneLength)]
        public string PhoneNo;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.VersionLength)]
        public string ServerVersion;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.NameLength)]
        public string ManufacturerName;

        /// <summary />
        public char LogLevel;

        /// <summary />
        public char EventLevel;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.SiteLength)]
        public string SiteName;

        /// <summary />
        public byte LanguageId;

        /// <summary />
        public uint ShowPlayerPin;

        /// <summary />
        public int GtId;

        /// <summary />
        public int BillLimit;

        /// <summary />
        public int IrsLimit;

        /// <summary />
        public int LockupLimit;

        /// <summary />
        public int BillCapacity;

        /// <summary />
        public byte GameSoundVol;

        /// <summary />
        public byte AttractVol;

        /// <summary />
        public int Reserved1;

        /// <summary />
        public byte AllowReprint;

        /// <summary />
        public int GtBank;

        /// <summary />
        public int CashoutTimeout;

        /// <summary />
        public int HelpTimeout;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.FileNameLength)]
        public string LogoFileName;

        /// <summary />
        public byte FoodButton;

        /// <summary />
        public byte ATMButton;

        /// <summary />
        public byte MusicButton;

        /// <summary />
        public byte DropTicket;

        /// <summary />
        public int ConsolationDaubTime;

        /// <summary />
        public int GameEndDaubTime;

        /// <summary />
        public int ReelStopTime;

        /// <summary />
        public int ConsolationBallDropTime;

        /// <summary />
        public int GameEndBallDropTime;

        /// <summary />
        public int Reserved2;

        /// <summary />
        public int DefaultGameId;

        /// <summary />
        public int GameIdCount;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxGtGames)]
        public int[] GameIds;

        /// <summary />
        public int Reserved0;

        /// <summary />
        public int FreePlayDuration;

        /// <summary />
        public int Reserved4;

        /// <summary />
        public uint FreePlayCredits;

        /// <summary />
        public byte FreePlayMode;

        /// <summary />
        public int MaxWaitForGameTime;

        /// <summary />
        public int TimeoutWaitForMoneyCashout;

        /// <summary />
        public int TimeoutConnectingCashout;

        /// <summary />
        public int gameEndingWait;

        /// <summary />
        public byte bingoReelDisplay;

        /// <summary />
        public byte multiDropScope;

        /// <summary />
        public int expirationDays;

        /// <summary />
        public byte playerTracking;

        /// <summary />
        public int countTypeDefault;

        /// <summary />
        public int countdownResetDefault;

        /// <summary />
        public int awardDefault;

        /// <summary />
        public int hotPlayerEvalTimeDefault;

        /// <summary />
        public int hotPlayerWagerDefault;

        /// <summary />
        public int hotPlayerGamesDefault;

        /// <summary />
        public int hotPlayerResetTimeDefault;

        /// <summary />
        public int abandonedCardTimeDefault;

        /// <summary />
        public int statusUpdateTimeDefault;

        /// <summary />
        public byte voucherStatus;

        /// <summary />
        public int handPayDeviceId;

        /// <summary />
        public int handPayWinLimit;

        /// <summary />
        public int handPayCreditLimit;

        /// <summary />
        public int ticketConfigurationId;

        /// <summary />
        public int expireEgm;

        /// <summary />
        public int expireCage;

        /// <summary />
        public byte roundUp;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TypeLength)]
        public string expirePrint;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TicketTitleLength)]
        public string cashoutTicketTitle;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TicketTitleLength)]
        public string promoTicketTitle;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TicketTitleLength)]
        public string restrictedTicketTitle;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TicketTitleLength)]
        public string duplicateTicketSuffix;

        /// <summary />
        public int ticketType;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string gameStartMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string gameEndMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string voucherInMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string voucherOutMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string cashInMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string cashOutMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string accountValidMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string accountInvalidMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string cardReadErrorMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string abandonedCardMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string idleMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string offlineMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string cardInMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string cardOutMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string awardMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string attractIdleMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string VIPAccountValidMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string annAccountValidMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string empAccountValidMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string certificateInMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string certificateOutMsgDefault;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string birAccountValidMsgDefault;

        /// <summary />
        public int accountNumberTrack;

        /// <summary />
        public byte optimizations;

        /// <summary />
        public byte ticketInEnabled;

        /// <summary />
        public uint lastTransId;

        /// <summary />
        public double eodTime;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.CityLength)]
        public string siteCity;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.StateLength)]
        public string siteState;

        /// <summary />
        public int gameMode;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string attractMsgDefault;

        /// <summary />
        public byte tournamentCount;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxGtTournaments)]
        public int[] tournamentIds;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.TournamentDescLength)]
        public string tournamentDescriptions;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageLengthConstants.MaxGtPromos)]
        public int[] promoIds;

        /// <summary />
        public byte promoCount;

        /// <summary />
        public int interfaceType;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.AddressLength)]
        public string siteAddress;

        /// <summary />
        public byte lockupAllProgressives;

        /// <summary />
        public byte cashoutNonCashCredits;

        /// <summary />
        public byte convertPromoCreditsToCash;

        /// <summary />
        public byte cashoutPromoCredits;

        /// <summary />
        public int historyRecordCount;

        /// <summary />
        public int daubType;

        /// <summary />
        public int attractSoundMode;

        /// <summary />
        public int sigKeyLen;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.SignatureKeyLength)]
        public string sigKey;

        /// <summary />
        public byte printW2G;

        /// <summary />
        public int autoRemoteKeyOffWaitTime;

        /// <summary />
        public int systemDropMode;

        /// <summary />
        public byte showBingoElements;

        /// <summary />
        public char EZBetFlag;

        /// <summary />
        public int defaultMsgDisplayTime;

        /// <summary />
        public int maxTicketInCents;

        /// <summary />
        public int creditLimitCents;

        /// <summary />
        public int sysReservedInt0;

        /// <summary />
        public int sysReservedInt1;

        /// <summary />
        public int sysReservedInt2;

        /// <summary />
        public int sysReservedInt3;

        /// <summary />
        public int sysReservedInt4;

        /// <summary />
        public int sysReservedInt5;

        /// <summary />
        public int sysReservedInt6;

        /// <summary />
        public int sysReservedInt7;

        /// <summary />
        public int sysReservedInt8;

        /// <summary />
        public int sysReservedInt9;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr0;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr3;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr4;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr5;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr6;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr7;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr8;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string sysReservedStr9;

        /// <summary />
        public int bankReservedInt0;

        /// <summary />
        public int bankReservedInt1;

        /// <summary />
        public int bankReservedInt2;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string bankReservedStr0;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string bankReservedStr1;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.MessageLength)]
        public string bankReservedStr2;

        /// <summary />
        public int HandicapLockWinAmount;

        /// <summary />
        public int HandicapStatTimer;

        /// <summary />
        public int HandicapPickTimer;

        /// <summary />
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MessageLengthConstants.UdpIpLength)]
        public string UdpIp;
    }
}