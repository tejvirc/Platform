﻿namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     List of long poll commands
    /// </summary>
    public enum LongPoll
    {
        None = 0,
        Shutdown = 0x01,
        Startup,
        SoundOff,
        SoundOn,
        GameSoundsDisable,
        EnableBillAcceptor,
        DisableBillAcceptor,
        ConfigureBillDenominations,
        EnableDisableGameN,
        EnterMaintenanceMode,
        ExitMaintenanceMode,
        EnableDisableRealTimeEventReporting = 0x0E,
        SendMeters10Thru15,
        SendCanceledCreditsMeter,
        SendCoinInMeter,
        SendCoinOutMeter,
        SendDropMeter,
        SendJackpotMeter,
        SendGamesPlayedMeter,
        SendGamesWonMeter,
        SendGamesLostMeter,
        SendGamesSincePowerUpLastDoorMeter,
        SendMeters11Thru15,
        SendCurrentCredits,
        SendHandpayInformation,
        SendMeters,
        SendBillCountMeters = 0x1E,
        SendMachineIdAndInformation,
        SendTotalBillInValueMeter,
        RomSignatureVerification,
        SendTrueCoinIn = 0x2A,
        SendTrueCoinOut,
        SendCurrentHopperLevel,
        SendTotalHandPaidCanceledCredits,
        DelayGame,
        SendSelectedMetersForGameN,
        SendOneDollarInMeter = 0x31,
        SendTwoDollarInMeter,
        SendFiveDollarInMeter,
        SendTenDollarInMeter,
        SendTwentyDollarInMeter,
        SendFiftyDollarInMeter,
        SendOneHundredDollarInMeter,
        SendFiveHundredDollarInMeter,
        SendOneThousandDollarInMeter,
        SendTwoHundredDollarInMeter,
        SendTwentyFiveDollarInMeter,
        SendTwoThousandDollarInMeter,
        SendCashOutTicketInformation,
        SendTwoThousandFiveHundredDollarInMeter,
        SendFiveThousandDollarInMeter,
        SendTenThousandDollarInMeter,
        SendTwentyThousandDollarInMeter,
        SendTwentyFiveThousandDollarInMeter,
        SendFiftyThousandDollarInMeter,
        SendOneHundredThousandDollarInMeter,
        SendTwoHundredFiftyDollarInMeter,
        SendCreditAmountOfBillsAccepted,
        SendCoinAcceptedFromExternalAcceptor,
        SendLastBillInformation,
        SendNumberOfBillsInStacker,
        SendCreditAmountOfBillsInStacker,
        SetSecureEnhancedValidationId = 0x4C,
        SendEnhancedValidationInformation,
        SendCurrentHopperStatus = 0x4F,
        SendValidationMeters,
        SendNumberOfGames,
        SendGameNMeters,
        SendGameNConfiguration,
        SendSasVersionAndGameSerial,
        SendSelectedGameNumber,
        SendEnabledGameNumbers,
        SendPendingCashoutInformation,
        ReceiveValidationNumber,
        SendEnabledCurrencyCodes,
        SendSupportedBills,
        SendBillMeters,
        ForeignBillReportingMode,
        SendNonSasProgressiveWinData,
        SendConfiguredProgressiveControllers,
        SendProgressiveBroadcastValues,
        SingleLevelProgressiveBroadcastValue = 0x80,
        MultipleLevelProgressiveBroadcastValues = 0x86,
        SendAuthenticationInformation = 0x6E,
        SendExtendedMetersForGameN,
        SendTicketValidationData,
        RedeemTicket,
        AftTransferFunds,
        AftRegisterGamingMachine,
        AftGameLockAndStatusRequest,
        SetAftReceiptData,
        SetCustomAftTicketData,
        SendProgressiveAccountingData,
        ExtendedProgressiveBroadcast = 0x7A,
        ExtendedValidationStatus,
        SetExtendedTicketData,
        SetTicketData,
        SetCurrentDateTime,
        ReceiveDateTime,
        ReceiveProgressiveAmount,
        SendCumulativeProgressiveWins = 0x83,
        SendProgressiveWinAmount,
        SendSasProgressiveWinAmount,
        ReceiveMultipleProgressiveLevels,
        SendMultipleSasProgressiveWinAmounts,
        InitiateLegacyBonusPay = 0x8A,
        InitiateMultipliedJackpotMode,
        EnterExitTournamentMode,
        SendCardInformation = 0x8E,
        SendPhysicalReelStopInformation,
        SendLegacyBonusWinAmount,
        RemoteHandpayReset = 0x94,
        SendTournamentGamesPlayed,
        SendTournamentGamesWon,
        SendTournamentCreditsWagered,
        SendTournamentCreditsWon,
        SendMeters95Thru98,
        SendLegacyBonusMeters,
        SendEnabledFeatures = 0xA0,
        SendCashOutLimit = 0xA4,
        EnableJackpotHandpayResetMethod = 0xA8,
        EnableDisableGameAutoRebet = 0xAA,
        SendExtendedMetersForGameNAlternate = 0xAF,
        MultiDenominationPreamble,
        SendCurrentPlayerDenominations,
        SendEnabledPlayerDenominations,
        SendTokenDenomination,
        SendWagerCategoryInformation,
        SendExtendedGameNInformation,
        MeterCollectStatus,
        SetMachineNumbers,
        RealTimeEventResponseToLongPoll = 0xFF,

        #region EftCommands

        // (From section 8.EFT of the SAS v5.02 document)
        EftSendCumulativeMeters = 0x1D,
        EftSendCurrentPromotionalCredits = 0x27,
        EftSendTransferLogs = 0x28,
        EftTransferPromotionalCreditsToMachine = 0x63,
        EftTransferCashAndNonCashableCreditsToHost = 0x64,
        EftSendLastCashOutCreditAmount = 0x66,
        EftTransferCashableCreditsToMachine = 0x69,
        EftSendAvailableEftTransfers = 0x6A,
        EftTransferPromotionalCreditsToHost = 0x6B,

        #endregion
    }

    public static class LongPollExtensions
    {
        public static bool IsEftTransactionLongPoll(this LongPoll longPoll)
        {
            return longPoll == LongPoll.EftTransferPromotionalCreditsToMachine ||
                   longPoll == LongPoll.EftTransferCashableCreditsToMachine ||
                   longPoll == LongPoll.EftTransferCashAndNonCashableCreditsToHost ||
                   longPoll == LongPoll.EftTransferPromotionalCreditsToHost ||
                   longPoll == LongPoll.EftSendLastCashOutCreditAmount;
        }
    }
}