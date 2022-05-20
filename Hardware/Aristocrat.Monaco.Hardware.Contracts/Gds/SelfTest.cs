namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) A self test request.</summary>
    [Serializable]
    public class SelfTest : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public SelfTest() : base(GdsConstants.ReportId.SelfTest) { }

        /// <summary>Gets or sets the Clear-NVM flag.</summary>
        /// <value>The Clear-NVM flag.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public int Nvm { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Nvm={Nvm}]");
        }
    }
}
