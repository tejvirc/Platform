namespace Aristocrat.Monaco.Bingo.UI.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using Aristocrat.Monaco.Bingo.UI.Models;
    using Common;
    using Common.GameOverlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BingoGameRoundModelTests
    {
        private static readonly BingoAppearance Appearance = new()
        {
            CardInitialNumberColor = Color.Red.Name,
            DaubNumberColor = Color.Blue.Name,
            BallsEarlyNumberColor = Color.Purple.Name
        };

        private static readonly BingoPatternModel Pattern1 = new(7,"Pattern A",20,0001,500);
        private static readonly BingoPatternModel Pattern2 = new(8,"Pattern B",24,0011,600);
        private const BingoNumberState NumberFoundState = BingoNumberState.BallCallInitial;
        private const int ArbitraryFontSize = 10;
        private const int BallNumber1 = 15;
        private const int BallNumber2 = 25;
        private const int BallNumber3 = 35;
        private const int CardSerialNumber = 2710;
        private const int DefaultIndex = 0;
        private const long RoundId = 17;

        private BingoGameRoundModel _target;
        private IList<BingoCardModel> _cards;
        private BingoBallCallModel _ballCall;
        private BingoCardModel _losingCard;
        private BingoCardModel _singlePatternCard;
        private BingoCardModel _multiPatternCard;

        [TestInitialize]
        public void Initialize()
        {
            _cards = new List<BingoCardModel>();
            PopulateBallCall();
            PopulateCards();
        }

        [DataRow(true, false, DisplayName = "Null IList<BingoCardModel>")]
        [DataRow(false, true, DisplayName = "Null BingoBallCallModel")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Null(bool nullCards, bool nullBallCall)
        {
            _target = new BingoGameRoundModel(
                nullCards ? null : _cards,
                nullBallCall ? null : _ballCall,
                RoundId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_Empty()
        {
            _target = new BingoGameRoundModel(new List<BingoCardModel>(), _ballCall, RoundId);
        }

        [TestMethod]
        public void Constructor()
        {
            _target = new BingoGameRoundModel(_cards, _ballCall, RoundId);

            Assert.AreEqual(RoundId, _target.RoundId);
            Assert.AreEqual(_cards.Count, _target.CardCount);
            Assert.IsTrue(_target.IsMultiCardGame);
            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);
            Assert.IsFalse(_target.IsLosingCard);
            Assert.AreEqual(_ballCall, _target.BallCall);
            Assert.AreEqual(_cards[DefaultIndex], _target.CurrentCard);
            Assert.AreEqual(_cards[DefaultIndex].Patterns[DefaultIndex], _target.CurrentPattern);
        }

        [DataRow(true, DefaultIndex, DisplayName = "No change")]
        [DataRow(true, -1, DisplayName = "Negative index")]
        [DataRow(true, 70, DisplayName = "Index out of range")]
        [DataRow(false, DefaultIndex, DisplayName = "Not multi card game")]
        [DataTestMethod]
        public void CurrentCardIndex_Fail(bool multiCard, int index)
        {
            if (!multiCard)
            {
                _cards.Remove(_cards.Last());
                _cards.Remove(_cards.Last());
            }

            _target = new BingoGameRoundModel(_cards, _ballCall, RoundId);

            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);

            _target.CurrentCardIndex = index;

            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);
        }

        [TestMethod]
        public void CurrentCardIndex()
        {
            _target = new BingoGameRoundModel(_cards, _ballCall, RoundId);

            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);

            const int newIndex = 1;
            _target.CurrentCardIndex = newIndex;

            Assert.AreEqual(newIndex, _target.CurrentCardIndex);
            Assert.AreEqual(_cards[newIndex], _target.CurrentCard);
            Assert.IsTrue(_target.CurrentCard.Numbers.Select(
                x => x.State == NumberFoundState).ToList().Any());
        }

        [DataRow(false, DefaultIndex, DisplayName = "No change")]
        [DataRow(false, -1, DisplayName = "Negative index")]
        [DataRow(false, 70, DisplayName = "Index out of range")]
        [DataRow(true, DefaultIndex, DisplayName = "No patterns won")]
        [DataTestMethod]
        public void CurrentPatternIndex_Fail(bool losingCard, int index)
        {
            _target = new BingoGameRoundModel(_cards, _ballCall, RoundId);

            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);

            if (losingCard)
            {
                _target.CurrentCardIndex = _cards.IndexOf(_losingCard);
            }

            _target.CurrentCardIndex = index;

            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);
        }

        [TestMethod]
        public void CurrentPatternIndex()
        {
            _target = new BingoGameRoundModel(_cards, _ballCall, RoundId);

            Assert.AreEqual(DefaultIndex, _target.CurrentCardIndex);
            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);

            // Change the card to one that includes multiple patterns.
            var cardIndex = _cards.IndexOf(_multiPatternCard);
            _target.CurrentCardIndex = cardIndex;

            Assert.AreEqual(cardIndex, _target.CurrentCardIndex);
            Assert.AreEqual(_multiPatternCard, _target.CurrentCard);
            Assert.IsTrue(_target.HasMultipleWinningPatterns);

            // Test: Change pattern index to valid value.
            var newPatternIndex = 1;
            _target.CurrentPatternIndex = newPatternIndex;

            Assert.AreEqual(newPatternIndex, _target.CurrentPatternIndex);
            Assert.AreEqual(_cards[cardIndex].Patterns[newPatternIndex], _target.CurrentPattern);

            // Test: Pattern Index resets when card changes.
            _target.CurrentCardIndex = DefaultIndex;

            Assert.AreEqual(DefaultIndex, _target.CurrentPatternIndex);

            // Test: CurrentPattern when losing card.
            _target.CurrentCardIndex = _cards.IndexOf(_losingCard);
            Assert.IsNull(_target.CurrentPattern);
        }

        private void PopulateBallCall()
        {
            _ballCall = new BingoBallCallModel(
                new ObservableCollection<BingoNumberModel>
                {
                    new(Appearance, ArbitraryFontSize) { Number = BallNumber1 },
                    new(Appearance, ArbitraryFontSize) { Number = BallNumber2 },
                    new(Appearance, ArbitraryFontSize) { Number = BallNumber3 }
                });
        }

        private void PopulateCards()
        {
            var numbers = new ObservableCollection<BingoNumberModel>();
            for (var index = 0; index < BingoConstants.BingoCardDimension * BingoConstants.BingoCardDimension; index++)
            {
                numbers.Add(new BingoNumberModel(Appearance, ArbitraryFontSize)
                {
                    Number = index == BingoConstants.FreeSpaceIndex ? BingoConstants.CardCenterNumber : index
                });
            }

            _losingCard = new BingoCardModel(numbers, new List<BingoPatternModel>(), CardSerialNumber + 1, 0000);
            _singlePatternCard = new BingoCardModel(numbers, new List<BingoPatternModel> { Pattern1 }, CardSerialNumber, 0001);
            _multiPatternCard = new BingoCardModel(numbers, new List<BingoPatternModel> { Pattern1, Pattern2 }, CardSerialNumber - 1, 0011);

            _cards.Add(_singlePatternCard);
            _cards.Add(_losingCard);
            _cards.Add(_multiPatternCard);
        }
    }
}
