namespace Aristocrat.Monaco.Hardware.Serial.NoteAcceptor.EBDS
{
    using System;
    using System.Collections.Generic;
    using Protocols;

    /// <summary>
    ///     The EBDS Control byte
    /// </summary>
    [Flags]
    public enum EbdsControl : byte
    {
        Nack = 0x00,
        Ack = 0x01,
        StandardOmnibusCommand = 0x10,
        StandardOmnibusReply = 0x20,
        OmnibusWithBookmarkMode = 0x30,
        CalibrateRequest = 0x40,
        FlashDownload = 0x50,
        AuxiliaryCommand = 0x60,
        ExtendedMessageSet = 0x70,
        MessageType = 0x70
    }

    /// <summary>
    ///     The Omnibus reply device state
    /// </summary>
    [Flags]
    public enum EbdsDeviceState : byte
    {
        OutOfService = 0,
        Idling = 0x01,
        Accepting = 0x02,
        EscrowedState = 0x04,
        Stacking = 0x08,
        StackedEvent = 0x10,
        Returning = 0x20,
        ReturnedEvent = 0x40
    }

    /// <summary>
    ///     The Omnibus Denom support
    /// </summary>
    [Flags]
    public enum EbdsDenomSupport : byte
    {
        None = 0x00,
        Denom1 = 0x01,
        Denom2 = 0x02,
        Denom3 = 0x04,
        Denom4 = 0x08,
        Denom5 = 0x10,
        Denom6 = 0x20,
        Denom7 = 0x40,
        All = Denom1 | Denom2 | Denom3 | Denom4 | Denom5 | Denom6 | Denom7
    }

    /// <summary>
    ///     The Omnibus Device Status
    /// </summary>
    [Flags]
    public enum EbdsDeviceStatus : byte
    {
        Cheated = 0x01,
        Rejected = 0x02,
        Jammed = 0x04,
        StackerFull = 0x08,
        CassetteAttached = 0x10,
        Paused = 0x20,
        CalibrationInProgress = 0x40
    }

    /// <summary>
    ///     The Omnibus Exceptional Status
    /// </summary>
    [Flags]
    public enum OmnibusExceptionalStatus : byte
    {
        PowerUp = 0x01,
        InvalidCommand = 0x02,
        Failure = 0x04,
        TransportOpen = 0x40
    }

    /// <summary>
    ///     The Omnibus Miscellaneous State
    /// </summary>
    [Flags]
    public enum OmnibusMiscellaneousState : byte
    {
        Stalled = 0x01,
        FlashDownload = 0x02,
        //PreStack = 0x04, // Deprecated Do NOT use
        RawBarcode = 0x08,
        DeviceCapabilities = 0x10, // Always 0 for gaming devices
        Disabled = 0x20
    }

    /// <summary>
    ///     The Omnibus Configurations
    /// </summary>
    [Flags]
    public enum OmnibusConfigurations
    {
        NoPushMode = 0x01,
        BarcodeSupport = 0x02,
        PolicySettingA = 0x00,
        PolicySettingB = 0x04,
        PolicySettingC = 0x08,
        ExtendedNoteReporting = 0x10,
        ExtendedCouponReporting = 0x20,
        Default = PolicySettingA | ExtendedNoteReporting
    }

    /// <summary>
    ///     The Omnibus Operations
    /// </summary>
    [Flags]
    public enum OmnibusOperations
    {
        //SpecialInterruptMode = 0x01, // Deprecated Do NOT use
        //HighSecurity = 0x02, // Deprecated Do NOT use
        OneWayOrientationControl = 0x00,
        TwoWayOrientationControl = 0x04,
        FourWayOrientationControl = 0x0C,
        EscrowMode = 0x10,
        DocumentStackCommand = 0x20,
        DocumentReturnCommand = 0x40,
        Default = FourWayOrientationControl | EscrowMode
    }

    /// <summary>
    ///     Commands when sending a message with the EBDS control Auxiliary set
    /// </summary>
    public enum AuxiliaryCommands : byte
    {
        QuerySoftwareCrc = 0x00,
        QueryCashboxTotal = 0x01,
        QueryDeviceReset = 0x02,
        ClearCashboxTotal = 0x03,
        QueryAcceptorType = 0x04,
        QuerySerialNumber = 0x05,
        QueryBootPartNumber = 0x06,
        QueryApplicationPartNumber = 0x07,
        QueryVariantName = 0x08,
        QueryVariantPartNumber = 0x09,
        QueryAuditLifeTimeTotals = 0x0A,
        QueryAuditQpMeasures = 0x0B,
        QueryAuditPerformanceMeasures = 0x0C,
        QueryDeviceCapabilities = 0x0D,
        QueryApplicationId = 0x0E,
        QueryVariantId = 0x0F,
        QueryBnfStatus = 0x10,
        SetBezel = 0x11,
        QueryAuditLifeTimeTotalExtended = 0x12,
        QueryAuditQpMeasuresExtended = 0x13,
        QueryAuditPerformanceMeasuresExtended = 0x14,
        QueryAssetNumber = 0x15,
        QueryAuditTotalDocumentsReportingStructure = 0x16,
        QueryTotalDocumentsRecognized = 0x17,
        QueryTotalDocumentsValidated = 0x18,
        QueryTotalDocumentsStacked = 0x19,
        DiagnosticsSelfTest = 0x1A,
        QueryDiagnosticsSensorData = 0x1D,
        QueryHardwareStatus = 0x23,
        SetCustomerConfigurationOption = 0x25,
        QueryCustomerConfigurationOption = 0x26,
        SoftReset = 0x7F
    }

    /// <summary>
    ///     Commands when sending a message with the EBDS control Extended Message set
    /// </summary>
    public enum ExtendedCommands : byte
    {
        BarcodeReply = 0x01,
        NoteSpecificationMessage = 0x02,
        SetNoteInhibits = 0x03,
        SetEscrowTimeout = 0x04,
        SetAssetNumber = 0x05,
        //QueryValueTable = 0x06, // Deprecated Do NOT use
        NoteRetrieved = 0x0B,
        Sha1Request = 0x0C,
        AdvancedBookmarkMode = 0xD,
        CashboxCleanliness = 0x10,
        Request16BitCrcCalculation = 0x11,
        Request32BitCrcCalculation = 0x12,
        RequestRecyclerNoteValueEnables = 0x13,
        SetRecyclerNoteValueEnables = 0x14,
        RecyclerOmnibusCommand = 0x15,
        DispenseBills = 0x16,
        CancelDispense = 0x17,
        FloatBills = 0x18,
        LastCashMovementSummaryReport = 0x19,
        MissingNoteReport = 0x1A,
        RequestPhysicalRecyclerStatus = 0x1B,
        RfidDataRequest = 0x1C,
        ClearAuditDataRequest = 0x1D,
        EscrowSessionSummaryReport = 0x20,
        TransferNotes = 0x21,
        RequestPhysicalRecyclerOrderContent = 0x22,
        PerformRAndSCommand = 0x23,
        DispenseClassifiedBills = 0x24,
        FloatClassifiedBills = 0x25,
        ClassifiedBanknotesEscrowSessionSummaryReport = 0x26,
        RequestPhysicalClassifiedBanknotesOrderedContent = 0x27,
        SetRecyclerNoteInhibits = 0x28
    }

    /// <summary>
    ///     The operator type for the self command
    /// </summary>
    public enum SelfTestOperatorType : byte
    {
        StartTest = 0x01,
        QueryResults = 0x02
    }

    /// <summary>
    ///     The self test level to perform
    /// </summary>
    public enum SelfTestLevel : byte
    {
        SensorTest = 0x01,
        MmiAndMotorsTest = 0x02
    }

    /// <summary>
    ///     The self test transport motor results
    /// </summary>
    public enum SelfTestTransportMotorResults : byte
    {
        Pass = 0,
        FailedTransportTach = 0x01,
        FailedBnfMotor = 0x02,
        ResultsNotReady = 0x7F
    }

    /// <summary>
    ///     The self test stacker motor results
    /// </summary>
    public enum SelfTestStackerMotorResults : byte
    {
        Pass = 0,
        FailedOffHome = 0x01,
        ResultsNotReady = 0x7F
    }

    /// <summary>
    ///     Constants used by EBDS protocol
    /// </summary>
    public static class EbdsProtocolConstants
    {
        private const byte Stx = 0x02;
        private const byte Etx = 0x03;

        /// <summary>
        ///     The expected length when receiving a standard Omnibus Reply
        /// </summary>
        public const int StandardOmnibusResponseLength = 7;

        /// <summary>
        ///     The index for the control command
        /// </summary>
        public const int EbdsControlIndex = 0;

        /// <summary>
        ///     The number of digits for the CRC being returned
        /// </summary>
        public const int CrcDigitCount = 4;

        /// <summary>
        ///     The index for the start of the CRC
        /// </summary>
        public const int CrcDigitStartIndex = 1;

        /// <summary>
        ///     The expected length for the CRC response
        /// </summary>
        public const int CrcResponseLength = 7;

        /// <summary>
        ///     Format for all EBDS messages:
        ///     [STX][Length][CTL]{[Data0][Data1]...}[ETX][Checksum]
        ///
        ///     Where:
        ///     STX is 0x02
        ///     Length is the total message length
        ///     CTL is a mix of the ACK/NAK field, a device descriptor and a message type identifier.
        ///     Data0, Data1, ... are additional data bytes as required (or not) by the command/status
        ///     Checksum is one byte where it is a XOR checksum of bytes 1 through Len-3.
        /// </summary>
        public static readonly MessageTemplate<XorChecksumEngine> DefaultMessageTemplate =
            new MessageTemplate<XorChecksumEngine>(new List<MessageTemplateElement>
                {
                    new MessageTemplateElement { ElementType = MessageTemplateElementType.Constant, Length = 1, IncludedInCrc = false, Value = new[] { Stx } },
                    new MessageTemplateElement { ElementType = MessageTemplateElementType.FullLength, Length = 1, IncludedInCrc = true },
                    new MessageTemplateElement { ElementType = MessageTemplateElementType.VariableData, IncludedInCrc = true },
                    new MessageTemplateElement { ElementType = MessageTemplateElementType.Constant, Length = 1, IncludedInCrc = false, Value = new[] { Etx } },
                    new MessageTemplateElement { ElementType = MessageTemplateElementType.Crc, Length = 1 }
                },
                0);
    }
}