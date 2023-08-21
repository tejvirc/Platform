namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) an error data.</summary>
    [Serializable]
    public class ErrorData : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public ErrorData() : base(GdsConstants.ReportId.CardReaderErrorData) { }

        /// <summary>Gets or sets the error code.</summary>
        /// <value>The error code.</value>
        [FieldOrder(0)]
        public byte ErrorCode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [ErrorCode={ErrorCode}]");
        }
    }
}