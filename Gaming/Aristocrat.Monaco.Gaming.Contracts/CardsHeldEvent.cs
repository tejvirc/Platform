namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Sent by the PokerHandProvider when the poker game is finished.
    /// </summary>
    [ProtoContract]
    public class CardsHeldEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CardsHeldEvent" /> class
        /// </summary>
        /// <param name="cardsHeld">The information about the held cards</param>
        public CardsHeldEvent(IList<HoldStatus> cardsHeld)
        {
            CardsHeld = cardsHeld;
        }

        /// <summary>
        /// Parameterless constructor used while deserializing
        /// </summary>
        public CardsHeldEvent()
        {
        }

        /// <summary>
        ///     Gets or sets the held cards
        /// </summary>
        [ProtoMember(1)]
        public IList<HoldStatus> CardsHeld { get; set; }
    }
}