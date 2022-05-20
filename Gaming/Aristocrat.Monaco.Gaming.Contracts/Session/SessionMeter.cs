namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using System;

    /// <summary>
    ///     Represents a session meter
    /// </summary>
    [Serializable]
    public class SessionMeter
    {
        /// <summary>
        ///     Gets or sets the meter name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the meter value
        /// </summary>
        public long Value { get; set; }
    }
}
