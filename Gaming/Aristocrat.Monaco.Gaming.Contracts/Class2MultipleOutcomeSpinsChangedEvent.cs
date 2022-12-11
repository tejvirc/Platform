namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The Class2MultipleOutcomeSpinsChangedEvent is posted when the runtime updates the Class2MultipleOutcomeSpins Flag.
    /// </summary>
    [ProtoContract]
    public class Class2MultipleOutcomeSpinsChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Class2MultipleOutcomeSpinsChangedEvent" /> class.
        /// </summary>
        /// <param name="triggered">Triggered.</param>
        public Class2MultipleOutcomeSpinsChangedEvent(bool triggered)
        {
            Triggered = triggered;
        }

        /// <summary>
        ///     Gets the Triggered parameter
        /// </summary>
        [ProtoMember(1)]
        public bool Triggered { get; }
    }
}