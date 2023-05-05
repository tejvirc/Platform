namespace Aristocrat.Monaco.G2S.Tests.Services
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.G2S.Data.Model;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ProgressiveServiceTest
    {
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>> _commandBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();
        private readonly Mock<IEventBus> _eventBusMock = new Mock<IEventBus>();
        private readonly Mock<IEventLift> _eventLiftMock = new Mock<IEventLift>();
        private readonly Mock<IGameProvider> _gameProviderMock = new Mock<IGameProvider>();
        private readonly Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapterMock = new Mock<IProtocolLinkedProgressiveAdapter>();
        private readonly Mock<IPersistentStorageManager> _storageMock = new Mock<IPersistentStorageManager>();
        private readonly Mock<IProgressiveMeterManager> _progressiveMetersMock = new Mock<IProgressiveMeterManager>();
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>> _progressiveStatusBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
        private readonly Mock<IGameHistory> _gameHistoryMock = new Mock<IGameHistory>();
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveHit>> _progressiveHitBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveHit>>();
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveCommit>> _progressiveCommitBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveCommit>>();
        private readonly Mock<IProtocolProgressiveEventsRegistry> _protocolProgressiveEventsRegistryMock = new Mock<IProtocolProgressiveEventsRegistry>();
        private readonly Mock<ConcurrentDictionary<string, IList<ProgressiveInfo>>> _progressivesMock = new Mock<ConcurrentDictionary<string, IList<ProgressiveInfo>>>();
        private readonly object _pendingAwardsLock = new object();
        private readonly Mock<IUnitOfWork> work = new Mock<IUnitOfWork>();

        private ProgressiveService CreateProgressiveService()
        {
            var repository = new Mock<Protocol.Common.Storage.Repositories.IRepository<PendingJackpotAwards>>();
            _unitOfWorkFactoryMock.Setup(m => m.Create()).Returns(work.Object);
            work.Setup(m => m.Repository<PendingJackpotAwards>()).Returns(repository.Object);
            Mock<IProgressiveDevice> deviceMock = new Mock<IProgressiveDevice>();
            _egmMock.Object.AddDevice(deviceMock.Object);


            return new ProgressiveService(
                _egmMock.Object,
                _eventLiftMock.Object,
                _eventBusMock.Object,
                _gameProviderMock.Object,
                _protocolProgressiveEventsRegistryMock.Object,
                _protocolLinkedProgressiveAdapterMock.Object,
                _storageMock.Object,
                _progressiveMetersMock.Object,
                _gameHistoryMock.Object,
                _unitOfWorkFactoryMock.Object,
                _commandBuilderMock.Object,
                _progressiveStatusBuilderMock.Object,
                _progressiveHitBuilderMock.Object,
                _progressiveCommitBuilderMock.Object
                );
        }

        [TestMethod]
        public void LevelMistmatchLockupTest()
        {
            var service = CreateProgressiveService();

            var devices = _egmMock.Object.GetDevices<IProgressiveDevice>();
            foreach (var device in devices)
            {
                service.LevelMismatchLockup(true, device);
                Assert.IsTrue(device.HostEnabled);
                service.LevelMismatchLockup(false, device);
                Assert.IsFalse(device.HostEnabled);
            }

        }

        [TestMethod]
        public void ProgressiveValueTimeoutLockupTest()
        {
            var service = CreateProgressiveService();

            var devices = _egmMock.Object.GetDevices<IProgressiveDevice>();
            foreach (var device in devices)
            {
                service.ProgressiveValueTimeoutLockup(true, device);
                Assert.IsTrue(device.HostEnabled);
                service.ProgressiveValueTimeoutLockup(false, device);
                Assert.IsFalse(device.HostEnabled);
            }
        }
    }
}