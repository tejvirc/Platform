namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     The message types that exist in the HHR protocol, and whose presence in the header
    ///     indicates the length and format of the data that will follow.
    /// </summary>
    public enum Command
    {
        /// <summary>
        ///     Indicates that there was NO command number, for example if we never receive a response from the server.
        /// </summary>
        CmdInvalidCommand = 0,

        /// <summary>
        /// </summary>
        CmdCloseTran = 1,

        /// <summary>
        /// </summary>
        CmdCloseTranError = 2,

        /// <summary>
        /// </summary>
        CmdHeartbeat = 4,

        /// <summary>
        /// </summary>
        CmdHeartbeatCloseTran = 5,

        /// <summary>
        /// </summary>
        CmdParameterRequest = 6,

        /// <summary>
        /// </summary>
        CmdParameter = 7,

        /// <summary>
        /// </summary>
        CmdParameterGt = 8,

        /// <summary>
        /// </summary>
        CmdGameRequest = 9,

        /// <summary>
        /// </summary>
        CmdGameOpen = 10,

        //CMD_CARD_INSERTED = 12,

        /// <summary>
        /// </summary>
        CmdCard = 13,

        //CMD_CARD_REMOVED = 14,

        /// <summary>
        /// </summary>
        CmdCashout = 15,

        /// <summary>
        /// </summary>
        CmdGame = 19,

        /// <summary>
        /// </summary>
        CmdGamePlay = 20,

        /// <summary>
        /// </summary>
        CmdDrop = 23,

        /// <summary>
        /// </summary>
        CmdDropData = 24,

        /// <summary>
        /// </summary>
        CmdDropComplete = 25,

        /// <summary>
        /// </summary>
        CmdLogon = 26,

        /// <summary>
        /// </summary>
        CmdLogonReply = 27,

        /// <summary>
        /// </summary>
        CmdLogoff = 28,

        /// <summary>
        /// </summary>
        CmdOpenSession = 29,

        /// <summary>
        /// </summary>
        CmdCloseSession = 30,

        /// <summary>
        /// </summary>
        CmdCloseSessionReply = 31,

        /// <summary>
        /// </summary>
        CmdCreateAccount = 32,

        /// <summary>
        /// </summary>
        CmdCreateAccountReply = 33,

        /// <summary>
        /// </summary>
        CmdCreateAccountComplete = 34,

        /// <summary>
        /// </summary>
        CmdAddMoney = 35,

        /// <summary>
        /// </summary>
        /// <summary>
        /// </summary>
        CmdAddMoneyReply = 36,

        /// <summary>
        /// </summary>
        CmdAddMoneyComplete = 37,

        /// <summary>
        /// </summary>
        CmdCheckAccount = 38,

        /// <summary>
        /// </summary>
        CmdCheckAccountReply = 39,

        /// <summary>
        /// </summary>
        CmdWithdrawMoney = 40,

        /// <summary>
        /// </summary>
        /// <summary>
        /// </summary>
        CmdWithdrawMoneyReply = 41,

        /// <summary>
        /// </summary>
        CmdWithdrawMoneyComplete = 42,

        /// <summary>
        /// </summary>
        CmdCashoutTicket = 43,

        /// <summary>
        /// </summary>
        CmdCashoutTicketReply = 44,

        /// <summary>
        /// </summary>
        CmdFillDrawer = 45,

        /// <summary>
        /// </summary>
        CmdFillDrawerReply = 46,

        /// <summary>
        /// </summary>
        CmdBleedDrawer = 47,

        /// <summary>
        /// </summary>
        CmdBleedDrawerReply = 48,

        /// <summary>
        /// </summary>
        CmdOpenDrawer = 49,

        /// <summary>
        /// </summary>
        CmdReprintPcccard = 50,

        /// <summary>
        /// </summary>
        CmdReprintPcccardReply = 51,

        /// <summary>
        /// </summary>
        CmdSiteConfigUpdate = 52,

        /// <summary>
        /// </summary>
        CmdDeviceConfigUpdate = 53,

        /// <summary>
        /// </summary>
        CmdGameConfigUpdate = 54,

        /// <summary>
        /// </summary>
        CmdEventMonitorUpdate = 55,

        /// <summary>
        /// </summary>
        CmdEventMonitorGame = 56,

        /// <summary>
        /// </summary>
        CmdSuspend = 57,

        /// <summary>
        /// </summary>
        CmdOpenSessionReply = 60,

        /// <summary>
        /// </summary>
        CmdAdjustMoney = 61,

        /// <summary>
        /// </summary>
        CmdAdjustMoneyReply = 62,

        /// <summary>
        /// </summary>
        CmdCashDrawer = 63,

        /// <summary>
        /// </summary>
        CmdCashDrawerReply = 64,

        /// <summary>
        /// </summary>
        CmdGtParamUpdate = 68,

        /// <summary>
        /// </summary>
        CmdForceDrop = 69,

        /// <summary>
        /// </summary>
        CmdDropAll = 70,

        /// <summary>
        /// </summary>
        CmdVersionUpdate = 71,

        /// <summary>
        /// </summary>
        CmdLostPin = 74,

        /// <summary>
        /// </summary>
        CmdPauseAll = 75,

        /// <summary>
        /// </summary>
        CmdForcePause = 76,

        /// <summary>
        /// </summary>
        CmdResumeAll = 77,

        /// <summary>
        /// </summary>
        CmdForceResume = 78,

        /// <summary>
        /// </summary>
        CmdAddPlayer = 79,

        /// <summary>
        /// </summary>
        CmdRemovePlayer = 80,

        /// <summary>
        /// </summary>
        CmdConfigPlayers = 81,

        /// <summary>
        /// </summary>
        CmdRequestPlayerData = 82,

        /// <summary>
        /// </summary>
        CmdSearchPcc = 85,

        /// <summary>
        /// </summary>
        CmdSearchPccReply = 86,

        /// <summary>
        /// </summary>
        CmdChargeAccount = 89,

        /// <summary>
        /// </summary>
        CmdCharge = 90,

        /// <summary>
        /// </summary>
        CmdRefundAccount = 91,

        /// <summary>
        /// </summary>
        CmdRefund = 92,

        /// <summary>
        /// </summary>
        CmdEftAccount = 93,

        /// <summary>
        /// </summary>
        CmdEft = 94,

        /// <summary>
        /// </summary>
        CmdRequestCard = 95,

        /// <summary>
        /// </summary>
        CmdNewCard = 96,

        /// <summary>
        /// </summary>
        CmdGameAbort = 97,

        /// <summary>
        /// </summary>
        CmdConsolationDaub = 98,

        /// <summary>
        /// </summary>
        CmdConsolationDaubResponse = 99,

        /// <summary>
        /// </summary>
        CmdGameendingDaub = 100,

        /// <summary>
        /// </summary>
        CmdSleeperGame = 101,

        /// <summary>
        /// </summary>
        CmdGameOver = 102,

        /// <summary>
        /// </summary>
        CmdDaub = 103,

        /// <summary>
        /// </summary>
        CmdInactive1 = 104,

        /// <summary>
        /// </summary>
        CmdInactive2 = 105,

        /// <summary>
        /// </summary>
        CmdSessionKey = 106,

        /// <summary>
        /// </summary>
        CmdPatternSetReq = 107,

        /// <summary>
        /// </summary>
        CmdPatternSet = 108,

        /// <summary>
        /// </summary>
        CmdGtbankUpdate = 109,

        /// <summary>
        /// </summary>
        CmdGtMove = 110,

        /// <summary>
        /// </summary>
        CmdCardRemoveProcessed = 111,

        //CMD_EXCHANGE_POINTS = 112,

        /// <summary>
        /// </summary>
        CmdUpdateGameFace = 113,

        /// <summary>
        /// </summary>
        CmdUpdateFreeplay = 120,

        /// <summary>
        /// </summary>
        CmdBgcVoucherCreate = 122,

        /// <summary>
        /// </summary>
        CmdBgcVoucherRedeemed = 123,

        /// <summary>
        /// </summary>
        CmdBgcVoucherNotredeemed = 124,

        /// <summary>
        /// </summary>
        CmdTpsVoucherAccepted = 125,

        /// <summary>
        /// </summary>
        CmdTpsVoucherRejected = 126,

        /// <summary>
        /// </summary>
        CmdBgcVoucherRedeemrequest = 127,

        /// <summary>
        /// </summary>
        CmdTpsVoucherRedeemauthorized = 128,

        /// <summary>
        /// </summary>
        CmdTpsVoucherRedeemrejected = 129,

        /// <summary>
        /// </summary>
        CmdBgcEvent = 130,

        /// <summary>
        /// </summary>
        CmdBgcDailyMeter = 131,

        /// <summary>
        /// </summary>
        CmdBgcDropMeter = 132,

        /// <summary>
        /// </summary>
        CmdBgcGameInit = 133,

        /// <summary>
        /// </summary>
        CmdBgcGameOffline = 134,

        /// <summary>
        /// </summary>
        CmdBgcGameEnable = 135,

        /// <summary>
        /// </summary>
        CmdBgcGameDisable = 136,

        /// <summary>
        /// </summary>
        CmdTpsGameParams = 137,

        /// <summary>
        /// </summary>
        CmdBgcSystemOnline = 138,

        /// <summary>
        /// </summary>
        CmdTpsSystemInit = 139,

        /// <summary>
        /// </summary>
        CmdBgcDayMetersComplete = 140,

        //CMD_BGC_CARD_INSERTED = 141,

        //CMD_BGC_CARD_REMOVED = 142,

        //CMD_TPS_ACCOUNT_VALID = 143,

        //CMD_TPS_ACCOUNT_INVALID = 144,

        /// <summary>
        /// </summary>
        CmdBgcAbandonedCard = 145,

        /// <summary>
        /// </summary>
        CmdBgcSessionStatus = 146,

        /// <summary>
        /// </summary>
        CmdBgcLogicalCardRemoved = 147,

        /// <summary>
        /// </summary>
        CmdBgcJackpot = 148,

        /// <summary>
        /// </summary>
        CmdBgcHotPlayer = 149,

        /// <summary>
        /// </summary>
        CmdBgcInitialized = 152,

        /// <summary>
        /// </summary>
        CmdBgcShutdown = 153,

        //CMD_PT_CARD_INSERTED = 154,

        //CMD_PT_CARD_REMOVED = 155,

        /// <summary>
        /// </summary>
        CmdPtSessionStatus = 156,

        /// <summary>
        /// </summary>
        CmdPtLogicalCardRemoved = 157,

        //CMD_PT_ACCOUNT_VALID = 158,

        //CMD_PT_ACCOUNT_INVALID = 159,

        /// <summary>
        /// </summary>
        CmdPtAbandonedCard = 160,

        /// <summary>
        /// </summary>
        CmdPtHotPlayer = 161,

        /// <summary>
        /// </summary>
        CmdGameEnable = 162,

        /// <summary>
        /// </summary>
        CmdGameDisable = 163,

        /// <summary>
        /// </summary>
        CmdGameOffline = 164,

        /// <summary>
        /// </summary>
        CmdTcktBarcodeRequest = 165,

        /// <summary>
        /// </summary>
        CmdTcktGetBarcode = 166,

        /// <summary>
        /// </summary>
        CmdTcktCreated = 167,

        /// <summary>
        /// </summary>
        CmdTcktAccepted = 168,

        /// <summary>
        /// </summary>
        CmdTcktRejected = 169,

        /// <summary>
        /// </summary>
        CmdTcktRedeemRequest = 170,

        /// <summary>
        /// </summary>
        CmdTcktRedeemAuthorized = 171,

        /// <summary>
        /// </summary>
        CmdTcktRedeemRejected = 172,

        /// <summary>
        /// </summary>
        CmdTcktRedeemed = 173,

        /// <summary>
        /// </summary>
        CmdTcktNotredeemed = 174,

        /// <summary>
        /// </summary>
        CmdBgcBarcodeRequest = 175,

        /// <summary>
        /// </summary>
        CmdTpsBarcode = 176,

        /// <summary>
        /// </summary>
        CmdUnsupported = 178,

        /// <summary>
        /// </summary>
        CmdTpsConnectionlost = 179,

        /// <summary>
        /// </summary>
        CmdTransaction = 180,

        /// <summary>
        /// </summary>
        CmdPlayerRequest = 181,

        /// <summary>
        /// </summary>
        CmdPlayerRequestResponse = 182,

        /// <summary>
        /// </summary>
        CmdGameBonanza = 185,

        /// <summary>
        /// </summary>
        CmdLocalGamePlayed = 189,

        /// <summary>
        /// </summary>
        CmdEnterTournamentMode = 190,

        /// <summary>
        /// </summary>
        CmdStartTournament = 191,

        /// <summary>
        /// </summary>
        CmdStopTournament = 192,

        /// <summary>
        /// </summary>
        CmdExitTournamentMode = 193,

        /// <summary>
        /// </summary>
        CmdPromoRequest = 194,

        /// <summary>
        /// </summary>
        CmdPromo = 195,

        /// <summary>
        /// </summary>
        CmdCommand = 196,

        /// <summary>
        /// </summary>
        CmdCardInfo = 197,

        //CMD_BGC_CARD_INFO = 198,

        /// <summary>
        /// </summary>
        CmdBgcReadyToPlay = 199,

        /// <summary>
        /// </summary>
        CmdBgcGameOpen = 200,

        /// <summary>
        /// </summary>
        CmdTpsCommand = 201,

        /// <summary>
        /// </summary>
        CmdBgcValidateCard = 202,

        /// <summary>
        /// </summary>
        CmdTpsLock = 203,

        /// <summary>
        /// </summary>
        CmdBgcComputeChecksum = 204,

        /// <summary>
        /// </summary>
        CmdGtReadyToPlay = 205,

        /// <summary>
        /// </summary>
        CmdGtComputeChecksum = 206,

        /// <summary>
        /// </summary>
        CmdBgcAuthorizeAction = 207,

        /// <summary>
        /// </summary>
        CmdTpsActionResponse = 208,

        /// <summary>
        /// </summary>
        CmdBgcSetAttribute = 209,

        /// <summary>
        /// </summary>
        CmdAuthorizeAction = 210,

        /// <summary>
        /// </summary>
        CmdActionResponse = 211,

        /// <summary>
        /// </summary>
        CmdSetAttribute = 212,

        /// <summary>
        /// </summary>
        CmdGetAttributes = 213,

        /// <summary>
        /// </summary>
        CmdGetAttributesResponse = 214,

        /// <summary>
        /// </summary>
        CmdGtConnected = 215,

        /// <summary>
        /// </summary>
        CmdBgcGetAttributes = 216,

        /// <summary>
        /// </summary>
        CmdTpsGetAttributesResponse = 217,

        /// <summary>
        /// </summary>
        CmdBgcGtConnected = 218,

        /// <summary>
        /// </summary>
        CmdBgcGtNotification = 219,

        /// <summary>
        /// </summary>
        CmdCardGetType = 220,

        /// <summary>
        /// </summary>
        CmdCardType = 221,

        /// <summary>
        /// </summary>
        CmdCardError = 222,

        /// <summary>
        /// </summary>
        CmdCardLoginFailed = 223,

        /// <summary>
        /// </summary>
        CmdCardPtLogin = 224,

        /// <summary>
        /// </summary>
        CmdCardPtInfo = 225,

        /// <summary>
        /// </summary>
        CmdCardPtLogout = 226,

        /// <summary>
        /// </summary>
        CmdCardCaLogin = 227,

        /// <summary>
        /// </summary>
        CmdCardCaInfo = 228,

        /// <summary>
        /// </summary>
        CmdCardCaLogout = 229,

        /// <summary>
        /// </summary>
        CmdGameResults = 230,

        /// <summary>
        /// </summary>
        CmdServerAddressRequest = 232,

        /// <summary>
        /// </summary>
        CmdServerAddress = 233,

        /// <summary>
        /// </summary>
        CmdConnect = 234,

        /// <summary>
        /// </summary>
        CmdBgcHeartbeatMeters = 235,

        /// <summary>
        /// </summary>
        CmdHandpayCreate = 236,

        /// <summary>
        /// </summary>
        CmdHandpayKeyoff = 237,

        /// <summary>
        /// </summary>
        CmdRemoteHandpayKeyoff = 238,

        /// <summary>
        /// </summary>
        CmdTicketConfig = 239,

        /// <summary>
        /// </summary>
        CmdTcktGetHandpayBarcode = 240,

        /// <summary>
        /// </summary>
        CmdPtDeviceConfig = 241,

        /// <summary>
        /// </summary>
        CmdPtCountdownConfig = 242,

        /// <summary>
        /// </summary>
        CmdPtPlayerInfo = 243,

        /// <summary>
        /// </summary>
        CmdPtPlayerConfig = 244,

        /// <summary>
        /// </summary>
        CmdGetPtParams = 245,

        /// <summary>
        /// </summary>
        CmdPlayerTrackingConfig = 246,

        /// <summary>
        /// </summary>
        CmdGetPtBaseCountdown = 247,

        /// <summary>
        /// </summary>
        CmdProgressivePrize = 22,

        /// <summary>
        /// </summary>
        CmdProgRequest = 248,

        /// <summary>
        /// </summary>
        CmdProgInfo = 249,

        /// <summary>
        /// </summary>
        CmdProgUpdate = 114,

        /// <summary>
        /// </summary>
        CmdGameProgUpdate = 115,

        /// <summary>
        /// </summary>
        CmdProgCarryover = 121,

        /// <summary>
        /// </summary>
        CmdProgressiveWin = 187,

        /// <summary>
        /// </summary>
        CmdProgressiveWinResponse = 188,

        /// <summary>
        /// </summary>
        CmdGameReset = 21,

        /// <summary>
        /// </summary>
        CmdProgShown = 250,

        /// <summary>
        /// </summary>
        CmdProgHit = 251,

        /// <summary>
        /// </summary>
        CmdMsRequestScratchoffTicketsFromMs = 500,

        /// <summary>
        /// </summary>
        CmdScratchoffTicketsResponse = 501,

        /// <summary>
        /// </summary>
        CmdMsInitialized = 502,

        /// <summary>
        /// </summary>
        CmdScratchticketComplete = 503,

        /// <summary>
        /// </summary>
        CmdScratchsetWithdraw = 504,

        /// <summary>
        /// </summary>
        CmdRaceStart = 505,

        /// <summary>
        /// </summary>
        CmdRacePariReq = 506,

        /// <summary>
        /// </summary>
        CmdRacePari = 507,

        /// <summary>
        /// </summary>
        CmdRacepoolAdjustment = 508,

        /// <summary>
        /// </summary>
        CmdGameRecover = 509,

        /// <summary>
        /// </summary>
        CmdGameRecoverResponse = 510,

        /// <summary>
        /// </summary>
        CmdGameFinished = 1000,

        /// <summary>
        /// </summary>
        CmdGameComplete = 1001,

        /// <summary>
        /// </summary>
        CmdProgDisplayupdate = 1002
    }
}