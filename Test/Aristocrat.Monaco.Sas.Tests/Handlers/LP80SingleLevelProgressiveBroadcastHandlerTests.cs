namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Handlers;

    [TestClass]
    public class LP80SingleLevelProgressiveBroadcastHandlerTests
    {
        private LP80SingleLevelProgressiveBroadcastHandler _target;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            _propertiesManager = new Mock<IPropertiesManager>();
            _target = new LP80SingleLevelProgressiveBroadcastHandler(
                _protocolLinkedProgressiveAdapter.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        public void HandleLevelUpdatePassTest()
        {
            var data = new SingleLevelProgressiveBroadcastData { ProgId = 1, LevelId = 2, LevelAmount = 600 };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = 1 });

            var actualValues = new List<IViewableLinkedProgressiveLevel>();
            _protocolLinkedProgressiveAdapter.Setup(
                    m => m.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(), It.IsAny<string>()))
                .Callback<IEnumerable<IViewableLinkedProgressiveLevel>, string>((l, p) => actualValues = l.ToList());

            var result = _target.Handle(data);

            Assert.AreEqual(1, actualValues.Count);
            Assert.AreEqual(ProgressiveConstants.ProtocolName, actualValues[0].ProtocolName);
            Assert.AreEqual(1, actualValues[0].ProgressiveGroupId);
            Assert.AreEqual(2, actualValues[0].LevelId);
            Assert.AreEqual(600L, actualValues[0].Amount);
            Assert.AreEqual(true, result.Data);
        }

        [TestMethod]
        public void HandleTestWithInvalidProgressiveGroupId()
        {
            var data = new SingleLevelProgressiveBroadcastData { ProgId = 2, LevelId = 2, LevelAmount = 600 };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = 1 });

            _target.Handle(data);

            _protocolLinkedProgressiveAdapter.Verify(
                x => x.UpdateLinkedProgressiveLevelsAsync(It.IsAny<IReadOnlyCollection<LinkedProgressiveLevel>>(), ProgressiveConstants.ProtocolName),
                Times.Never);
        }
    }
}