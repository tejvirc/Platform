namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Provides information about the current poker hand
    /// </summary>
    public class PokerHandProvider : IHandProvider
    {
        private readonly HandInformation _handInformation = new HandInformation();
        private static readonly PokerHand Hand = new PokerHand();
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;

        public PokerHandProvider(IEventBus eventBus, IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public void UpdateDealtCards(List<Hand> cards)
        {
            Hand.ClearHand();
            Hand.Deal.Add(cards[0].Cards);
            InitialHand();
            UpdatePokerHandInformation();
        }

        /// <inheritdoc />
        public void UpdateDrawCards(List<Hand> cards)
        {
            _eventBus.Publish(new PlayerRequestedDrawEvent());
            Hand.Draw.Add(cards[0].Cards);
            DrawHand();
            UpdatePokerHandInformation();
        }

        /// <inheritdoc />
        public void UpdateHoldCards(List<Hand> cards)
        {
            Hand.Hold.Add(cards[0].Cards);
            MarkHeldCards();
            _eventBus.Publish(new CardsHeldEvent(CardsHeld));
            UpdatePokerHandInformation();
        }

        /// <inheritdoc />
        public IList<HoldStatus> CardsHeld => _handInformation.IsHeld.ToList();

        /// <inheritdoc />
        public IList<GameCard> CurrentHand => _handInformation.Cards.ToList();

        /// <inheritdoc />
        public bool FinalHand => _handInformation.FinalHand;

        private void UpdatePokerHandInformation()
        {
            // this property allows the backend to access the most recent information about the hand.
            // this is not required to be persisted.
            _propertiesManager.SetProperty(GamingConstants.PokerHandInformation, _handInformation);
        }

        private void InitialHand()
        {
            // copy new card data and clear held information
            _handInformation.Cards = Hand.Deal[0].Select(x => (GameCard)x).ToArray();
            _handInformation.IsHeld = new[] { HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld };
            _handInformation.FinalHand = false;
        }

        private void MarkHeldCards()
        {
            foreach (var h in Hand.Hold[0])
            {
                _handInformation.IsHeld[Array.FindIndex(
                    _handInformation.Cards,
                    i => i == (GameCard)h)] = HoldStatus.Held;
            }
        }

        private void DrawHand()
        {
            // update non-held cards
            var j = 0;
            var result = _handInformation.Cards.Select(
                (card, index) => (_handInformation.IsHeld[index] == HoldStatus.Held ? card : (GameCard)Hand.Draw[0][j++])).ToArray();

            _handInformation.Cards = result;
            _handInformation.FinalHand = true;
        }
    }

    /// <summary>
    ///     A data class that holds information about the current poker hand
    /// </summary>
    internal class PokerHand
    {
        /// <summary> Gets or sets the dealt cards </summary>
        public List<long[]> Deal { get; set; } = new List<long[]>();

        /// <summary> Gets or sets the held cards </summary>
        public List<long[]> Hold { get; set; } = new List<long[]>();

        /// <summary> Gets or sets the drawn cards </summary>
        public List<long[]> Draw { get; set; } = new List<long[]>();

        /// <summary>
        ///     Clears any information from prior hands
        /// </summary>
        public void ClearHand()
        {
            Deal.Clear();
            Hold.Clear();
            Draw.Clear();
        }
    }
}