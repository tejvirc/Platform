namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a light control command.</summary>
    [Serializable]
    public class LightControl : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public LightControl() : base(GdsConstants.ReportId.CardReaderLightControl) { }

        [FieldOrder(0)]
        [FieldBitLength(1)]
        internal bool Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the red LED should be turned on.</summary>
        /// <value>True if red, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Red { get; set; }

        /// <summary>Gets or sets a value indicating whether the green LED should be turned on.</summary>
        /// <value>True if green, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool Green { get; set; }

        /// <summary>Gets or sets a value indicating whether the yellow LED should be turned on.</summary>
        /// <value>True if yellow, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool Yellow { get; set; }

        [FieldOrder(4)]
        [FieldBitLength(4)]
        internal byte Reserved2 { get; set; }

        /// <summary>Gets or sets the timer interval the LED should be turned on.</summary>
        /// <value>The timer interval.</value>
        /// <remarks>0 sets the selected LEDs to be permanently turned on until further instructions are received. Non-zero values are a
        /// Multiplier of 100ms. LEDs selected alternate between ON and OFF states every(LED Timer * 100) milliseconds</remarks>
        [FieldOrder(4)]
        public byte TimerInterval { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Red={Red}, Green={Green}, Yellow={Yellow}, TimerInterval={TimerInterval}]");
        }
    }
}