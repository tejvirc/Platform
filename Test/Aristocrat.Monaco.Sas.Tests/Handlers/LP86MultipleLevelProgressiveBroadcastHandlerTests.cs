namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Handlers;

    [TestClass]
    public class LP86MultipleLevelProgressiveBroadcastHandlerTests
    {
        private LP86MultipleLevelProgressiveBroadcastHandler _target;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IPropertiesManager> _propertiesProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            _propertiesProvider = new Mock<IPropertiesManager>();
            _target = new LP86MultipleLevelProgressiveBroadcastHandler(
                _protocolLinkedProgressiveAdapter.Object,
                _propertiesProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.MultipleLevelProgressiveBroadcastValues));
        }

        [TestMethod]
        public void HandleTest()
        {
            var data = new MultipleLevelProgressiveBroadcastData
            {
                ProgId = 1, LevelInfo = new Dictionary<int, long> { { 1, 600 }, { 2, 500 } }
            };

            _propertiesProvider.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
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
            Assert.AreEqual(ProgressiveErrors.None, actualValues[0].CurrentErrorStatus);
            Assert.AreEqual(ProgressiveConstants.ProtocolName, actualValues[1].ProtocolName);
            Assert.AreEqual(1, actualValues[1].ProgressiveGroupId);
            Assert.AreEqual(2, actualValues[1].LevelId);
            Assert.AreEqual(500L, actualValues[1].Amount);
            Assert.AreEqual(ProgressiveErrors.None, actualValues[1].CurrentErrorStatus);
            Assert.AreEqual(true, result.Data);
        }

        [TestMethod]
        public void HandleTestWithInvalidProgressiveGroupId()
        {
            var data = new MultipleLevelProgressiveBroadcastData
            {
                ProgId = 2, LevelInfo = new Dictionary<int, long> { { 1, 600 }, { 2, 500 } }
            };

            _propertiesProvider.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = 1 });

            _target.Handle(data);

            _protocolLinkedProgressiveAdapter.Verify(
                x => x.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(), ProgressiveConstants.ProtocolName),
                Times.Never);
        }
    }
}