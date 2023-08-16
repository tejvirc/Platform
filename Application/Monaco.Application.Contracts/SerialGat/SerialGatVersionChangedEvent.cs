﻿namespace Aristocrat.Monaco.Application.Contracts.SerialGat
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to notify that operator has changed serial GAT version.
    /// </summary>

    [ProtoContract]
    public class SerialGatVersionChangedEvent : BaseEvent
    {
        /// <summary>
        ///     The legacy GAT 3 protocol version
        /// </summary>
        public static readonly string LegacyGat3 = "LegacyGAT3";

        /// <summary>
        ///     The normal GAT 3.5 protocol version
        /// </summary>
        public static readonly string Gat35 = "GAT3.5";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="version">New GAT protocol version</param>
        public SerialGatVersionChangedEvent(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Parameterless constructor used while deserializing
        /// </summary>
        public SerialGatVersionChangedEvent()
        {
        }

        /// <summary>
        ///     New GAT protocol version
        /// </summary>
        [ProtoMember(3)]
        public string Version { get; }
    }
}