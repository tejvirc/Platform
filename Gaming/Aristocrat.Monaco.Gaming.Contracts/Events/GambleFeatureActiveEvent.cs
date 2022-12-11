namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The GambleFeatureEnabledEvent is posted when the runtime updates the GambleFeatureActive flag.
    /// </summary>
    [ProtoContract]
    public class GambleFeatureActiveEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GambleFeatureActiveEvent" /> class.
        /// </summary>
        /// <param name="active">Gamble Button is Active on Game's UPI</param>
        public GambleFeatureActiveEvent(bool active)
        {
            Active = active;
        }

        /// <summary>
        ///     True if Gamble Button is Enabled on Game's UPI
        /// </summary>
        [ProtoMember(1)]
        public bool Active { get; }
    }
}