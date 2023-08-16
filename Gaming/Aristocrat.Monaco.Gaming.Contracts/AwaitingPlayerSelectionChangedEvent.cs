namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The AwaitingPlayerSelectionChangedEvent is posted when the runtime updates the Awaiting Player Selection flag.
    /// </summary>
    [ProtoContract]
    public class AwaitingPlayerSelectionChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AwaitingPlayerSelectionChangedEvent" /> class.
        /// </summary>
        /// <param name="awaitingPlayerSelection">Awaiting Player Selection</param>
        public AwaitingPlayerSelectionChangedEvent(bool awaitingPlayerSelection)
        {
            AwaitingPlayerSelection = awaitingPlayerSelection;
        }

        /// <summary>
        /// Parameterless constructor used while deserializing
        /// </summary>
        public AwaitingPlayerSelectionChangedEvent()
        {
        }

        /// <summary>
        ///     Gets the AwaitingPlayerSelection parameter
        /// </summary>
        [ProtoMember(1)]
        public bool AwaitingPlayerSelection { get; }
    }
}