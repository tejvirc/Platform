namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using IdReader;
    using static System.FormattableString;

    /// <summary>(Not GDS defined) Card Reader device behavior.</summary>
    public class CardReaderBehavior : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CardReaderBehavior() : base(GdsConstants.ReportId.CardReaderBehaviorResponse) { }

        /// <summary>Gets or sets a value indicating which reader types are supported.</summary>
        public IdReaderTypes SupportedTypes { get; set; }

        /// <summary>Gets or sets a value indicating which reader type a non-physical reader reports.</summary>
        public IdReaderTypes VirtualReportedType{ get; set; }

        /// <summary>Gets or sets a value indicating which validation method is used.</summary>
        public IdValidationMethods ValidationMethod { get; set; }

        /// <summary>Gets or sets a value indicating whether the EGM controls the reader (vs. a host).</summary>
        public bool IsEgmControlled { get; set; }

        /// <summary>Gets or sets a value indicating whether the reader exists physically (vs. a figment).</summary>
        public bool IsPhysical { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [SupportedTypes={SupportedTypes}, VirtualReportedType={VirtualReportedType}, ValidationMethod={ValidationMethod}, IsEgmControlled={IsEgmControlled}, IsPhysical={IsPhysical}]");
        }
    }
}