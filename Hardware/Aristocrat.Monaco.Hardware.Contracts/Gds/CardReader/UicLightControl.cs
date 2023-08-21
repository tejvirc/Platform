namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a UIC-specific light control command.</summary>
    [CLSCompliant(false)]
    [Serializable]
    public class UicLightControl : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public UicLightControl() : base(GdsConstants.ReportId.CardReaderLightControl) { }

        /// <summary>Gets or sets the red LED intensity.</summary>
        /// <value>The red LED intensity.</value>
        [FieldOrder(0)]
        [FieldEndianness(Endianness.Little)]
        public ushort Red { get; set; }

        /// <summary>Gets or sets the green LED intensity.</summary>
        /// <value>The green LED intensity.</value>
        [FieldOrder(1)]
        [FieldEndianness(Endianness.Little)]
        public ushort Green { get; set; }

        /// <summary>Gets or sets the blue LED intensity.</summary>
        /// <value>The blue.</value>
        [FieldOrder(2)]
        [FieldEndianness(Endianness.Little)]
        public ushort Blue { get; set; }

        /// <summary>Gets or sets the red LED intensity.</summary>
        /// <value>The red LED intensity.</value>
        [FieldOrder(3)]
        [FieldEndianness(Endianness.Little)]
        public ushort Red2 { get; set; }

        /// <summary>Gets or sets the green LED intensity.</summary>
        /// <value>The green LED intensity.</value>
        [FieldOrder(4)]
        [FieldEndianness(Endianness.Little)]
        public ushort Green2 { get; set; }

        /// <summary>Gets or sets the blue LED intensity.</summary>
        /// <value>The blue.</value>
        [FieldOrder(5)]
        [FieldEndianness(Endianness.Little)]
        public ushort Blue2 { get; set; }

        /// <summary>Gets or sets the timer interval the LED should be turned on.</summary>
        /// <value>The timer interval.</value>
        /// <remarks>0 sets the selected LEDs to be permanently turned on until further instructions are received. Non-zero values are a
        /// Multiplier of 100ms. LEDs selected alternate between ON and OFF states every(LED Timer * 100) milliseconds</remarks>
        [FieldOrder(6)]
        public byte TimerInterval { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Red={Red}, Green={Green}, Blue={Blue}, Red2={Red2}, Green2={Green2}, Blue2={Blue2}, TimerInterval={TimerInterval}]");
        }
    }
}
