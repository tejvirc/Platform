namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;

    /// <summary>Transfer status event type enumerations.</summary>
    public enum TransferStatusEventType
    {
        /// <summary>Indicates transfer status type none.</summary>
        None,

        /// <summary>Indicates transfer status type print.</summary>
        Print,

        /// <summary>Indicates transfer status type template.</summary>
        Template,

        /// <summary>Indicates transfer status type region.</summary>
        Region,

        /// <summary>Indicates transfer status type graphic.</summary>
        Graphic
    }

    /// <summary>Transfer status event code enumerations.</summary>
    public enum TransferStatusEventCode
    {
        /// <summary>Indicates transfer status code none.</summary>
        None,

        /// <summary>Indicates transfer status code syntax error.</summary>
        SyntaxError,

        /// <summary>Indicates transfer status code incorrect index.</summary>
        IncorrectIndex,

        /// <summary>Indicates transfer status code out of memory.</summary>
        OutOfMemory,

        /// <summary>Indicates transfer status code wrong Id range.</summary>
        WrongIdRange,

        /// <summary>Indicates transfer status code region overflow.</summary>
        RegionOverflow,

        /// <summary>Indicates transfer status code data type mismatch.</summary>
        DataTypeMismatch,

        /// <summary>Indicates transfer status code region truncation error.</summary>
        RegionTruncationError,

        /// <summary>Indicates transfer status code undefined font.</summary>
        UndefinedFont,

        /// <summary>Indicates transfer status code undefined graphic.</summary>
        UndefinedGraphic,

        /// <summary>Indicates transfer status code undefined barcode.</summary>
        UndefinedBarcode,

        /// <summary>Indicates transfer status code undefined line.</summary>
        UndefinedLine,

        /// <summary>Indicates transfer status code undefined box.</summary>
        UndefinedBox,

        /// <summary>Indicates transfer status code undefined justification.</summary>
        UndefinedJustification,

        /// <summary>Indicates transfer status code incorrect multiplier value.</summary>
        IncorrectMultiplierValue,

        /// <summary>Indicates transfer status code undefined region Id.</summary>
        UndefinedRegionId,

        /// <summary>Indicates transfer status code too many fields of interest.</summary>
        TooManyFieldsOfInterest,

        /// <summary>Indicates transfer status code already defined field of interest.</summary>
        AlreadyDefinedFieldOfInterest,

        /// <summary>Indicates transfer status code number of regions does not match template definition.</summary>
        NumberOfRegionsDoesNotMatchTemplateDefinition,

        /// <summary>Indicates transfer status code undefined dynamic graphic.</summary>
        UndefinedDynamicGraphics,

        /// <summary>Indicates transfer status code undefined template.</summary>
        UndefinedTemplate,

        /// <summary>Indicates transfer status code unsupported color feature.</summary>
        UnsupportedColorFeature,

        /// <summary>Indicates transfer status code file size too large.</summary>
        FileSizeTooLarge,

        /// <summary>Indicates transfer status code corrupted file.</summary>
        CorruptedFile,

        /// <summary>Indicates transfer status code unsupported graphics.</summary>
        UnsupportedGraphics,

        /// <summary>Indicates transfer status code graphic transfer setup undefined.</summary>
        GraphicTransferSetupUndefined
    }

    /// <summary>Definition of the print TransferStatusEvent class.</summary>
    [Serializable]
    public class TransferStatusEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferStatusEvent" /> class.
        /// </summary>
        public TransferStatusEvent()
        {
            TransferStatusId = string.Empty;
            TransferStatusType = (int)TransferStatusEventType.None;
            TransferStatusCode = (int)TransferStatusEventCode.None;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferStatusEvent" /> class.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        /// <param name="id">The region or template Id for this transfer status.</param>
        /// <param name="type">The transfer status type.</param>
        /// <param name="code">The transfer status code.</param>
        public TransferStatusEvent(int printerId, string id, int type, int code)
            : base(printerId)
        {
            TransferStatusId = id;
            TransferStatusType = type;
            TransferStatusCode = code;
        }

        /// <summary>Gets the transfer status Id.</summary>
        public string TransferStatusId { get; }

        /// <summary>Gets the transfer status type.</summary>
        public int TransferStatusType { get; }

        /// <summary>Gets the transfer status code.</summary>
        public int TransferStatusCode { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.PrinterText} {Resources.TransferStatusErrorText}";
        }
    }
}
