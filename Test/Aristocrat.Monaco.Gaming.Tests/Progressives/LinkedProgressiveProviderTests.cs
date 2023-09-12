namespace Aristocrat.Monaco.Gaming.Tests.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Gaming.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LinkedProgressiveProviderTests
    {
        private readonly List<LinkedProgressiveLevel> _testData = new List<LinkedProgressiveLevel>
        {
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 1,
                Amount = 100000,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(1).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 2,
                Amount = 9999,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(1).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 3,
                Amount = 12345,
                CurrentErrorStatus = ProgressiveErrors.ProgressiveRtpError,
                Expiration = DateTime.Now.AddMilliseconds(1).ToUniversalTime()
            }
        };

        private readonly List<LinkedProgressiveLevel> _updatedTestData = new List<LinkedProgressiveLevel>
        {
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 1,
                Amount = 100001,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 2,
                Amount = 10000,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 3,
                Amount = 12346,
                CurrentErrorStatus = ProgressiveErrors.ProgressiveRtpError,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            }
        };

        private readonly List<LinkedProgressiveLevel> _updatedTestDataIncludingNew = new List<LinkedProgressiveLevel>
        {
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 1,
                Amount = 100001,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 2,
                Amount = 10000,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 3,
                Amount = 12346,
                CurrentErrorStatus = ProgressiveErrors.ProgressiveRtpError,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            },
            new LinkedProgressiveLevel
            {
                ProtocolName = "SAS",
                ProgressiveGroupId = 1,
                LevelId = 4,
                Amount = 12346,
                CurrentErrorStatus = ProgressiveErrors.None,
                Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
            }
        };

        private ProgressiveBroadcastTimer _broadcastTimer;
        private LinkedProgressiveProvider _linkedProgressiveProvider;
        private Mock<IProgressiveMeterManager> _progressiveMeterManager;
        private Mock<IPersistenceProvider> _mockPersistenceProvider;
        private Mock<IPersistentBlock> _mockSaveBlock;
        private Mock<IPersistentTransaction> _mockSaveTransaction;
        private Mock<IEventBus> _mockEventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageManager> _persistenceProvider;
        private Mock<IScopedTransaction> _scopedTransaction;
        private bool _timerElapsed;

        [TestInitialize]
        public void Init()
        {
            _broadcastTimer = new ProgressiveBroadcastTimer();
            _progressiveMeterManager = new Mock<IProgressiveMeterManager>();
            _mockEventBus = new Mock<IEventBus>();
            _mockSaveTransaction = new Mock<IPersistentTransaction>();
            _propertiesManager = new Mock<IPropertiesManager>();
            _persistenceProvider = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);

            _scopedTransaction.Setup(x => x.Complete());
            _persistenceProvider.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);

            // Setup the save block so it can return a valid linked progressive index
            _mockSaveBlock = new Mock<IPersistentBlock>();
            _mockSaveBlock
                .Setup(
                    x => x
                        .GetOrCreateValue<
                            Dictionary<string, LinkedProgressiveLevel>>(
                            It.IsAny<string>()))
                .Returns(new Dictionary<string, LinkedProgressiveLevel>());
            _mockSaveBlock.Setup(x => x.Transaction()).Returns(_mockSaveTransaction.Object);

            // Setup the IPersistenceProvider
            _mockPersistenceProvider = new Mock<IPersistenceProvider>();
            _mockPersistenceProvider.Setup(
                x => x.GetOrCreateBlock(
                    It.IsAny<string>(),
                    PersistenceLevel.Static)).Returns(_mockSaveBlock.Object);

            _linkedProgressiveProvider = new LinkedProgressiveProvider(
                _mockEventBus.Object,
                _broadcastTimer,
                _progressiveMeterManager.Object,
                _mockPersistenceProvider.Object,
                _propertiesManager.Object,
                _persistenceProvider.Object);
            _broadcastTimer.Timer.Interval = 1;
            _broadcastTimer.Timer.AutoReset = false;
            _broadcastTimer.Timer.Stop();
            _broadcastTimer.Timer.Elapsed += TimerElapsedHandler;
            _timerElapsed = false;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _linkedProgressiveProvider.Dispose();
            _broadcastTimer.Dispose();
        }

        [TestMethod]
        public void AddLinkedProgressiveLevelsWithNoLevelsTest()
        {
            _linkedProgressiveProvider.AddLinkedProgressiveLevels(new List<LinkedProgressiveLevel>());
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddLinkedProgressiveLevelsWithNullLevelsTest()
        {
            _linkedProgressiveProvider.AddLinkedProgressiveLevels(null);
        }

        [TestMethod]
        public void AddLinkedProgressiveLevelsWithSeveralLevelsTest()
        {
            var addedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()))
                .Callback<LinkedProgressiveAddedEvent>(
                    e => addedEventData = e.LinkedProgressiveLevels.ToList());

            _linkedProgressiveProvider.AddLinkedProgressiveLevels(_testData);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()), Times.Once);

            Assert.AreEqual(3, addedEventData.Count);

            VerifyEventData(
                _testData, addedEventData, ProgressiveErrors.None);
        }

        [TestMethod]
        public void RemoveLinkedProgressiveLevelsWithNoLevelsTest()
        {
            _linkedProgressiveProvider.RemoveLinkedProgressiveLevels(new List<LinkedProgressiveLevel>());
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRemovedEvent>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveLinkedProgressiveLevelsWithNullLevelsTest()
        {
            _linkedProgressiveProvider.RemoveLinkedProgressiveLevels(null);
        }

        [TestMethod]
        public void RemoveLinkedProgressiveLevelsWithNoLevelsToRemoveTest()
        {
            // Attempt to remove with no existing levels
            var removedItems = new List<LinkedProgressiveLevel> { _testData[0] };
            _linkedProgressiveProvider.RemoveLinkedProgressiveLevels(removedItems);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRemovedEvent>()), Times.Never);
        }

        [TestMethod]
        public void RemoveLinkedProgressiveLevelsWithSeveralLevelsTest()
        {
            var removedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveRemovedEvent>()))
                .Callback<LinkedProgressiveRemovedEvent>(
                    e => removedEventData = e.LinkedProgressiveLevels.ToList());

            // Setup a level that can be removed
            _linkedProgressiveProvider.AddLinkedProgressiveLevels(_testData);

            // Remove the level
            var removedItems = new List<LinkedProgressiveLevel> { _testData[0] };
            _linkedProgressiveProvider.RemoveLinkedProgressiveLevels(removedItems);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRemovedEvent>()), Times.Once);

            Assert.AreEqual(1, removedEventData.Count);

            VerifyEventData(
                removedItems, removedEventData, ProgressiveErrors.None);
        }

        [TestMethod]
        public void UpdateLinkedProgressiveLevelsWithNoLevelsTest()
        {
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(new List<LinkedProgressiveLevel>());
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveUpdatedEvent>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateLinkedProgressiveLevelsWithNullLevelsTest()
        {
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(null);
        }

        [TestMethod]
        public void UpdateLinkedProgressiveLevelsWithNoExistingLevelsTest()
        {
            var addedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()))
                .Callback<LinkedProgressiveAddedEvent>(
                    e => addedEventData.AddRange(e.LinkedProgressiveLevels.ToList()));

            // Call update with no existing levels
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()), Times.Once);
            Assert.AreEqual(3, addedEventData.Count);

            VerifyEventData(_testData, addedEventData, ProgressiveErrors.None);
        }

        [TestMethod]
        public void UpdateLinkedProgressiveLevelsWithExistingLevelsTest()
        {
            var addedEventData = new List<IViewableLinkedProgressiveLevel>();
            var updatedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()))
                .Callback<LinkedProgressiveAddedEvent>(
                    e => addedEventData.AddRange(e.LinkedProgressiveLevels.ToList()));
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveUpdatedEvent>()))
                .Callback<LinkedProgressiveUpdatedEvent>(
                    e => updatedEventData.AddRange(e.LinkedProgressiveLevels.ToList()));

            // Call update with no existing levels
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Update it again with updated levels
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_updatedTestData);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()), Times.Once);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveUpdatedEvent>()), Times.Once);

            Assert.AreEqual(3, addedEventData.Count);
            Assert.AreEqual(3, updatedEventData.Count);

            VerifyEventData(_updatedTestData, updatedEventData, ProgressiveErrors.None);
        }

        [TestMethod]
        public void UpdateLinkedProgressiveLevelsWithExistingAndNewLevelsTest()
        {
            var addedEventData = new List<IViewableLinkedProgressiveLevel>();
            var updatedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()))
                .Callback<LinkedProgressiveAddedEvent>(
                    e => addedEventData.AddRange(e.LinkedProgressiveLevels.ToList()));
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveUpdatedEvent>()))
                .Callback<LinkedProgressiveUpdatedEvent>(
                    e => updatedEventData.AddRange(e.LinkedProgressiveLevels.ToList()));

            // Call update with no existing levels
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Update it again with updated levels
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_updatedTestDataIncludingNew);

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveAddedEvent>()), Times.Exactly(2));

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveUpdatedEvent>()), Times.Once);

            Assert.AreEqual(4, addedEventData.Count);
            Assert.AreEqual(3, updatedEventData.Count);

            VerifyEventData(_updatedTestData, updatedEventData, ProgressiveErrors.None);
        }

        [TestMethod]
        public void ReportLinkDownForNoExistingLevelsTest()
        {
            _linkedProgressiveProvider.ReportLinkDown(1);
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveDisconnectedEvent>()), Times.Never);
        }

        [TestMethod]
        public void ReportLinkDownForNonMatchingGroupIdLevelsTest()
        {
            // Setup test data
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Report link down for group id 2, which doesn't match test data
            _linkedProgressiveProvider.ReportLinkDown(2);

            // No event should be reported
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveDisconnectedEvent>()), Times.Never);
        }

        [TestMethod]
        public void ReportLinkDownForMatchingGroupIdLevelsAlreadyLinkDownTest()
        {
            // Setup test data
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Report link down twice
            _linkedProgressiveProvider.ReportLinkDown(1);
            _linkedProgressiveProvider.ReportLinkDown(1);

            // Only 1 event should be reported since we were already link down
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveDisconnectedEvent>()), Times.Once);
        }

        [TestMethod]
        public void ReportLinkDownForExistingMatchingLevelsTest()
        {
            var disconnectedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveDisconnectedEvent>()))
                .Callback<LinkedProgressiveDisconnectedEvent>(
                    e =>
                        disconnectedEventData = e.LinkedProgressiveLevels.OrderBy(x => x.LevelId).ToList());
            // Setup test data
            const int testGroupId = 1;
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Report link down for group id 1
            _linkedProgressiveProvider.ReportLinkDown(testGroupId);

            // Verify the disconnect events were reported
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveDisconnectedEvent>()), Times.Once);
            Assert.AreEqual(3, disconnectedEventData.Count);

            VerifyEventData(
                _testData,
                disconnectedEventData,
                ProgressiveErrors.ProgressiveDisconnected);
        }

        [TestMethod]
        public void ReportLinkUpWithNoExistingLevelsTest()
        {
            _linkedProgressiveProvider.ReportLinkDown(1);
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRefreshedEvent>()), Times.Never);
        }

        [TestMethod]
        public void ReportLinkUpForMatchingGroupIdLevelsAlreadyLinkDownTest()
        {
            // Setup test data
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Report link up when we are already link up
            _linkedProgressiveProvider.ReportLinkUp(1);

            // 0 events should be reported since we are already link up
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveConnectedEvent>()), Times.Never);
        }

        [TestMethod]
        public void ReportLinkUpForNonMatchingLevelsTest()
        {
            // Setup test data
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            // Report link down for group id 2, which doesn't match test data
            _linkedProgressiveProvider.ReportLinkUp(2);

            // No event should be reported
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveConnectedEvent>()), Times.Never);
        }

        [TestMethod]
        public void ReportLinkUpForExistingMatchingLevelsTest()
        {
            var connectedEventData = new List<IViewableLinkedProgressiveLevel>();
            _mockEventBus.Setup(x => x.Publish(It.IsAny<LinkedProgressiveConnectedEvent>()))
                .Callback<LinkedProgressiveConnectedEvent>(
                    e =>
                        connectedEventData = e.LinkedProgressiveLevels.OrderBy(x => x.LevelId).ToList());

            // Setup test data
            const int testGroupId = 1;
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);
            _linkedProgressiveProvider.ReportLinkDown(testGroupId);

            // Report the link is back up
            _linkedProgressiveProvider.ReportLinkUp(testGroupId);

            // Verify the connect events were reported
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveConnectedEvent>()), Times.Once);
            Assert.AreEqual(3, connectedEventData.Count);

            VerifyEventData(_testData, connectedEventData, ProgressiveErrors.None);
        }

        [TestMethod]
        [Timeout(5000)] // Should never take this long
        public void CheckForExpiredBroadcastsWithNoLevelsTest()
        {
            _broadcastTimer.Timer.Start();

            WaitForBroadcastTimerToElapse();

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveExpiredEvent>()), Times.Never);
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRefreshedEvent>()), Times.Never);
        }

        [TestMethod]
        [Timeout(5000)] // Should never take this long
        public void CheckForExpiredBroadcastsWithExpiredLevelsTest()
        {
            // Setup with already expired data
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            _broadcastTimer.Timer.Start();

            WaitForBroadcastTimerToElapse();

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveExpiredEvent>()), Times.Once);
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRefreshedEvent>()), Times.Never);
        }

        [TestMethod]
        [Timeout(5000)] // Should never take this long
        public void CheckForRefreshedBroadcastsTest()
        {
            // Setup with expired data
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(_testData);

            _broadcastTimer.Timer.Start();

            WaitForBroadcastTimerToElapse();

            // Now that the levels are expired, we can update with refreshed levels
            _linkedProgressiveProvider.UpdateLinkedProgressiveLevels(UpdateLevels(_testData));

            _broadcastTimer.Timer.Start();

            WaitForBroadcastTimerToElapse();

            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveExpiredEvent>()), Times.Once);
            _mockEventBus.Verify(
                x => x.Publish(It.IsAny<LinkedProgressiveRefreshedEvent>()), Times.Once);
        }

        private List<LinkedProgressiveLevel> UpdateLevels(IList<LinkedProgressiveLevel> levelData)
        {
            var refreshedLevels = new List<LinkedProgressiveLevel>();

            foreach (var level in levelData)
            {
                refreshedLevels.Add(
                    new LinkedProgressiveLevel
                    {
                        ProtocolName = level.ProtocolName,
                        ProgressiveGroupId = level.ProgressiveGroupId,
                        LevelId = level.LevelId,
                        Amount = level.Amount + 10,
                        CurrentErrorStatus = ProgressiveErrors.None,
                        Expiration = DateTime.Now.AddMilliseconds(5000).ToUniversalTime()
                    });
            }

            return refreshedLevels;
        }

        private void TimerElapsedHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerElapsed = true;
        }

        private void WaitForBroadcastTimerToElapse()
        {
            while (!_timerElapsed) { /* wait for timer to elapse*/ }

            _timerElapsed = false;
        }

        private void VerifyEventData(
            List<LinkedProgressiveLevel> expectedData,
            List<IViewableLinkedProgressiveLevel> actualData,
            ProgressiveErrors expectedStatus)
        {
            for (var i = 0; i < actualData.Count; i++)
            {
                Assert.AreEqual(expectedData[i].ProtocolName, actualData[i].ProtocolName, "Invalid ProtocolName");
                Assert.AreEqual(expectedData[i].ProgressiveGroupId, actualData[i].ProgressiveGroupId, "Invalid ProgressiveGroupId");
                Assert.AreEqual(expectedData[i].LevelId, actualData[i].LevelId, "Invalid LevelId");
                Assert.AreEqual(expectedData[i].LevelName, actualData[i].LevelName, "Invalid LevelName");
                Assert.AreEqual(expectedData[i].Amount, actualData[i].Amount, "Invalid Amount");
                Assert.AreEqual(expectedData[i].Expiration, actualData[i].Expiration, "Invalid Expiration");
                Assert.AreEqual(expectedStatus, actualData[i].CurrentErrorStatus, "Invalid Status");
            }
        }
    }
}