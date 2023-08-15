﻿namespace Aristocrat.Monaco.Bingo.Tests.Services.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Bingo.Services.Progressives;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Common;

    [TestClass]
    public class ProgressiveHandlerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter = new(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new(MockBehavior.Default);
        private readonly Mock<IProgressiveLevelInfoProvider> _progressiveLevelInfoProvider = new(MockBehavior.Default);

        private ProgressiveHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false, DisplayName="Null IEventBus")]
        [DataRow(false, true, false, false, DisplayName= "Null IProtocolLinkedProgressiveAdapter")]
        [DataRow(false, false, true, false, DisplayName = "Null ISystemDisableManager")]
        [DataRow(false, false, false, true, DisplayName = "Null IProgressiveLevelInfoProvider")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(
            bool nullEventBus,
            bool nullProtocolLinkedProgressiveAdapter,
            bool nullSystemDisableManager,
            bool nullProgressiveLevelInfoProvider)
        {
            _target = CreateTarget(
                nullEventBus,
                nullProtocolLinkedProgressiveAdapter,
                nullSystemDisableManager,
                nullProgressiveLevelInfoProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProcessProgressiveInfoArgumentNull()
        {
            using var source = new CancellationTokenSource();
            var result = await _target.ProcessProgressiveInfo(null, source.Token);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ProcessProgressiveInfo()
        {
            const int gameTitle = 123;
            const long denomination = 25L;
            const string authToken = "ABC123";
            var metersToReport = new List<int> { 10, 100, 200 };
            using var source = new CancellationTokenSource();
            var progressiveLevels = new List<ProgressiveLevelInfo>
            {
                new ProgressiveLevelInfo(10001, 1, gameTitle, denomination),
                new ProgressiveLevelInfo(10002, 2, gameTitle, denomination),
                new ProgressiveLevelInfo(10003, 3, gameTitle, denomination),
            };

            var progressiveInfoMessage = new ProgressiveInfoMessage(ResponseCode.Ok, true, gameTitle, authToken, progressiveLevels, metersToReport);

            var result = await _target.ProcessProgressiveInfo(progressiveInfoMessage, source.Token);

            Assert.IsTrue(result);
            _eventBus.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProcessProgressiveUpdateArgumentNull()
        {
            using var source = new CancellationTokenSource();
            var result = await _target.ProcessProgressiveUpdate(null, source.Token);
        }

        [TestMethod]
        public async Task ProcessProgressiveUpdate()
        {
            const int gameTitle = 123;
            const long denomination = 25L;
            const long progressiveLevel = 10001;
            const long amount = 1000;
            using var source = new CancellationTokenSource();
            var progressiveUpdateMessage = new ProgressiveUpdateMessage(progressiveLevel, amount);

            // Viewable progressives use 0-based id values, not the one link id values
            var viewableProgressives = new List<IViewableProgressiveLevel>()
            {
                CreateViewableProgressive(0, 500),
                CreateViewableProgressive(1, 700),
                CreateViewableProgressive(2, 900),
            };

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels()).Returns(viewableProgressives);
            _protocolLinkedProgressiveAdapter.Setup(x => x.UpdateLinkedProgressiveLevels(It.IsAny<LinkedProgressiveLevel[]>(), ProtocolNames.Bingo));

            var mappedId = progressiveLevel;
            _progressiveLevelInfoProvider.Setup(x => x.GetServerProgressiveLevelId(It.IsAny<int>())).Returns(mappedId);

            // Must first call ProcessProgressiveInfo to set internal variables
            var metersToReport = new List<int> { 10, 100, 200 };
            var progressiveLevels = new List<ProgressiveLevelInfo>
            {
                new ProgressiveLevelInfo(10001, 1, gameTitle, denomination),
                new ProgressiveLevelInfo(10002, 2, gameTitle, denomination),
                new ProgressiveLevelInfo(10003, 3, gameTitle, denomination),
            };

            var progressiveInfoMessage = new ProgressiveInfoMessage(ResponseCode.Ok, true, 123, "ABC123", progressiveLevels, metersToReport);
            var result = await _target.ProcessProgressiveInfo(progressiveInfoMessage, source.Token);
            Assert.IsTrue(result);

            result = await _target.ProcessProgressiveUpdate(progressiveUpdateMessage, source.Token);

            Assert.IsTrue(result);
            _eventBus.Verify();
        }

        [TestMethod]
        public async Task ProcessProgressiveUpdateInvalidLevelIdTest()
        {
            const int gameTitle = 123;
            const long denomination = 25L;
            const long progressiveLevel = 10001;
            const long amount = 1000;
            using var source = new CancellationTokenSource();
            var progressiveUpdateMessage = new ProgressiveUpdateMessage(progressiveLevel, amount);

            // Viewable progressives use 0-based id values, not the one link id values
            var viewableProgressives = new List<IViewableProgressiveLevel>()
            {
                CreateViewableProgressive(0, 500),
                CreateViewableProgressive(1, 700),
                CreateViewableProgressive(2, 900),
            };

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels()).Returns(viewableProgressives);
            _progressiveLevelInfoProvider.Setup(x => x.GetServerProgressiveLevelId(It.IsAny<int>())).Returns(-1L);

            // Must first call ProcessProgressiveInfo to set internal variables
            var metersToReport = new List<int> { 10, 100, 200 };
            var progressiveLevels = new List<ProgressiveLevelInfo>
            {
                new ProgressiveLevelInfo(10001, 1, gameTitle, denomination),
                new ProgressiveLevelInfo(10002, 2, gameTitle, denomination),
                new ProgressiveLevelInfo(10003, 3, gameTitle, denomination),
            };

            var progressiveInfoMessage = new ProgressiveInfoMessage(ResponseCode.Ok, true, 123, "ABC123", progressiveLevels, metersToReport);
            var result = await _target.ProcessProgressiveInfo(progressiveInfoMessage, source.Token);
            Assert.IsTrue(result);

            result = await _target.ProcessProgressiveUpdate(progressiveUpdateMessage, source.Token);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ProcessProgressiveUpdateUnknownProgressive()
        {
            const int gameTitle = 123;
            const long denomination = 25L;
            const long progressiveLevel = 10005;
            const long amount = 1000;
            using var source = new CancellationTokenSource();
            var progressiveUpdateMessage = new ProgressiveUpdateMessage(progressiveLevel, amount);

            // Viewable progressives use 0-based id values, not the one link id values
            var viewableProgressives = new List<IViewableProgressiveLevel>()
            {
                CreateViewableProgressive(0, 500),
                CreateViewableProgressive(1, 700),
                CreateViewableProgressive(2, 900),
            };

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels()).Returns(viewableProgressives);
            _protocolLinkedProgressiveAdapter.Setup(x => x.UpdateLinkedProgressiveLevels(It.IsAny<LinkedProgressiveLevel[]>(), ProtocolNames.Bingo));

            // Must first call ProcessProgressiveInfo to set internal variables
            var metersToReport = new List<int> { 10, 100, 200 };
            var progressiveLevels = new List<ProgressiveLevelInfo>
            {
                new ProgressiveLevelInfo(10001, 1, gameTitle, denomination),
                new ProgressiveLevelInfo(10002, 2, gameTitle, denomination),
                new ProgressiveLevelInfo(10003, 3, gameTitle, denomination),
            };

            var progressiveInfoMessage = new ProgressiveInfoMessage(ResponseCode.Ok, true, 123, "ABC123", progressiveLevels, metersToReport);
            var result = await _target.ProcessProgressiveInfo(progressiveInfoMessage, source.Token);
            Assert.IsTrue(result);

            result = await _target.ProcessProgressiveUpdate(progressiveUpdateMessage, source.Token);

            Assert.IsFalse(result);
            _eventBus.Verify();
        }

        [TestMethod]
        public async Task ProcessProgressiveUpdateNoProgressiveInfoCall()
        {
            const long progressiveLevel = 10005;
            const long amount = 1000;
            using var source = new CancellationTokenSource();
            var progressiveUpdateMessage = new ProgressiveUpdateMessage(progressiveLevel, amount);

            // Viewable progressives use 0-based id values, not the one link id values
            var viewableProgressives = new List<IViewableProgressiveLevel>()
            {
                CreateViewableProgressive(0, 500),
                CreateViewableProgressive(1, 700),
                CreateViewableProgressive(2, 900),
            };

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels()).Returns(viewableProgressives);
            _protocolLinkedProgressiveAdapter.Setup(x => x.UpdateLinkedProgressiveLevels(It.IsAny<LinkedProgressiveLevel[]>(), ProtocolNames.Bingo));

            var result = await _target.ProcessProgressiveUpdate(progressiveUpdateMessage, source.Token);

            Assert.IsFalse(result);
            _eventBus.Verify();
        }

        [TestMethod]
        public async Task DisableByProgressive()
        {
            using var source = new CancellationTokenSource();
            var disableByProgressiveMessage = new DisableByProgressiveMessage(ResponseCode.Ok);

            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.BingoWinMismatchKey,
                SystemDisablePriority.Immediate,
                It.IsAny<Func<string>>(),
                null)).Verifiable();

            var result = await _target.DisableByProgressive(disableByProgressiveMessage, source.Token);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task EnableByProgressive()
        {
            using var source = new CancellationTokenSource();
            var enableByProgressiveMessage = new EnableByProgressiveMessage(ResponseCode.Ok);

            _systemDisableManager.Setup(m => m.Enable(
                BingoConstants.BingoWinMismatchKey)).Verifiable();

            var result = await _target.EnableByProgressive(enableByProgressiveMessage, source.Token);

            Assert.IsTrue(result);
        }

        private ProgressiveHandler CreateTarget(
            bool nullEventBus = false,
            bool nullProtocolLinkedProgressiveAdapter = false,
            bool nullSystemDisableManager = false,
            bool nullProgressiveLevelInfoProvider = false)
        {
            return new(
                nullEventBus ? null : _eventBus.Object,
                nullProtocolLinkedProgressiveAdapter ? null : _protocolLinkedProgressiveAdapter.Object,
                nullSystemDisableManager ? null : _systemDisableManager.Object,
                nullProgressiveLevelInfoProvider ? null : _progressiveLevelInfoProvider.Object);
        }

        private IViewableProgressiveLevel CreateViewableProgressive(int levelId, long amount)
        {
            return new ProgressiveLevel()
            {
                DeviceId = 1,
                ProgressivePackName = "TestPack",
                ProgressivePackId = 100,
                ProgressiveId = levelId,
                Denomination = new List<long> { 1, 2, 5 },
                BetOption = "2",
                Variation = "99",
                LevelType = ProgressiveLevelType.LP,
                LevelId = levelId,
                LevelName = "Test",
                IncrementRate = 3,
                MaximumValue = 10000,
                ResetValue = 1000,
                LevelRtp = 123,
                CurrentState = ProgressiveLevelState.Active,
                CurrentValue = amount,
                InitialValue = 250,
                GameId = 456,
                WagerCredits = 789
            };
        }
    }
}