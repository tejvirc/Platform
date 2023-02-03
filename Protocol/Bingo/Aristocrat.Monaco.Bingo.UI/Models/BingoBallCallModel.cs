namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Common.GameOverlay;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Data to display within <see cref="ViewModels.OperatorMenu.BingoGameHistoryDetailsViewModel"/>;
    ///     contains the data for a ball call.
    /// </summary>
    public class BingoBallCallModel : ObservableObject
    {
        /// <summary>
        ///     Constructor for <see cref="BingoBallCallModel"/>.
        /// </summary>
        /// <param name="numbers">The numbers of the balls called in the ball call.</param>
        public BingoBallCallModel(
            ObservableCollection<BingoNumberModel> numbers)
        {
            Numbers = numbers ?? throw new ArgumentNullException(nameof(numbers));
        }

        public ObservableCollection<BingoNumberModel> Numbers { get; }

        /// <summary>
        ///     Updates the ball call numbers with daubing for matching numbers from the bingo card
        ///     and a single winning pattern, if provided. This display is for a single card and pattern; both are optional.
        /// </summary>
        /// <param name="cardDaubs">The covered numbers on the bingo card.</param>
        /// <param name="patternDaubs">The covered numbers from the winning pattern.</param>
        public void UpdateDaubs(IList<int> cardDaubs, IList<int> patternDaubs)
        {
            cardDaubs ??= new List<int>();
            patternDaubs ??= new List<int>();

            foreach (var number in Numbers)
            {
                number.State = cardDaubs.Contains(number.Number)
                    ? BingoNumberState.BallCallInitial
                    : BingoNumberState.CardInitial;
                number.Daubed = patternDaubs.Contains(number.Number);
            }

            OnPropertyChanged(nameof(Numbers));
        }
    }
}
