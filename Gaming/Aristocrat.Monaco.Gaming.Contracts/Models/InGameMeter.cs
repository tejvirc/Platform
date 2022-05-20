namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;

    /// <summary>
    ///     Represents an in-game meter
    /// </summary>
    [Serializable]
    public class InGameMeter
    {
        /// <summary>
        ///     Gets or sets the meter name
        /// </summary>
        public string MeterName { get; set; }

        /// <summary>
        ///     Gets or sets the meter value
        /// </summary>
        public long Value { get; set; }
    }
}
