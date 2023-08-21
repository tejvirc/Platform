namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Handlers;
    using Test.Common;

    [TestClass]
    public class LP7AExtendedProgressiveBroadcastHandlerTests
    {
        private LP7AExtendedProgressiveBroadcastHandler _target;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            _propertiesManager = new Mock<IPropertiesManager>();

            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.EnhancedProgressiveDataReportingKey, It.IsAny<bool>()))
                .Returns(true);
            _target = new LP7AExtendedProgressiveBroadcastHandler(
                _protocolLinkedProgressiveAdapter.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.ExtendedProgressiveBroadcast));
        }

        [TestMethod]
        public void HandleTest()
        {
            var data = new ExtendedProgressiveBroadcastData
            {
                ProgId = 1,
                LevelInfo = new Dictionary<int, ExtendedLevelData>
                {
                    { 1, new ExtendedLevelData { ResetValue = 500, ContributionRate = 1000, Amount = 600L } },
                    { 2, new ExtendedLevelData { ResetValue = 400, ContributionRate = 1500, Amount = 500L } }
                }
            };

            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.EnhancedProgressiveDataReportingKey, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = 1 });

            var actualValues = new List<IViewableLinkedProgressiveLevel>();
            _protocolLinkedProgressiveAdapter
                .Setup(m => m.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(), It.IsAny<string>()))
                .Callback<IEnumerable<IViewableLinkedProgressiveLevel>, string>((l, p) => actualValues = l.ToList());

            var result = _target.Handle(data);

            Assert.AreEqual(2, actualValues.Count);
            Assert.AreEqual(ProgressiveConstants.ProtocolName, actualValues[0].ProtocolName);
            Assert.AreEqual(1, actualValues[0].ProgressiveGroupId);
            Assert.AreEqual(1, actualValues[0].LevelId);
            Assert.AreEqual(600L, actualValues[0].Amount);
            Assert.AreEqual(ProgressiveConstants.ProtocolName, actualValues[1].ProtocolName);
            Assert.AreEqual(1, actualValues[1].ProgressiveGroupId);
            Assert.AreEqual(2, actualValues[1].LevelId);
            Assert.AreEqual(500L, actualValues[1].Amount);
            Assert.IsTrue(result.Data);
        }

        [TestMethod]
        public void HandleTestWithInvalidGroupId()
        {
            var data = new ExtendedProgressiveBroadcastData
            {
                ProgId = 2,
                LevelInfo = new Dictionary<int, ExtendedLevelData>
                {
                    { 1, new ExtendedLevelData { ResetValue = 500, ContributionRate = 1000, Amount = 600L } },
                    { 2, new ExtendedLevelData { ResetValue = 400, ContributionRate = 1500, Amount = 500L } }
                }
            };

            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.EnhancedProgressiveDataReportingKey, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = 1 });

            var result = _target.Handle(data);
            _protocolLinkedProgressiveAdapter.Verify(
                x => x.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(), ProgressiveConstants.ProtocolName),
                Times.Never);
            Assert.IsTrue(result.Data);
        }

        [TestMethod]
        public void NoEnhancedProgressiveDataSupport()
        {
            var data = new ExtendedProgressiveBroadcastData
            {
                ProgId = 2,
                LevelInfo = new Dictionary<int, ExtendedLevelData>
                {
                    { 1, new ExtendedLevelData { ResetValue = 500, ContributionRate = 1000, Amount = 600L } },
                    { 2, new ExtendedLevelData { ResetValue = 400, ContributionRate = 1500, Amount = 500L } }
                }
            };

            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.EnhancedProgressiveDataReportingKey, It.IsAny<bool>()))
                .Returns(false);
            var result = _target.Handle(data);
            _protocolLinkedProgressiveAdapter.Verify(
                x => x.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(), ProgressiveConstants.ProtocolName),
                Times.Never);
            Assert.IsFalse(result.Data);
        }
    }
}