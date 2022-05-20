namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) an extended light control command.</summary>
    [CLSCompliant(false)]
    [Serializable]
    public class ExtendedLightControl : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public ExtendedLightControl() : base(GdsConstants.ReportId.CardReaderExtendedLightControl) { }

        /// <summary>Gets or sets the red LED intensity.</summary>
        /// <value>The red LED intensity.</value>
        [FieldOrder(0)]
        public byte Red { get; set; }

        /// <summary>Gets or sets the green LED intensity.</summary>
        /// <value>The green LED intensity.</value>
        [FieldOrder(1)]
        public byte Green { get; set; }

        /// <summary>Gets or sets the blue LED intensity.</summary>
        /// <value>The blue.</value>
        [FieldOrder(2)]
        public byte Blue{ get; set; }

        /// <summary>Gets or sets the timer interval the LED should be turned on.</summary>
        /// <value>The timer interval.</value>
        /// <remarks>0 sets the selected LEDs to be permanently turned on until further instructions are received. Non-zero values are a
        /// Multiplier of 10ms. LEDs selected alternate between ON and OFF states every(LED Timer * 10) milliseconds.</remarks>
        [FieldOrder(3)]
        [FieldEndianness(Endianness.Little)]
        public ushort TimerInterval { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Red={Red}] [Green={Green}] [Blue={Blue}] [TimerInterval={TimerInterval}]");
        }
    }
}