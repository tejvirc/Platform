namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents a Snapshot Meter
    /// </summary>
    [ProtoContract]
    public class SnapshotMeter
    {
        /// <summary>
        ///     Gets or sets the meter name
        /// </summary>
        [ProtoMember(1)]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the meter value
        /// </summary>
        [ProtoMember(2)]
        public long Value { get; set; }
    }
}