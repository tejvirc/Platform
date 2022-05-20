namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
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
    ///     Contains the unit tests for the LP87SendMultipleSASProgressiveWinAmountsHandlerTests class
    /// </summary>
    [TestClass]
    public class LP87SendMultipleSAsProgressiveWinAmountsHandlerTests
    {
        private readonly Mock<IPropertiesManager>
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);

        private readonly Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter =
            new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);

        private readonly Mock<IPersistentStorageManager> _persistence =
            new Mock<IPersistentStorageManager>(MockBehavior.Default);

        private readonly Mock<IProgressiveHitExceptionProvider> _exceptionProvider =
            new Mock<IProgressiveHitExceptionProvider>(MockBehavior.Default);

        private readonly Mock<IGamePlayState> _gamePlayState =
            new Mock<IGamePlayState>(MockBehavior.Default);

        private LP87SendMultipleSasProgressiveWinAmountsHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP87SendMultipleSasProgressiveWinAmountsHandler(
                _propertiesManager.Object,
                _gamePlayState.Object,
                _protocolLinkedProgressiveAdapter.Object,
                _persistence.Object,
                _exceptionProvider.Object);

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
        }

        [TestMethod]
        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest(
            bool nullProperties,
            bool nullGamePlay,
            bool nullLinkedProvider,
            bool nullPersistence,
            bool nullExceptionProvider)
        {
            _target = new LP87SendMultipleSasProgressiveWinAmountsHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullGamePlay ? null : _gamePlayState.Object,
                nullLinkedProvider ? null : _protocolLinkedProgressiveAdapter.Object,
                nullPersistence ? null : _persistence.Object,
                nullExceptionProvider ? null : _exceptionProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendMultipleSasProgressiveWinAmounts));
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
            Assert.AreEqual(expectedGroupId, response.GroupId);
            Assert.AreEqual(0, response.ProgressivesWon.Count);
        }

        [TestMethod]
        public void HandleTestWithProgressiveWinQueue()
        {
            const int expectedGroupId = 1;
            var expectedLevels = new List<LinkedProgressiveLevel>
            {
                new LinkedProgressiveLevel
                {
                    Amount = 10000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.Hit, WinAmount = 10005 },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId,
                    LevelId = 2
                },
                new LinkedProgressiveLevel
                {
                    Amount = 40000,
                    ClaimStatus = new LinkedProgressiveClaimStatus { Status = LinkedClaimState.Hit, WinAmount = 40003 },
                    ProtocolName = ProgressiveConstants.ProtocolName,
                    ProgressiveGroupId = expectedGroupId,
                    LevelId =4
                }
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
                }
            };

            levels.AddRange(expectedLevels);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = expectedGroupId });
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevels()).Returns(levels);

            var response = _target.Handle(new LongPollData());
            Assert.AreEqual(expectedGroupId, response.GroupId);
            Assert.AreEqual(expectedLevels.Count, response.ProgressivesWon.Count);
            CollectionAssert.AreEqual(
                expectedLevels.Select(x => new LinkedProgressiveWinData(x.LevelId, x.ClaimStatus.WinAmount, x.LevelName)).ToList(),
                response.ProgressivesWon.ToList(),
                new LinkedProgressiveWinDataComparer());
        }

        private class LinkedProgressiveWinDataComparer : IComparer<LinkedProgressiveWinData>, IComparer
        {
            public int Compare(LinkedProgressiveWinData left, LinkedProgressiveWinData right)
            {
                if (ReferenceEquals(left, right))
                {
                    return 0;
                }

                if (left is null || right is null)
                {
                    return -1;
                }

                return left.LevelId.CompareTo(right.LevelId) + left.Amount.CompareTo(right.Amount) +
                       string.CompareOrdinal(left.LevelName, right.LevelName);
            }

            public int Compare(object left, object right)
            {
                if (left is LinkedProgressiveWinData leftWin && right is LinkedProgressiveWinData rightWin)
                {
                    return Compare(leftWin, rightWin);
                }

                return -1;
            }
        }
    }
}