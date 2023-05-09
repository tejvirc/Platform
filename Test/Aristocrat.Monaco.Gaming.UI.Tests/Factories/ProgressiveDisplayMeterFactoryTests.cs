namespace Aristocrat.Monaco.Gaming.UI.Tests.Factories
{
    using System;
    using System.Linq.Expressions;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.MeterPage;
    using Aristocrat.Monaco.Application.UI.MeterPage;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.UI.Factories;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ProgressiveDisplayMeterFactoryTests
    {
        private MeterNode _meterNode;

        private Mock<IViewableProgressiveLevel> _progressiveLevel;
        private Mock<IMeter> _progressiveMeter;
        private Mock<IProgressiveMeterManager> _progressiveMeterManager;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _meterNode = new MeterNode();
            _progressiveLevel = new Mock<IViewableProgressiveLevel>();
            _progressiveMeter = new Mock<IMeter>();
            _progressiveMeterManager = MoqServiceManager.CreateAndAddService<IProgressiveMeterManager>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<double>())).Returns(ApplicationConstants.DefaultCurrencyMultiplier);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>())).Returns(10000000000000L);
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>())).Returns(100000000L);
            SetupProgressiveLevel();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(ProgressiveMeters.ProgressivePackNameDisplayMeter, "UnitTestProgressivePackDisplayName", "UnitTestProgressivePackName")]
        [DataRow(ProgressiveMeters.LevelNameDisplayMeter, "UnitTestLevelDisplayName", "UnitTestGetValue")]
        [DataRow(ProgressiveMeters.CurrentValueDisplayMeter, "CurrentValueDisplayName", "$12.34")]
        [DataRow(ProgressiveMeters.OverflowDisplayMeter, "OverflowDisplayName", "$0.35")]
        [DataRow(ProgressiveMeters.CeilingDisplayMeter, "CeilingDisplayName", "$23.45")]
        [DataRow(ProgressiveMeters.HiddenIncrementDisplayMeter, "HiddenIncrementDisplayName", "1.23%")]
        [DataRow(ProgressiveMeters.InitialValueDisplayMeter, "InitialValueDisplayName", "$1.35")]
        [DataRow(ProgressiveMeters.IncrementDisplayMeter, "IncrementDisplayName", "1.23%")]
        [DataRow(ProgressiveMeters.StartupDisplayMeter, "StartupDisplayName", "$2.46")]
        [DataRow(ProgressiveMeters.WagerBetLevelsDisplayMeter, "WagerBetLevelsDisplayName", "$10,000.00")]
        [DataTestMethod]
        public void CreateProgressiveDisplayMeterSuccess(string meterName, string displayName, string expectedValue)
        {
            SetupFactoryObjects(meterName, displayName);
            CheckValues(ProgressiveDisplayMeterFactory.Build(_progressiveMeterManager.Object, _progressiveLevel.Object, _meterNode, false, 1000, 0), displayName, expectedValue);
        }
        
        [TestMethod]
        public void CreateWagerBetLevelsDisplayMeter_DisplayWagerColumnFalse()
        {
            _progressiveLevel.SetupGet(p => p.CreationType).Returns(LevelCreationType.Default);
            SetupFactoryObjects(ProgressiveMeters.WagerBetLevelsDisplayMeter, "WagerBetLevelsDisplayName");
            CheckValues(ProgressiveDisplayMeterFactory.Build(_progressiveMeterManager.Object, _progressiveLevel.Object, _meterNode, false, 1000, 0), "WagerBetLevelsDisplayName", "$0.00");
        }

        [DataRow(ProgressiveMeters.WageredAmount, "WageredAmountDisplayName", "$10.00", true, true)]
        [DataRow(ProgressiveMeters.WageredAmount, "WageredAmountDisplayName", "$0.00", false, true)]
        [DataRow(ProgressiveMeters.PlayedCount, "PlayedCountDisplayName", "1,000,000", true, false)]
        [DataRow(ProgressiveMeters.PlayedCount, "PlayedCountDisplayName", "0", false, false)]
        [DataRow(ProgressiveMeters.ProgressiveLevelBulkContribution, "ProgressiveLevelBulkContributionDisplayName", "$10.00", true, true)]
        [DataRow(ProgressiveMeters.ProgressiveLevelBulkContribution, "ProgressiveLevelBulkContributionDisplayName", "$0.00", false, true)]
        [DataRow(ProgressiveMeters.ProgressiveLevelWinAccumulation, "ProgressiveLevelWinAccumulationDisplayName", "$10.00", true, true)]
        [DataRow(ProgressiveMeters.ProgressiveLevelWinAccumulation, "ProgressiveLevelWinAccumulationDisplayName", "$0.00", false, true)]
        [DataRow(ProgressiveMeters.ProgressiveLevelWinOccurrence, "ProgressiveLevelWinOccurrenceDisplayName", "1,000,000", true, false)]
        [DataRow(ProgressiveMeters.ProgressiveLevelWinOccurrence, "ProgressiveLevelWinOccurrenceDisplayName", "0", false, false)]
        [DataTestMethod]
        public void CreateProgressiveMeterUsingIMeterSuccess(string meterName, string displayName, string expectedValue, bool meterShouldExist, bool currencyClassNeeded)
        {
            SetupFactoryObjects(meterName, displayName);
            SetupProgressiveIMeter(1000000, meterShouldExist, currencyClassNeeded);
            CheckValues(ProgressiveDisplayMeterFactory.Build(_progressiveMeterManager.Object, _progressiveLevel.Object, _meterNode, false, 1000, 0), displayName, expectedValue);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void CreateUnknownProgressiveDisplayMeterFailed()
        {
            SetupFactoryObjects("Unknown", "Unknown");
            var result = ProgressiveDisplayMeterFactory.Build(_progressiveMeterManager.Object, _progressiveLevel.Object, _meterNode, false, 1000, 0);
        }

        private void CheckValues(DisplayMeter result, string expectedName, string expectedValue)
        {
            Assert.AreEqual(expectedName, result.Name);
            Assert.AreEqual(expectedValue, result.Value);
        }

        private void SetupProgressiveLevelProperty<T>(Expression<Func<IViewableProgressiveLevel, T>> progressiveInfoGetter, T returnValue)
        {
            _progressiveLevel.SetupGet(progressiveInfoGetter).Returns(returnValue);
        }

        private void SetupProgressiveLevel()
        {
            SetupProgressiveLevelProperty(m => m.ProgressivePackName, "UnitTestProgressivePackName");
            SetupProgressiveLevelProperty(m => m.LevelName, "UnitTestGetValue");
            SetupProgressiveLevelProperty(m => m.CurrentValue, 1234567);
            SetupProgressiveLevelProperty(m => m.WagerCredits, 1000000);
            SetupProgressiveLevelProperty(m => m.ResetValue, 246802);
            SetupProgressiveLevelProperty(m => m.IncrementRate, 123000000);
            SetupProgressiveLevelProperty(m => m.InitialValue, 135791);
            SetupProgressiveLevelProperty(m => m.HiddenIncrementRate, 123000000);
            SetupProgressiveLevelProperty(m => m.MaximumValue, 2345678);
            SetupProgressiveLevelProperty(m => m.Overflow, 35791);
            SetupProgressiveLevelProperty(p => p.CreationType, LevelCreationType.All);
        }

        private void SetupFactoryObjects(string meterNodeName, string meterNodeDisplayName)
        {
            _meterNode.Name = meterNodeName;
            _meterNode.DisplayName = meterNodeDisplayName;
        }

        private void SetupProgressiveIMeter(long meterValue, bool meterShouldExist, bool currencyClassNeeded)
        {
            if (currencyClassNeeded)
            {
                _progressiveMeter.SetupGet(p => p.Classification).Returns(new CurrencyMeterClassification());
            }
            else
            {
                _progressiveMeter.SetupGet(p => p.Classification).Returns(new OccurrenceMeterClassification());
            }

            _progressiveMeter.SetupGet(p => p.Period).Returns(meterValue);
            _progressiveMeter.SetupGet(p => p.Lifetime).Returns(meterValue);
            _progressiveMeter.SetupGet(p => p.Session).Returns(meterValue);
            _progressiveMeterManager.Setup(p => p.IsMeterProvided(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(meterShouldExist);
            _progressiveMeterManager.Setup(p => p.GetMeter(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(_progressiveMeter.Object);
            _progressiveMeterManager.Setup(p => p.IsMeterProvided(It.IsAny<int>(), It.IsAny<string>())).Returns(meterShouldExist);
            _progressiveMeterManager.Setup(p => p.GetMeter(It.IsAny<int>(), It.IsAny<string>())).Returns(_progressiveMeter.Object);
        }
    }
}
