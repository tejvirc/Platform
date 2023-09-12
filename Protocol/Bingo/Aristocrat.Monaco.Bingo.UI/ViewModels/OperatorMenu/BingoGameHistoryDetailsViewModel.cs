namespace Aristocrat.Monaco.Bingo.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Common;
    using Common.GameOverlay;
    using Common.Storage;
    using CommunityToolkit.Mvvm.Input;
    using Gaming.Contracts;
    using Localization.Properties;
    using Models;

    /// <summary>
    ///     Provides a means of viewing bingo round details.
    /// </summary>
    public class BingoGameHistoryDetailsViewModel : OperatorMenuSaveViewModelBase, IGameRoundDetailsViewModel
    {
        private static readonly BingoAppearance Appearance = new()
        {
            CardInitialNumberColor = Color.DeepSkyBlue.Name,
            DaubNumberColor = Color.HotPink.Name,
            BallsEarlyNumberColor = Color.White.Name,
            BallsLateNumberColor = Color.White.Name,
        };

        private readonly string _noPatternInfoToDisplay;
        private readonly string _patternIndexOfMaxIndex;
        private readonly string _cardIndexOfMaxIndex;

        private const int NumberModelFontSize = 17;

        public BingoGameHistoryDetailsViewModel(BingoGameDescription bingoGame)
        {
            _noPatternInfoToDisplay = Localizer.For(CultureFor.Operator)
                .GetString(ResourceKeys.NoBingoPatternInformationToDisplay);
            _patternIndexOfMaxIndex = Localizer.For(CultureFor.Operator)
                .GetString(ResourceKeys.BingoPatternDisplayIndexOfMaxIndex);
            _cardIndexOfMaxIndex = Localizer.For(CultureFor.Operator)
                .GetString(ResourceKeys.BingoCardDisplayIndexOfMaxIndex);

            // Set up button actions
            DisplayNextPatternCommand = new RelayCommand<object>(DisplayNextPattern);
            DisplayPreviousPatternCommand = new RelayCommand<object>(DisplayPreviousPattern);
            DisplayNextCardCommand = new RelayCommand<object>(DisplayNextCard);
            DisplayPreviousCardCommand = new RelayCommand<object>(DisplayPreviousCard);
            BingoRoundData = GetRoundData(bingoGame ?? throw new ArgumentNullException(nameof(bingoGame)));

            BingoRoundData.UpdateDaubing();
        }

        /// <summary>
        ///     The data for the bingo round, including ball call, card(s), and winning pattern(s).
        /// </summary>
        public BingoGameRoundModel BingoRoundData { get; }

        /// <summary>
        ///     Display text indicating which card is currently displayed in the case
        ///     of a round that has multiple cards.
        /// </summary>
        public string CardNote => string.Format(_cardIndexOfMaxIndex,
            BingoRoundData.CurrentCardIndex + 1,
            BingoRoundData.CardCount);

        /// <summary>
        ///     Display text indicating whether or not any patterns were won, and which
        ///     pattern is currently displayed in the case of a round that has multiple patterns.
        /// </summary>
        public string PatternNote => BingoRoundData.IsLosingCard
            ? _noPatternInfoToDisplay
            : string.Format(_patternIndexOfMaxIndex,
                BingoRoundData.CurrentPatternIndex + 1,
                BingoRoundData.CurrentCard.Patterns.Count);

        /// <summary>
        ///     "Next Pattern" button command, implemented via <see cref="DisplayNextPattern"/>.
        /// </summary>
        public ICommand DisplayNextPatternCommand { get; }

        /// <summary>
        ///     "Previous Pattern" button command, implemented via <see cref="DisplayPreviousPattern"/>.
        /// </summary>
        public ICommand DisplayPreviousPatternCommand { get; }

        /// <summary>
        ///     "Next Card" button command, implemented via <see cref="DisplayNextCard"/>.
        /// </summary>
        public ICommand DisplayNextCardCommand { get; }

        /// <summary>
        ///     "Previous Card" button command, implemented via <see cref="DisplayPreviousCard"/>.
        /// </summary>
        public ICommand DisplayPreviousCardCommand { get; }

        private static BingoGameRoundModel GetRoundData(BingoGameDescription bingoGame)
        {
            return new BingoGameRoundModel(
                bingoGame.Cards.Select(ToBingoCardModel).ToList(),
                new BingoBallCallModel(GenerateNumberModels(bingoGame.BallCallNumbers)),
                bingoGame.GameSerial);

            BingoCardModel ToBingoCardModel(BingoCard card) =>
                new BingoCardModel(
                    GenerateNumberModels(card, true),
                    bingoGame.Patterns.Where(x => KeepPattern(x, card)).Select(ToBingoPatternModel).ToList(),
                    card.SerialNumber,
                    card.DaubedBits);

            BingoPatternModel ToBingoPatternModel(BingoPattern pattern) =>
                new BingoPatternModel(
                    pattern.PatternId,
                    pattern.Name,
                    pattern.BallQuantity,
                    pattern.BitFlags,
                    pattern.WinAmount);

            bool KeepPattern(BingoPattern x, BingoCard card) =>
                x.CardSerial == card.SerialNumber && (!x.IsGameEndWin || bingoGame.GameEndWinClaimAccepted);
        }

        private static ObservableCollection<BingoNumberModel> GenerateNumberModels(BingoCard card, bool bingoCard = false)
        {
            var numberModels = new ObservableCollection<BingoNumberModel>();
            foreach (var bingoNumber in card.Numbers)
            {
                numberModels.Add(
                    new BingoNumberModel(Appearance, NumberModelFontSize)
                    {
                        Number = bingoNumber.Number,
                        State = BingoNumberState.CardInitial
                    });
            }

            if (bingoCard)
            {
                var freeSpace = numberModels.ElementAtOrDefault(BingoConstants.FreeSpaceIndex);

                if (freeSpace is not null)
                {
                    freeSpace.State = BingoNumberState.BallCallInitial;
                }
            }

            return numberModels;
        }

        private static ObservableCollection<BingoNumberModel> GenerateNumberModels(IEnumerable<BingoNumber> bingoNumbers)
        {
            var numberModels = new ObservableCollection<BingoNumberModel>();
            foreach (var number in bingoNumbers)
            {
                numberModels.Add(
                    new BingoNumberModel(Appearance, NumberModelFontSize)
                    {
                        Number = number.Number,
                        State = number.State
                    });
            }

            return numberModels;
        }

        private void DisplayNextPattern(object o)
        {
            BingoRoundData.CurrentPatternIndex =
                BingoRoundData.CurrentPatternIndex == BingoRoundData.CurrentCard.Patterns.Count - 1
                    ? 0
                    : BingoRoundData.CurrentPatternIndex + 1;

            PatternChanged();
        }

        private void DisplayPreviousPattern(object o)
        {
            BingoRoundData.CurrentPatternIndex =
                BingoRoundData.CurrentPatternIndex == 0
                    ? BingoRoundData.CurrentCard.Patterns.Count - 1
                    : BingoRoundData.CurrentPatternIndex - 1;

            PatternChanged();
        }

        private void DisplayNextCard(object o)
        {
            BingoRoundData.CurrentCardIndex =
                BingoRoundData.CurrentCardIndex == BingoRoundData.CardCount - 1
                    ? 0
                    : BingoRoundData.CurrentCardIndex + 1;
            CardChanged();
        }

        private void DisplayPreviousCard(object o)
        {
            BingoRoundData.CurrentCardIndex =
                BingoRoundData.CurrentCardIndex == 0
                    ? BingoRoundData.CardCount - 1
                    : BingoRoundData.CurrentCardIndex - 1;
            CardChanged();
        }

        private void CardChanged()
        {
            OnPropertyChanged(nameof(CardNote));

            PatternChanged();
        }

        private void PatternChanged()
        {
            OnPropertyChanged(nameof(PatternNote));
        }
    }
}
