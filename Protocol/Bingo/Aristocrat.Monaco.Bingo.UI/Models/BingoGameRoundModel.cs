namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     Data to display within <see cref="ViewModels.OperatorMenu.BingoGameHistoryDetailsViewModel"/>;
    ///     contains the ball call, played cards, and winning patterns.
    /// </summary>
    public class BingoGameRoundModel : BaseObservableObject
    {
        private readonly IList<BingoCardModel> _cards;
        
        private int _currentCardIndex;
        private int _currentPatternIndex;

        /// <summary>
        ///     Constructor for <see cref="BingoGameRoundModel"/>.
        /// </summary>
        /// <param name="cards">A list of the played cards for the round, including their winning patterns.</param>
        /// <param name="ballCall">The ball call for the round, limited to the balls used in the round and no more.</param>
        /// <param name="roundId">The unique ID for the bingo round data.</param>
        public BingoGameRoundModel(
            IList<BingoCardModel> cards,
            BingoBallCallModel ballCall,
            long roundId)
        {
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            BallCall = ballCall ?? throw new ArgumentNullException(nameof(ballCall));

            if (!_cards.Any())
            {
                throw new ArgumentException(nameof(cards));
            }

            RoundId = roundId;
        }

        /// <summary>
        ///     The unique identifier for the bingo round data.
        /// </summary>
        public long RoundId { get; }

        /// <summary>
        ///     The number of cards played during the round.
        /// </summary>
        public int CardCount => _cards.Count;

        /// <summary>
        ///     Indicates whether or not the current card won any patterns.
        /// </summary>
        public bool IsLosingCard => !CurrentCard.Patterns.Any();

        /// <summary>
        ///     Indicates whether or not the round played many cards.
        /// </summary>
        public bool IsMultiCardGame => _cards.Count > 1;

        /// <summary>
        ///     Indicates whether or not the current card has multiple winning patterns.
        /// </summary>
        public bool HasMultipleWinningPatterns => CurrentCard.Patterns.Count > 1;

        /// <summary>
        ///     Gets or sets the index of the currently displayed card. Setting this value also
        ///     resets <see cref="CurrentPatternIndex"/>, and refreshes the card information,
        ///     pattern information, and daubing on the card and ball call.
        /// </summary>
        public int CurrentCardIndex
        {
            get => _currentCardIndex;
            set
            {
                if (!IsMultiCardGame || value < 0 || value >= _cards.Count)
                {
                    return;
                }

                _currentCardIndex = value;

                // Reset pattern display to the first pattern won, if any.
                CurrentPatternIndex = 0;
                UpdateDaubing();

                OnPropertyChanged(nameof(CurrentCardIndex));
                OnPropertyChanged(nameof(CurrentCard));
                OnPropertyChanged(nameof(IsLosingCard));
                OnPropertyChanged(nameof(HasMultipleWinningPatterns));
            }
        }

        /// <summary>
        ///     Gets or sets the index of the currently displayed pattern. Setting this value
        ///     refreshes the pattern information and daubing on the card and ball call.
        /// </summary>
        public int CurrentPatternIndex
        {
            get => _currentPatternIndex;
            set
            {
                if (IsLosingCard || value < 0 || value >= CurrentCard.Patterns.Count)
                {
                    return;
                }

                _currentPatternIndex = value;
                UpdateDaubing();

                OnPropertyChanged(nameof(CurrentPatternIndex));
                OnPropertyChanged(nameof(CurrentPattern));
            }
        }

        /// <summary>
        ///     The data for the card currently displayed.
        /// </summary>
        public BingoCardModel CurrentCard => _cards[CurrentCardIndex];

        /// <summary>
        ///     The data for the pattern currently displayed.
        /// </summary>
        public BingoPatternModel CurrentPattern => IsLosingCard ? null : CurrentCard.Patterns[CurrentPatternIndex];

        /// <summary>
        ///     The ball call data for the round.
        /// </summary>
        public BingoBallCallModel BallCall { get; }

        /// <summary>
        ///     Updates the color daubs for bingo card and ball call numbers based on the
        ///     currently displayed card and pattern.
        /// </summary>
        public void UpdateDaubing()
        {
            var patternDaubs = CurrentCard.Patterns.Any() ? CurrentPattern.DaubedNumbers : null;
            BallCall.UpdateDaubs(CurrentCard.BallCallDaubedNumbers, patternDaubs);
            CurrentCard.UpdateDaubs(patternDaubs);
        }
    }
}
