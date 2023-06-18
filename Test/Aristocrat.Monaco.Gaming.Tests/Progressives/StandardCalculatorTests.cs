namespace Aristocrat.Monaco.Gaming.Tests.Progressives
{
    using Aristocrat.Monaco.Application.Contracts.Tests;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Progressives;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class StandardCalculatorTests
    {
        private ICalculatorStrategy _target;
        private Mock<IMysteryProgressiveProvider> _mockMysteryProgressiveProvider;

        [TestInitialize]
        public void Init()
        {
            _mockMysteryProgressiveProvider = new Mock<IMysteryProgressiveProvider>();
            _target = new StandardCalculator(_mockMysteryProgressiveProvider.Object);
        }

        [DataRow(101_00_000L, 100L, 0L, 100_00_000L, 1_00_000L, 340000L /*3.4% incrementRate*/)]
        [DataRow(125_00_000L, 3400L, 0L, 500_00_000L, 5_00_000L, 340000L /*3.4% incrementRate*/)]
        [DataRow(135_00_000L, 1200L, 0L, 500_00_000L, 25_00_000L, 1000000L /*100.0% incrementRate*/)]
        [DataRow(177_00_000L, 1400L, 0L, 500_00_000L, 47_00_000L, 2000000L /*200.0% incrementRate*/)]
        [DataTestMethod]
        public void TestResetMethod(long current, long reset, long hidden, long ceiling, long overflowResult, long incrementRate)
        {
            var progressiveLevel = Mock.Of<ProgressiveLevel>();

            progressiveLevel.CurrentValue = current;
            progressiveLevel.IncrementRate = incrementRate; // Percentage
            progressiveLevel.HiddenIncrementRate = incrementRate; // Percentage
            progressiveLevel.ResetValue = reset;
            progressiveLevel.HiddenValue = hidden;
            progressiveLevel.MaximumValue = ceiling;
            progressiveLevel.Overflow = overflowResult;
            progressiveLevel.LevelType = ProgressiveLevelType.Sap;
            progressiveLevel.TriggerControl = TriggerType.Game;

            _target.Reset(progressiveLevel);

            Assert.AreEqual(progressiveLevel.CurrentValue, progressiveLevel.ResetValue + overflowResult + hidden);
        }

        [DataRow(100L, 100L, 100L, 130L, 1000000L, 130L, 4L, 340000L /*3.4% incrementRate*/, ProgressiveLevelType.Sap)]
        [DataRow(100L, 100L, 100L, 2000L, 1000000L, 134L, 0L, 340000L /*3.4% incrementRate*/, ProgressiveLevelType.Sap)]
        [DataRow(100L, 100L, 100L, 200L, 1000000L, 200L, 0L, 1000000L /*100.0% incrementRate*/, ProgressiveLevelType.Sap)]
        [DataRow(100L, 100L, 100L, 2000L, 1000000L, 300L, 0L, 2000000L /*200.0% incrementRate*/, ProgressiveLevelType.Selectable)]
        [DataTestMethod]
        public void TestIncrementMethod(long current, long reset, long hidden, long ceiling, long wager, long currentValueResult, long overflowResult, long incrementRate, ProgressiveLevelType levelType)
        {
            var progressiveLevel = Mock.Of<ProgressiveLevel>();
            progressiveLevel.CurrentValue = current;
            progressiveLevel.IncrementRate = incrementRate; // Percentage
            progressiveLevel.HiddenIncrementRate = incrementRate; // Percentage
            progressiveLevel.ResetValue = reset;
            progressiveLevel.HiddenValue = hidden; 
            progressiveLevel.MaximumValue = ceiling;
            progressiveLevel.LevelType = levelType;
            _target.Increment(progressiveLevel, wager, 0L, new TestMeter("new", new TestMeterClassification()));

            Assert.IsTrue(progressiveLevel.CurrentValue == currentValueResult);
            Assert.IsTrue(progressiveLevel.Overflow == overflowResult);
        }

        [DataRow(101_00_000L, 100L, 0L, 100_00_000L, 1_00_000L, 340000L /*3.4% incrementRate*/, 100_00_000L/*Mystery Trigger*/)]
        [DataRow(125_00_000L, 3400L, 0L, 500_00_000L, 5_00_000L, 340000L /*3.4% incrementRate*/, 120_00_000L/*Mystery Trigger*/)]
        [DataRow(135_00_000L, 1200L, 0L, 500_00_000L, 25_00_000L, 1000000L /*100.0% incrementRate*/, 110_00_000L/*Mystery Trigger*/)]
        [DataRow(177_00_000L, 1400L, 0L, 500_00_000L, 47_00_000L, 2000000L /*200.0% incrementRate*/, 130_00_000L/*Mystery Trigger*/)]
        [DataTestMethod]
        public void ClaimMysteryProgressive(long current, long reset, long hidden, long ceiling,long overflowResult, long incrementRate, long triggerAmount)
        {
            _mockMysteryProgressiveProvider.Setup(x => x.TryGetMagicNumber(It.IsAny<ProgressiveLevel>(), out triggerAmount)).Returns(true);
           
            var progressiveLevel = Mock.Of<ProgressiveLevel>();

            progressiveLevel.CurrentValue = current;
            progressiveLevel.IncrementRate = incrementRate; // Percentage
            progressiveLevel.HiddenIncrementRate = incrementRate; // Percentage
            progressiveLevel.ResetValue = reset;
            progressiveLevel.HiddenValue = hidden;
            progressiveLevel.MaximumValue = ceiling;
            progressiveLevel.LevelType = ProgressiveLevelType.Sap;
            progressiveLevel.TriggerControl = TriggerType.Mystery;

            var progressiveWin = _target.Claim(progressiveLevel);

            // Progressive Win is Trigger Amount
            Assert.AreEqual(progressiveWin,triggerAmount);
            Assert.AreEqual(progressiveLevel.CurrentValue, progressiveLevel.ResetValue + overflowResult);
            _mockMysteryProgressiveProvider.Verify(x => x.TryGetMagicNumber(It.IsAny<ProgressiveLevel>(), out triggerAmount), Times.Once);
        }

        [DataRow(101_00_000L, 100L, 0L, 100_00_000L, 1_00_000L, 340000L /*3.4% incrementRate*/)]
        [DataRow(125_00_000L, 3400L, 0L, 500_00_000L, 5_00_000L, 340000L /*3.4% incrementRate*/)]
        [DataRow(135_00_000L, 1200L, 0L, 500_00_000L, 25_00_000L, 1000000L /*100.0% incrementRate*/)]
        [DataRow(177_00_000L, 1400L, 0L, 500_00_000L, 47_00_000L, 2000000L /*200.0% incrementRate*/)]
        [DataTestMethod]
        public void ClaimProgressive(long current, long reset, long hidden, long ceiling, long overflowResult, long incrementRate)
        {
            var progressiveLevel = Mock.Of<ProgressiveLevel>();

            progressiveLevel.CurrentValue = current;
            progressiveLevel.IncrementRate = incrementRate; // Percentage
            progressiveLevel.HiddenIncrementRate = incrementRate; // Percentage
            progressiveLevel.ResetValue = reset;
            progressiveLevel.HiddenValue = hidden;
            progressiveLevel.MaximumValue = ceiling;
            progressiveLevel.LevelType = ProgressiveLevelType.Sap;
            progressiveLevel.TriggerControl = TriggerType.Game;

            var progressiveWin = _target.Claim(progressiveLevel);

            // Progressive Win is CurrentAmount
            Assert.AreEqual(progressiveWin, current);

            // Test overlow
        }
    }
}
