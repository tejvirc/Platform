namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;

    /// <summary>
    ///     Represents a Snapshot Meter
    /// </summary>
    [Serializable]
    public class SnapshotMeter
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