﻿namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     The SAS General exception codes from Appendix A Table A-1 of
    ///     the SAS 6.03 Protocol Document
    /// </summary>
    public enum GeneralExceptionCode
    {
        None = 0x00,
        SlotDoorWasOpened = 0x11,
        SlotDoorWasClosed,
        DropDoorWasOpened,
        DropDoorWasClosed,
        CardCageWasOpened,
        CardCageWasClosed,
        EgmPowerApplied,
        EgmPowerLost,
        CashBoxDoorWasOpened,
        CashBoxDoorWasClosed,
        CashBoxWasRemoved,
        CashBoxWasInstalled,
        BellyDoorWasOpened,
        BellyDoorWasClosed,
        UserInactivity,

        GeneralTilt = 0x20,
        CoinInTilt,
        CoinOutTilt,
        HopperEmptyDetected,
        ExtraCoinPaid,
        DiverterMalfunction,
        CashBoxFullDetected = 0x27,
        BillJam,
        BillAcceptorHardwareFailure,
        ReverseBillDetected,
        BillRejected,
        CounterfeitBillDetected,
        ReverseCoinDetected,
        CashBoxNearFullDetected,
        BillAcceptorVersionChanged,

        NvRamErrorDataRecovered = 0x31,
        NvRamErrorNoDataRecovered,
        NvRamErrorBadDevice,
        EePromDataError,
        EePromBadDeviceError,
        EePromErrorDifferentChecksum,
        EePromErrorBadChecksum,
        PartitionedEPromErrorVersionChanged,
        PartitionedEPromErrorBadChecksum,
        MemoryErrorReset,
        LowBackupBatteryDetected,
        OperatorChangedOptions,
        CashOutTicketPrinted,
        HandPayValidated,
        ValidationIdNotConfigured,
        ReelTilt,
        Reel1Tilt,
        Reel2Tilt,
        Reel3Tilt,
        Reel4Tilt,
        Reel5Tilt,
        ReelMechanismDisconnected,
        BillAccepted1,
        BillAccepted5,
        BillAccepted10,
        BillAccepted20,
        BillAccepted50,
        BillAccepted100,
        BillAccepted2,
        BillAccepted500,
        BillAccepted,
        BillAccepted200,

        HandPayIsPending = 0x51,
        HandPayWasReset,
        NoProgressiveInformationHasBeenReceivedFor5Seconds,
        ProgressiveWin,
        PlayerCanceledTheHandPayRequest,
        SasProgressiveLevelHit,
        SystemValidationRequest,

        NonSasProgressiveLevelHit = 0x59,
        JackpotHandpayKeyedOffToMachinePay,
        TipAwarded = 0x5F,

        PrinterCommunicationError = 0x60,
        PrinterPaperOutError,

        CashOutButtonPressed = 0x66,
        TicketHasBeenInserted,
        TicketTransferComplete,
        AftTransferComplete,
        AftRequestForHostCashOut,
        AftRequestForHostToCashOutWin,
        AftRequestToRegister,
        AftRegistrationAcknowledged,
        AftRegistrationCanceled,
        GameLocked,
        ExceptionBufferOverflow = 0x70,
        ChangeLampOn,
        ChangeLampOff,
        PrinterPaperLow = 0x74,
        PrinterPowerOff,
        PrinterPowerOn,
        ReplacePrinterRibbon,
        PrinterCarriageJam,
        CoinInLockoutMalfunction,
        GamingMachineSoftMetersReset,
        BillValidatorPeriodMetersReset,
        LegacyBonusPayAwarded,

        GameHasStarted = 0x7E,
        GameHasEnded,
        HopperFullDetected,
        HopperLevelLowDetected,
        AttendantMenuEntered,
        AttendantMenuExited,
        OperatorMenuEntered,
        OperatorMenuExited,
        GamingMachineOutOfServiceByOperator,
        PlayerHasRequestedDrawCards,
        ReelNHasStopped,
        CreditWagered,
        GameRecallEntryHasBeenDisplayed,
        CardHeldOrNotHeld,
        GameSelected,

        ComponentListChanged = 0x8E,
        AuthenticationComplete,

        PowerOffCardCageAccess = 0x98,
        PowerOffSlotDoorAccess,
        PowerOffCashBoxDoorAccess,
        PowerOffDropDoorAccess,
        SessionStart = 0x9E,
        SessionEnd,
        MeterChangePending,
        MeterChangeCanceled,
        EnabledGamesDenomsChanged
    }
}