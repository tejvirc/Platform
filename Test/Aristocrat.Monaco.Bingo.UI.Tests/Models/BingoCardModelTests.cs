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
    public class BingoCardModelTests
    {
        private static readonly BingoAppearance Appearance = new()
        {
            CardInitialNumberColor = Color.Red.Name,
            DaubNumberColor = Color.Blue.Name,
            BallsEarlyNumberColor = Color.Purple.Name
        };

        private static readonly Dictionary<int, List<int>> DaubBitsToIndices = new()
        {
            { BallCallDaubBits, new List<int> { 24, 22, 20, 19, 18, 15, 14, 12, 11, 10, 09, 08, 05, 04, 03, 00} },
            { Pattern1DaubBits, new List<int> { 24, 19, 14, 09, 04 } },
            { Pattern2DaubBits, new List<int> { 20, 15, 10, 05, 00 } }
        };

        private BingoCardModel _target;
        private readonly ObservableCollection<BingoNumberModel> _cardNumbers = new();
        private readonly IList<BingoPatternModel> _patterns = new List<BingoPatternModel>();

        private const BingoNumberState BallCallDaubedNumber = BingoNumberState.BallCallInitial;
        private const BingoNumberState UndaubedNumber = BingoNumberState.CardInitial;
        private const int ArbitraryFontSize = 10;
        private const int SerialNumber = 1234;
        private const int BallCallDaubBits = 0b0000000_10101_11001_10111_11001_11001;
        private const int Pattern1DaubBits = 0b0000000_10000_10000_10000_10000_10000;
        private const int Pattern2DaubBits = 0b0000000_00001_00001_00001_00001_00001;

        [TestInitialize]
        public void Initialize()
        {
            CreateCardNumbers();
        }

        [DataRow(true, false, DisplayName = "Null ObservableCollection<BingoNumberModel>")]
        [DataRow(false, true, DisplayName = "Null IList<BingoPatternModel>")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Fail(bool nullNumbers, bool nullPatterns)
        {
            _target = new BingoCardModel(
                nullNumbers ? null : _cardNumbers,
                nullPatterns ? null : _patterns,
                SerialNumber,
                BallCallDaubBits);
        }

        [TestMethod]
        public void Constructor_EmptyPatternList()
        {
            _target = new BingoCardModel(
                _cardNumbers,
                _patterns, // empty since we didn't call CreatePatterns()
                SerialNumber,
                0);

            Assert.IsNotNull(_target.Patterns);
            Assert.IsNotNull(_target.BallCallDaubedNumbers);
            Assert.AreEqual(0, _target.Patterns.Count);
            Assert.AreEqual(0, _target.BallCallDaubedNumbers.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_EmptyNumberList()
        {
            _target = new BingoCardModel(
                new ObservableCollection<BingoNumberModel>(),
                _patterns,
                SerialNumber,
                0);
        }

        [TestMethod]
        public void Constructor()
        {
            var ballCallBits = 0;
            _target = new BingoCardModel(_cardNumbers, _patterns, SerialNumber, ballCallBits);

            Assert.AreEqual(SerialNumber.ToString(), _target.SerialNumber);
            Assert.AreEqual(ballCallBits, _target.BallCallBitDaubs);
            Assert.AreEqual(_cardNumbers, _target.Numbers);
            Assert.AreEqual(_patterns, _target.Patterns);
            Assert.AreEqual(0, _target.BallCallDaubedNumbers.Count);
        }

        [TestMethod]
        public void CreateCardLayout()
        {
            _target = new BingoCardModel(_cardNumbers, _patterns, SerialNumber, BallCallDaubBits);

            Assert.AreEqual(DaubBitsToIndices[BallCallDaubBits].Count, _target.BallCallDaubedNumbers.Count);

            AssessDaubedNumbers(BallCallDaubBits, _target.BallCallDaubedNumbers);
        }

        [TestMethod]
        public void SetPatternDaubNumbers()
        {
            CreatePatterns();
            _target = new BingoCardModel(_cardNumbers, _patterns, SerialNumber, BallCallDaubBits);

            AssessDaubedNumbers(Pattern1DaubBits, _target.Patterns[0].DaubedNumbers);
            AssessDaubedNumbers(Pattern2DaubBits, _target.Patterns[1].DaubedNumbers);
        }

        [DataRow(true, DisplayName = "Null IList<int>")]
        [DataRow(false, DisplayName = "Empty IList<int>")]
        [DataTestMethod]
        public void UpdateDaubs_NullOrEmptyParam(bool nullParam)
        {
            _target = new BingoCardModel(_cardNumbers, _patterns, SerialNumber, BallCallDaubBits);
            _target.UpdateDaubs(nullParam ? null : new List<int>());

            foreach (var num in _target.Numbers)
            {
                Assert.IsFalse(num.Daubed);
                Assert.AreEqual(_target.BallCallDaubedNumbers.Contains(num.Number)
                    ? BallCallDaubedNumber
                    : UndaubedNumber,
                    num.State);
            }
        }

        [TestMethod]
        public void UpdateDaubs()
        {
            _target = new BingoCardModel(_cardNumbers, _patterns, SerialNumber, BallCallDaubBits);

            var patternDaubNumbers = new List<int>();
            foreach (var index in DaubBitsToIndices[Pattern1DaubBits])
            {
                patternDaubNumbers.Add(_cardNumbers.ElementAt(index).Number);
            }

            _target.UpdateDaubs(patternDaubNumbers);

            foreach (var num in _target.Numbers)
            {
                Assert.AreEqual(patternDaubNumbers.Contains(num.Number), num.Daubed);
            }
        }

        private void AssessDaubedNumbers(int indicesKey, ICollection<int> daubedNumberList)
        {
            foreach (var index in DaubBitsToIndices[indicesKey])
            {
                Assert.IsTrue(daubedNumberList.Contains(_cardNumbers.ElementAt(index).Number));
            }
        }

        private void CreateCardNumbers()
        {
            for (var index = BingoConstants.BingoCardSquares - 1; index >= 0; index--)
            {
                _cardNumbers.Add(new BingoNumberModel(Appearance, ArbitraryFontSize)
                {
                    Number = index == BingoConstants.FreeSpaceIndex ? BingoConstants.CardCenterNumber : index
                });
            }
        }

        private void CreatePatterns()
        {
            _patterns.Add(new BingoPatternModel(12, "Pattern 1", 15, Pattern1DaubBits, 5000));
            _patterns.Add(new BingoPatternModel(22, "Pattern 2", 25, Pattern2DaubBits, 2000));
        }
    }
}
