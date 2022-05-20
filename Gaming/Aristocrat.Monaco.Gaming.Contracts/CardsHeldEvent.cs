namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     Sent by the PokerHandProvider when the poker game is finished.
    /// </summary>
    [Serializable]
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
        ///     Gets or sets the held cards
        /// </summary>
        public IList<HoldStatus> CardsHeld { get; set; }
    }
}