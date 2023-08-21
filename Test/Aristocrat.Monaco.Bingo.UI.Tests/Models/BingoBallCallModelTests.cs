namespace Aristocrat.Monaco.Bingo.UI.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using Common.GameOverlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UI.Models;

    [TestClass]
    public class BingoBallCallModelTests
    {
        private static readonly BingoAppearance Appearance = new()
        {
            CardInitialNumberColor = Color.Red.Name,
            DaubNumberColor = Color.Blue.Name,
            BallsEarlyNumberColor = Color.Purple.Name
        };

        private const BingoNumberState DefaultNumberState = BingoNumberState.Undefined;
        private const BingoNumberState NumberNotFoundState = BingoNumberState.CardInitial;
        private const BingoNumberState NumberFoundState = BingoNumberState.BallCallInitial;
        private const bool DefaultDaubState = false;
        private const bool NumberFoundDaubState = true;
        private const int ArbitraryFontSize = 10;
        private const int UnusedNumber = 1;
        private const int BallNumber1 = 15;
        private const int BallNumber2 = 25;
        private const int BallNumber3 = 35;

        private BingoBallCallModel _target;
        private ObservableCollection<BingoNumberModel> _balls;

        [TestInitialize]
        public void Initialize()
        {
            _balls = new ObservableCollection<BingoNumberModel>();
            _target = new BingoBallCallModel(_balls);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Fail()
        {
            _target = new BingoBallCallModel(null);
        }

        [DataRow(true, DisplayName = "Empty ball call")]
        [DataRow(false, DisplayName = "Populated ball call")]
        [DataTestMethod]
        public void Constructor(bool emptyBallCall)
        {
            if (!emptyBallCall)
            {
                PopulateBallCall();
            }

            _target = new BingoBallCallModel(_balls);

            Assert.AreEqual(_balls.Count, _target.Numbers.Count);
            Assert.AreEqual(_balls, _target.Numbers);

            CheckStates(_target.Numbers, DefaultNumberState, DefaultDaubState);
        }

        [TestMethod]
        public void UpdateDaubs_Null()
        {
            UpdateDaubs(null, null);

            CheckStates(_target.Numbers, NumberNotFoundState, DefaultDaubState);
        }

        [TestMethod]
        public void UpdateDaubs_Empty()
        {
            var sample = new List<int>();
            UpdateDaubs(sample, sample);

            CheckStates(_target.Numbers, NumberNotFoundState, DefaultDaubState);
        }

        [TestMethod]
        public void UpdateDaubs_NumberNotFound()
        {
            var sample = new List<int> { UnusedNumber };
            UpdateDaubs(sample, sample);

            CheckStates(_target.Numbers, NumberNotFoundState, DefaultDaubState);
        }

        [TestMethod]
        public void UpdateDaubs_NumberFound()
        {
            var sample = new List<int> { BallNumber1 };

            UpdateDaubs(sample, sample);

            foreach (var num in _target.Numbers)
            {
                if (num.Number == BallNumber1)
                {
                    Assert.AreEqual(NumberFoundState, num.State);
                    Assert.AreEqual(NumberFoundDaubState, num.Daubed);
                    continue;
                }

                Assert.AreEqual(NumberNotFoundState, num.State);
                Assert.AreEqual(DefaultDaubState, num.Daubed);
            }
        }

        private void UpdateDaubs(IList<int> cardDaubs, IList<int> patternDaubs)
        {
            PopulateBallCall();

            _target = new BingoBallCallModel(_balls);
            _target.UpdateDaubs(cardDaubs, patternDaubs);
        }

        private void PopulateBallCall()
        {
            _balls = new ObservableCollection<BingoNumberModel>
            {
                new(Appearance, ArbitraryFontSize) { Number = BallNumber1 },
                new(Appearance, ArbitraryFontSize) { Number = BallNumber2 },
                new(Appearance, ArbitraryFontSize) { Number = BallNumber3 }
            };
        }

        private static void CheckStates(IEnumerable<BingoNumberModel> numbers, BingoNumberState expectedNumberState, bool expectedDaubState)
        {
            foreach (var num in numbers)
            {
                Assert.AreEqual(expectedNumberState, num.State);
                Assert.AreEqual(expectedDaubState, num.Daubed);
            }
        }
    }
}
