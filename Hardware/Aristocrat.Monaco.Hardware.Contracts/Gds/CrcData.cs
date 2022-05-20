namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) A CRC data.</summary>
    [Serializable]
    public class CrcData : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CrcData() : base(GdsConstants.ReportId.CrcData) { }

        /// <summary>Gets or sets the result.</summary>
        /// <value>The result.</value>
        [FieldOrder(0)]
        [FieldEndianness(Endianness.Little)]
        public int Result { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Result={Result}]");
        }
    }
}
