namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) A CRC request.</summary>
    [CLSCompliant(false)]
    [Serializable]
    public class CrcRequest : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CrcRequest() : base(GdsConstants.ReportId.CalculateCrc) { }

        /// <summary>Gets or sets the seed.</summary>
        /// <value>The seed.</value>
        [FieldOrder(0)]
        [FieldEndianness(Endianness.Little)]
        public uint Seed { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Seed={Seed}]");
        }
    }
}
