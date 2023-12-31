﻿namespace Aristocrat.Monaco.Bingo.Common
{
    /// <summary>
    ///     Events that get reported to the bingo server
    /// </summary>
    public enum ReportableEvent
    {
        AllEvents = 0,
        MainDoorOpened,
        MainDoorClosed,
        LogicDoorOpened,
        LogicDoorClosed,
        CashDoorOpened,
        CashDoorClosed,
        SetUpModeEntered,
        SetUpModeExited,
        NvRamCleared,
        CashIn,
        TicketOut,
        ResetPeriod,
        LimitedPlay,
        MeterBounds,
        InvalidCredit,
        Operator,
        MachineDisabled,
        CashDrop,
        StackerInserted,
        StackerRemoved,
        Online,
        Disabled,
        Enabled,
        TicketIn,
        CardInserted,
        CardRemoved,
        CardReaderError,
        HandpayKeyOff, // Unused
        BillAcceptorError,
        BillAcceptorErrorClear,
        PrinterError,
        PrinterErrorClear,
        PrinterPaperLow,
        PrinterPaperLowClear,
        PrinterPaperOut,
        PrinterPaperOutClear,
        CardReaderErrorClear,
        ReelError1,   // reel 1 error that's not a re-sync error
        ReelError2,   // reel 2 error that's not a re-sync error
        ReelError3,   // reel 3 error that's not a re-sync error
        ReelError4,   // reel 4 error that's not a re-sync error
        ReelError5,   // reel 5 error that's not a re-sync error
        ReelError6,   // reel 6 error that's not a re-sync error
        ReelError7,   // reel error that's not a re-sync error, reel number not provided
        ReelError8,   // reel 1 re-sync error
        ReelError9,   // reel 2 re-sync error
        ReelError10,  // reel 3 re-sync error
        ReelError11,  // reel 4 re-sync error
        ReelError12,  // reel 5 re-sync error
        ReelError13,  // reel 6 re-sync error
        ReelError14,  // reel re-sync error, reel number not provided
        ReelError15,  // not used. Just a place holder.
        ReelIndex,    // an indexing error occurred during a spin
        ReelHome,     // failed to send reel home
        ReelNeedHome, // reels require homing
        ReelNeedFault, // the controller requires the fault location
        ReelWatchdog, // the watchdog timer timed out and reset the controller
        ReelBrownOut, // a reel brownout detected and reset
        ReelFeedback, // a reel tamper occurred
        ReelSpin,     // a reel spin failed
        EPointTimeout,
        EPointInvalidCommand,
        EPointInvalidData,
        EPointNoFirmware,
        EPointReelsDisabled,
        EPointCommandFail,
        EPointOther,
        Offline,
        ResetDuringPayout,
        PowerUp,
        NewOnFloor,
        RemovedFromFloor,
        VoucherIssuedJackpot, // "Jackpot" 
        HandpayKeyedOffJackpot = VoucherIssuedJackpot, // "Jackpot" 
        HandpayKeyedOffCancelCredits, // "Cancel Credit"
        CardReaderMessageFramingError,
        CardReaderHardwareError,
        CardReaderSoftwareError,
        CardReaderCommunicationError,
        CardReaderConfigurationError,
        PrinterMediaLoaded,
        PrinterMediaMissingIndexMark,
        PrinterMediaJam,
        PrinterConfigurationError,
        PrinterSoftwareError,
        PrinterCommunicationError,
        PrinterMessageFramingError,
        PrinterHardwareFailure,
        BillAcceptorStackerIsFull,
        BillAcceptorStackerJammed,
        BillAcceptorHardwareFailure,
        BillAcceptorDeviceFlashing,
        BillAcceptorCommunicationsError,
        BillAcceptorCheatDetected,
        BillAcceptorSoftwareError,
        BillAcceptorMessageFramingError,
        BillAcceptorConfigurationError,
        BillAcceptorDocumentRejected,
        BillAcceptorDocumentReturned,
        CashOutButtonPressed,
        VoucherIssueTimeout,
        VoucherRedeemTimeout,
        RebootDuringCashout,
        NvRamBatteryLow,
        TransferInComplete,
        TransferOutComplete,
        TransferInRefusedByEgm,
        TransferOutRefusedByEgm,
        PartialTransferInComplete,
        PartialTransferOutComplete,
        TransferInFailed,
        TransferOutFailed,
        TransferInTimeout,
        TransferOutTimeout,
        NatEnabled,
        NatDisabled,
        TransferLock,
        TransferUnlock,
        BonusWinAwarded,
        BonusLargeWinAwarded,
        BonusWinRefused,
        BonusingEnabled,
        BonusingDisabled,
        BonusLock,
        BonusUnlock,
        ProgressiveHit,
        ProgressiveWinAwarded,
        ProgressiveLargeWinAwarded,
        ProgressiveGameEnabled,
        ProgressiveGameDisabled,
        InvalidPaytablePacketSize,
        IncompleteComplianceConfigurationDataSection,
        IncompleteMessageConfigurationDataSection,
        UnsupportedJackpotDeterminationStrategy,
        IncompleteSystemConfigurationDataSection,
        UnsupportedGameTypeConfigured,
        NonVideoMultiGameNotSupported,
        SetTimeZoneFailed,
        InvalidLanguageCode,
        InvalidCountryCode,
        AttractGameDoesNotExist,
        PowerDownDuringPlay,
        NvRamError,
        BillAcceptorStackerFullClear,
        ReelErrorClear,
        PinEntered,
        AuxDoorOpened,
        AuxDoorClosed,
        PizzaBoxDoorOpened,
        PizzaBoxDoorClosed,
        CallAttendantButtonActivated,
        CallAttendantButtonDeactivated,
        CashoutBonus,
        CashoutProgressive,
        HandpayKeyedOffCashoutExternalBonus,
        CashInExceedsVoucherInLimit,
        BellyDoorOpened,
        BellyDoorClosed,
        BaseDoorOpened,
        BaseDoorClosed,
        RamInsufficient,
        LcdDoorOpened,
        LcdDoorClosed,
        StackerDoorOpened,
        StackerDoorClosed,
        SpinFxReelMoveTilt,
        SpinFxSpinningTilt,
        SpinFxCommsLinkDown,
        SpinFxCommsLinkUp,
        StepperProtoCommReady,
        StepperProtoCommError,
        StepperProtoDeviceDisconnect,
        StepperProtoCrcMismatch,
        StepperProtoFileUploadError,
        StepperProtoIncompatibleFirmware,
        StepperProtoControllerError,
        StepperSoftLockup,
        StepperClear,
        StepperInitializing
    }
}
