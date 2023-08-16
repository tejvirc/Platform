namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;
    using System.Runtime.Serialization;
    using ProtoBuf;

    /// <summary>
    ///     Represents an in-game meter
    /// </summary>
    [ProtoContract]
    public class InGameMeter
    {
        /// <summary>
        ///     Gets or sets the meter name
        /// </summary>
        [ProtoMember(1)]
        public string MeterName { get; set; }

        /// <summary>
        ///     Gets or sets the meter value
        /// </summary>
        [ProtoMember(2)]
        public long Value { get; set; }
    }
}
