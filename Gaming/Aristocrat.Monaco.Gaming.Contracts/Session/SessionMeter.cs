namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents a session meter
    /// </summary>
    [ProtoContract]
    public class SessionMeter
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
