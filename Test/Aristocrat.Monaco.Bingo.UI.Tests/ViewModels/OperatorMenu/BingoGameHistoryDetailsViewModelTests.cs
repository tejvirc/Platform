namespace Aristocrat.Monaco.Bingo.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Aristocrat.Monaco.Bingo.Common.GameOverlay;
    using Aristocrat.Monaco.Bingo.UI.Models;
    using Aristocrat.Monaco.Bingo.UI.ViewModels.OperatorMenu;
    using Aristocrat.Monaco.Test.Common;
    using Common;
    using Common.Storage;
    using Kernel;
    using Localization.Properties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class BingoGameHistoryDetailsViewModelTests
    {
        private BingoGameHistoryDetailsViewModel _target;
        private BingoGameDescription _data;

        private readonly BingoPattern _patternCardOne = new("Pattern A", 7, CardSerialNumberOnePattern, 500, 20, 1, false, 0001, 1);
        private readonly BingoPattern _pattern1CardTwo = new("Pattern A", 7, CardSerialNumberAllPatterns, 500, 20, 1, false, 0001, 2);
        private readonly BingoPattern _pattern2 = new("Pattern B", 8, CardSerialNumberAllPatterns, 600, 24, 1, false, 0011, 3);
        private readonly BingoPattern _pattern3 = new("Pattern C", 9, CardSerialNumberAllPatterns, 100, 10, 1, false, 0011, 4);

        private const int ArbitraryNumber = 10;
        private const int CardSerialNumberOnePattern = 2710;
        private const int CardSerialNumberAllPatterns = 2709;
        private const int CardSerialNumberNoPatterns = 2711;
        private const int JoinBallIndex = 0;
        private const char FormatDelimiter = ',';
        private const string NoResult = "None";
        private readonly string _format = $"{{0}}{FormatDelimiter}{{1}}";

        private IList<BingoCard> _cards;
        private IList<int> _ballCall;
        private BingoCard _losingCard;
        private BingoCard _singlePatternCard;
        private BingoCard _multiPatternCard;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            MockLocalization.Setup(MockBehavior.Default);
            MockLocalization.Localizer.Setup(x => x.GetString(ResourceKeys.BingoPatternDisplayIndexOfMaxIndex))
                .Returns(_format);
            MockLocalization.Localizer.Setup(x => x.GetString(ResourceKeys.BingoCardDisplayIndexOfMaxIndex))
                .Returns(_format);
            MockLocalization.Localizer.Setup(x => x.GetString(ResourceKeys.NoBingoPatternInformationToDisplay))
                .Returns(NoResult);

            if (Application.Current == null)
            {
                new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateData_Fail()
        {
            _target = new BingoGameHistoryDetailsViewModel(null);
        }

        [TestMethod]
        public void UpdateData_LosingCard()
        {
            EstablishData(true, false, false);
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var result = _target.BingoRoundData;

            Assert.IsNotNull(result);
            Assert.AreEqual(_cards.Count, result.CardCount);
            Assert.AreEqual(_ballCall.Count, result.BallCall.Numbers.Count);
            Assert.AreEqual(0, result.CurrentCardIndex);
            Assert.AreEqual(0, result.CurrentPatternIndex);

            for (var index = 0; index < _ballCall.Count; index++)
            {
                Assert.AreEqual(_ballCall[index], result.BallCall.Numbers[index].Number);
                Assert.AreEqual(BingoNumberState.CardInitial, result.BallCall.Numbers[index].State);
            }

            for (var row = 0; row < BingoConstants.BingoCardDimension; ++row)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; ++col)
                {
                    var index = row * BingoConstants.BingoCardDimension + col;
                    var number = result.CurrentCard.Numbers[index];
                    Assert.AreEqual(_losingCard.Numbers[row, col].Number, number.Number);

                    Assert.AreEqual(BingoNumberState.CardInitial, number.State);
                }
            }
        }

        [TestMethod]
        public void CardNote()
        {
            EstablishData();
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var result = _target.CardNote;

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
            Assert.AreEqual("1", result.Split(FormatDelimiter)[0]);
            Assert.AreEqual(_cards.Count.ToString(), result.Split(FormatDelimiter)[1]);
        }

        [DataRow(true, false, false, DisplayName = "No Patterns")]
        [DataRow(false, true, false, DisplayName = "Single Pattern")]
        [DataRow(false, false, true, DisplayName = "Many Patterns")]
        [DataTestMethod]
        public void PatternNote(bool losingCard, bool singlePatternCard, bool multiPatternCard)
        {
            EstablishData(losingCard, singlePatternCard, multiPatternCard);
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var result = _target.PatternNote;

            Assert.IsFalse(string.IsNullOrWhiteSpace(result));

            if (losingCard)
            {
                Assert.AreEqual(NoResult, result);
            }
            else
            {
                Assert.AreEqual("1", result.Split(FormatDelimiter)[0]);
                Assert.AreEqual(_data.Patterns.Count().ToString(), result.Split(FormatDelimiter)[1]);
            }
        }

        [TestMethod]
        public void DisplayNextCardCommand()
        {
            EstablishData();
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var initialCard = _target.BingoRoundData.CurrentCard;
            var initialCardNote = _target.CardNote;
            var initialPatternNote = _target.PatternNote;
            Assert.AreEqual("1", initialCardNote.Split(FormatDelimiter)[0]);

            // Transition to middle card
            _target.DisplayNextCardCommand.Execute(null);

            var nextCard = _target.BingoRoundData.CurrentCard;
            var nextCardNote = _target.CardNote;
            var nextPatternNote = _target.PatternNote;

            ComparePreviousToCurrentCardDisplay(
                initialCard, nextCard,
                initialCardNote, nextCardNote,
                initialPatternNote, nextPatternNote,
                "2");

            // Transition to end card.
            _target.DisplayNextCardCommand.Execute(null);

            var lastCard = _target.BingoRoundData.CurrentCard;
            var lastCardNote = _target.CardNote;
            var lastPatternNote = _target.PatternNote;

            ComparePreviousToCurrentCardDisplay(
                nextCard, lastCard,
                nextCardNote, lastCardNote,
                nextPatternNote, lastPatternNote,
                "3");

            // Wrap around to the first card.
            _target.DisplayNextCardCommand.Execute(null);

            Assert.AreEqual(initialCard, _target.BingoRoundData.CurrentCard);
            Assert.AreEqual(initialCardNote, _target.CardNote);
            Assert.AreEqual(initialPatternNote, _target.PatternNote);
            Assert.AreEqual("1", _target.CardNote.Split(FormatDelimiter)[0]);
        }

        [TestMethod]
        public void DisplayPreviousCardCommand()
        {
            EstablishData();
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var initialCard = _target.BingoRoundData.CurrentCard;
            var initialCardNote = _target.CardNote;
            var initialPatternNote = _target.PatternNote;
            Assert.AreEqual("1", _target.CardNote.Split(FormatDelimiter)[0]);

            // Wrap around to the end card.
            _target.DisplayPreviousCardCommand.Execute(null);

            var lastCard = _target.BingoRoundData.CurrentCard;
            var lastCardNote = _target.CardNote;
            var lastPatternNote = _target.PatternNote;

            ComparePreviousToCurrentCardDisplay(
                initialCard, lastCard,
                initialCardNote, lastCardNote,
                initialPatternNote, lastPatternNote,
                "3");

            // Transition to middle card.
            _target.DisplayPreviousCardCommand.Execute(null);

            var middleCard = _target.BingoRoundData.CurrentCard;
            var middleCardNote = _target.CardNote;
            var middlePatternNote = _target.PatternNote;

            ComparePreviousToCurrentCardDisplay(
                lastCard, middleCard,
                lastCardNote, middleCardNote,
                lastPatternNote, middlePatternNote,
                "2");

            // Transition to first card
            _target.DisplayPreviousCardCommand.Execute(null);

            Assert.AreEqual(initialCard, _target.BingoRoundData.CurrentCard);
            Assert.AreEqual(initialCardNote, _target.CardNote);
            Assert.AreEqual(initialPatternNote, _target.PatternNote);
            Assert.AreEqual("1", _target.CardNote.Split(FormatDelimiter)[0]);
        }

        [TestMethod]
        public void DisplayNextPatternCommand()
        {
            EstablishData(false, false);
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var initialPattern = _target.BingoRoundData.CurrentPattern;
            var initialPatternNote = _target.PatternNote;
            Assert.AreEqual("1", initialPatternNote.Split(FormatDelimiter)[0]);

            // Display the middle pattern.
            _target.DisplayNextPatternCommand.Execute(null);

            var middlePattern = _target.BingoRoundData.CurrentPattern;
            var middlePatternNote = _target.PatternNote;

            Assert.AreNotEqual(initialPattern, middlePattern);
            Assert.AreNotEqual(initialPatternNote, middlePatternNote);
            Assert.AreEqual("2", middlePatternNote.Split(FormatDelimiter)[0]);

            // Display the end pattern.
            _target.DisplayNextPatternCommand.Execute(null);

            var lastPattern = _target.BingoRoundData.CurrentPattern;
            var lastPatternNote = _target.PatternNote;

            Assert.AreNotEqual(initialPattern, lastPattern);
            Assert.AreNotEqual(initialPatternNote, lastPatternNote);
            Assert.AreEqual("3", lastPatternNote.Split(FormatDelimiter)[0]);

            // Wrap to display the first pattern.
            _target.DisplayNextPatternCommand.Execute(null);

            Assert.AreEqual(initialPattern, _target.BingoRoundData.CurrentPattern);
            Assert.AreEqual(initialPatternNote, _target.PatternNote);
            Assert.AreEqual("1", _target.PatternNote.Split(FormatDelimiter)[0]);
        }

        [TestMethod]
        public void DisplayPreviousPatternCommand()
        {
            EstablishData(false, false);
            _target = new BingoGameHistoryDetailsViewModel(new List<BingoGameDescription> { _data });

            var initialPattern = _target.BingoRoundData.CurrentPattern;
            var initialPatternNote = _target.PatternNote;
            Assert.AreEqual("1", initialPatternNote.Split(FormatDelimiter)[0]);

            // Display the end pattern.
            _target.DisplayPreviousPatternCommand.Execute(null);

            var lastPattern = _target.BingoRoundData.CurrentPattern;
            var lastPatternNote = _target.PatternNote;

            Assert.AreNotEqual(initialPattern, lastPattern);
            Assert.AreNotEqual(initialPatternNote, lastPatternNote);
            Assert.AreEqual("3", lastPatternNote.Split(FormatDelimiter)[0]);

            // Display the middle pattern.
            _target.DisplayPreviousPatternCommand.Execute(null);

            var middlePattern = _target.BingoRoundData.CurrentPattern;
            var middlePatternNote = _target.PatternNote;

            Assert.AreNotEqual(initialPattern, middlePattern);
            Assert.AreNotEqual(initialPatternNote, middlePatternNote);
            Assert.AreEqual("2", middlePatternNote.Split(FormatDelimiter)[0]);

            // Wrap to display the first pattern.
            _target.DisplayPreviousPatternCommand.Execute(null);

            Assert.AreEqual(initialPattern, _target.BingoRoundData.CurrentPattern);
            Assert.AreEqual(initialPatternNote, _target.PatternNote);
            Assert.AreEqual("1", _target.PatternNote.Split(FormatDelimiter)[0]);
        }

        private void ComparePreviousToCurrentCardDisplay(
            BingoCardModel previousCard, BingoCardModel currentCard,
            string previousCardNote, string currentCardNote,
            string previousPatternNote, string currentPatternNote,
            string cardNoteIndex)
        {
            Assert.AreNotEqual(previousCard, currentCard);
            Assert.AreNotEqual(previousCardNote, currentCardNote);
            Assert.AreNotEqual(previousPatternNote, currentPatternNote);
            Assert.AreEqual(cardNoteIndex, currentCardNote.Split(FormatDelimiter)[0]);
        }

        private void EstablishData(
            bool losingCard = true,
            bool singlePatternCard = true,
            bool multiPatternCard = true)
        {
            if (!losingCard && !singlePatternCard && !multiPatternCard)
            {
                Assert.Fail();
            }

            PopulateBallCall();
            PopulateCards(losingCard, singlePatternCard, multiPatternCard);
            var patterns = new List<BingoPattern>();
            if (singlePatternCard)
            {
                patterns.Add(_patternCardOne);
            }

            if (multiPatternCard)
            {
                patterns.Add(_pattern1CardTwo);
                patterns.Add(_pattern2);
                patterns.Add(_pattern3);
            }

            _data = new BingoGameDescription
            {
                BallCallNumbers = _ballCall.Select(x => new BingoNumber(x, BingoNumberState.CardInitial)),
                Cards = _cards,
                DenominationId = 1000,
                JoinBallIndex = JoinBallIndex,
                GameTitleId = ArbitraryNumber,
                GameSerial = ArbitraryNumber,
                Patterns = patterns
            };
        }

        private void PopulateBallCall()
        {
            _ballCall = new List<int>();
            for (var index = 1; index <= BingoConstants.MaxBall; index++)
            {
                _ballCall.Add(index);
            }
        }

        private void PopulateCards(bool losingCard, bool singlePatternCard, bool multiPatternCard)
        {
            var numbers = new BingoNumber[BingoConstants.BingoCardDimension, BingoConstants.BingoCardDimension];
            for (var row = 0; row < BingoConstants.BingoCardDimension; ++row)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; ++col)
                {
                    var number = row * BingoConstants.BingoCardDimension + col;
                    numbers[row, col] = new BingoNumber(
                        number == BingoConstants.FreeSpaceIndex ? BingoConstants.CardCenterNumber : number,
                        BingoNumberState.CardInitial);
                }
            }

            _cards = new List<BingoCard>();

            if (losingCard)
            {
                _losingCard = new BingoCard(numbers, CardSerialNumberNoPatterns, 0, 0, false);
                _cards.Add(_losingCard);
            }

            if (singlePatternCard)
            {
                _singlePatternCard = new BingoCard(numbers, CardSerialNumberOnePattern, 0001, 0001, false);
                _cards.Add(_singlePatternCard);
            }

            if (multiPatternCard)
            {
                _multiPatternCard = new BingoCard(numbers, CardSerialNumberAllPatterns, 0011, 0011, false);
                _cards.Add(_multiPatternCard);
            }
        }
    }
}
