namespace Aristocrat.Monaco.Hardware.Serial.NoteAcceptor.ID003
{
    /// <summary>
    ///     All types of commands from Controller to Device.
    /// </summary>
    public enum Id003Command : byte
    {
        Unknown = 0,

        StatusRequest = 0x11,

        Ack = 0x50,

        Reset = 0x40,
        Stack1,
        Stack2,
        Return,
        Hold,
        Wait,

        ProgramSignature = 0xDC,
        EnableDisableDenomination = 0xC0,
        SecurityDenomination,
        CommunicationMode,
        InhibitAcceptor,
        Direction,
        OptionalFunction,
        BarcodeFunction,
        BarInhibit,

        EnableDisableDenominationStatus = 0x80,
        SecurityDenominationStatus,
        CommunicationModeStatus,
        InhibitAcceptorStatus,
        DirectionStatus,
        OptionalFunctionStatus,

        VersionRequest = 0x88,
        BootVersionRequest,
        CurrencyAssignRequest,
    }

    /// <summary>
    ///     Command subset: general commands
    /// </summary>
    public enum Id003GeneralCommand : byte
    {
        StatusRequest = Id003Command.StatusRequest,
        Ack = Id003Command.Ack,
    }

    /// <summary>
    ///     Command subset: operation commands
    /// </summary>
    public enum Id003OperationCommand : byte
    { 
        Reset = 0x40,
        Stack1,
        Stack2,
        Return,
        Hold,
        Wait,
        ProgramSignature = 0xDC,
    }

    /// <summary>
    ///     Command subset: Set commands
    /// </summary>
    public enum Id003SetCommand : byte
    {
        EnableDisableDenomination = 0xC0,
        SecurityDenomination,
        CommunicationMode,
        InhibitAcceptor,
        Direction,
        OptionalFunction,
        BarcodeFunction,
        BarInhibit,
    }

    /// <summary>
    ///     Command subset: get status of set commands
    /// </summary>
    public enum Id003SetStatusRequest : byte
    {
        EnableDisableDenominationStatus = 0x80,
        SecurityDenominationStatus,
        CommunicationModeStatus,
        InhibitAcceptorStatus,
        DirectionStatus,
        OptionalFunctionStatus,
        VersionRequest = 0x88,
        BootVersionRequest,
        CurrencyAssignRequest,
    }

    /// <summary>
    ///     All types of responses to Controller from Device.
    /// </summary>
    public enum Id003Status : byte
    {
        Unknown = 0,

        EnabledIdle = 0x11,
        Accepting,
        Escrow,
        Stacking,
        VendValid,
        Stacked,
        Rejecting,
        Returning,
        Holding,
        DisabledInhibited,
        Initializing,

        ProgramSignatureBusy = 0xDE,
        ProgramSignatureEnd,

        PowerUp = 0x40,
        PowerUpBillInAcceptor,
        PowerUpBillInStacker,

        StackerFull = 0x43,
        StackerOpen,
        JamInAcceptor,
        JamInStacker,
        Pause,
        Cheated,
        Failure,
        CommunicationError,

        //Enq = 0x05,
        InvalidCommand = 0x4B,
        Ack = 0x50,

        EnableDisableDenomination = 0xC0,
        SecurityDenomination,
        CommunicationMode,
        InhibitAcceptor,
        Direction,
        OptionalFunction,
        BarcodeFunction,
        BarcodeInhibit,

        EnableDisableDenominationStatus = 0x80,
        SecurityDenominationStatus,
        CommunicationModeStatus,
        InhibitAcceptorStatus,
        DirectionStatus,
        OptionalFunctionStatus,
        VersionRequest = 0x88,
        BootVersionRequest,
        CurrencyAssignRequest,
    }

    /// <summary>
    ///     Status subset: states
    /// </summary>
    public enum Id003StatusState : byte
    {
        EnabledIdle = 0x11,
        Accepting,
        Escrow,
        Stacking,
        VendValid,
        Stacked,
        Rejecting,
        Returning,
        Holding,
        DisabledInhibited,
        Initializing,

        ProgramSignatureBusy = 0xDE,
        ProgramSignatureEnd,

        PowerUp = 0x40,
        PowerUpBillInAcceptor,
        PowerUpBillInStacker,
    }

    /// <summary>
    ///     Status subset: errors
    /// </summary>
    public enum Id003StatusError : byte
    {
        StackerFull = 0x43,
        StackerOpen,
        JamInAcceptor,
        JamInStacker,
        Pause,
        Cheated,
        Failure,
        CommunicationError,
    }

    /// <summary>
    ///     Reasons to reject a note or ticket
    /// </summary>
    public enum Id003RejectCode : byte
    {
        InsertionError = 0x71,
        MagneticPatternError,
        IntrusionSensorDetected,
        DataAmplitudeError,
        FeedError,
        DenominationAssessmentError,
        PhotoPatternError,
        PhotoLevelError,
        Inhibited,
        Reserved,
        OperationError,
        BillInTransportError,
        LengthError,
        ColorPatternError,

        // Ticket-specific
        BarcodeFunctionNotSet = 0x91,
        BarcodeUnknown,
        BarcodeCharacterLengthError,
        BarcodeStartBitError,
        BarcodeStopBitError,
        BarcodeNotSet,
        BarcodeTicketLengthError = 0x9D,
    }
}
