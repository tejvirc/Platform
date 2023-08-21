namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Handlers;

    /// <summary>
    ///     Contains the unit tests for the LP85SendSasProgressiveWinAmountHandlerTests class
    /// </summary>
    [TestClass]
    public class LP85SendSasProgressiveWinAmountHandlerTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IProgressiveHitExceptionProvider> _progressiveHits;
        private Mock<IPersistentStorageManager> _persistence;
        private Mock<IGamePlayState> _gamePlayState;
        private LP85SendSasProgressiveWinAmountHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>();
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            _progressiveHits = new Mock<IProgressiveHitExceptionProvider>();
            _persistence = new Mock<IPersistentStorageManager>();
            _gamePlayState = new Mock<IGamePlayState>();
            _target = new LP85SendSasProgressiveWinAmountHandler(
                _propertiesManager.Object,
                _protocolLinkedProgressiveAdapter.Object,
                _progressiveHits.Object,
                _gamePlayState.Object,
                _persistence.Object);

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendSasProgressiveWinAmount));
        }

        [TestMethod]
        public void HandleTestWithEmptyProgressiveWinQueue()
        {
            const int expectedGroupId = 1;

            var levels = new List<LinkedProgressiveLevel>
            {
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.None },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId
                },
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.Awarded },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId
                },
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.None },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId
                }
            };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = expectedGroupId });
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(levels);

            var response = _target.Handle(new LongPollData());
            Assert.AreEqual(0L, response.GroupId);
            Assert.AreEqual(0, response.LevelId);
            Assert.AreEqual(0L, response.WinAmount);
        }

        [TestMethod]
        public void HandleTestWithProgressiveWinQueue()
        {
            const int expectedGroupId = 1;
            var expectedLevel = new LinkedProgressiveLevel
            {
                Amount = 10000,
                ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.Hit, WinAmount = 10000 },
                ProtocolName = ProgressiveConstants.ProtocolName,
                ProgressiveGroupId = expectedGroupId,
                LevelId = 2
            };

            var levels = new List<LinkedProgressiveLevel>
            {
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.None },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId
                },
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.Awarded },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId
                },
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.None },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId
                },
                expectedLevel
            };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = expectedGroupId });
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(levels);

            var response = _target.Handle(new LongPollData());
            Assert.AreEqual(expectedGroupId, response.GroupId);
            Assert.AreEqual(expectedLevel.LevelId, response.LevelId);
            Assert.AreEqual(expectedLevel.ClaimStatus.WinAmount, response.WinAmount);
        }
    }
}