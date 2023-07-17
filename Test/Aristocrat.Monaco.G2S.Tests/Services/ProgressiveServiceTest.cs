namespace Aristocrat.Monaco.G2S.Tests.Services
{
    using System;
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
    using Aristocrat.Monaco.G2S.DisableProvider;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.G2S.Services.Progressive;

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
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>> _progressiveStatusBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveStatus>>();
        private readonly Mock<IGameHistory> _gameHistoryMock = new Mock<IGameHistory>();
        private readonly Mock<ITransactionHistory> _transactionHistoryMock = new Mock<ITransactionHistory>();
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
        private readonly Mock<IG2SDisableProvider> _disableProviderMock = new Mock<IG2SDisableProvider>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<IProgressiveLevelManager> _progressiveLevelManager = new Mock<IProgressiveLevelManager>();
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveHit>> _progressiveHitBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveHit>>();
        private readonly Mock<ICommandBuilder<IProgressiveDevice, progressiveCommit>> _progressiveCommitBuilderMock = new Mock<ICommandBuilder<IProgressiveDevice, progressiveCommit>>();
        private readonly Mock<IProtocolProgressiveEventsRegistry> _protocolProgressiveEventsRegistryMock = new Mock<IProtocolProgressiveEventsRegistry>();
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
                _gameHistoryMock.Object,
                _transactionHistoryMock.Object,
                _unitOfWorkFactoryMock.Object,
                _disableProviderMock.Object,
                _propertiesManager.Object,
                _progressiveLevelManager.Object,
                _commandBuilderMock.Object,
                _progressiveStatusBuilderMock.Object,
                _progressiveHitBuilderMock.Object,
                _progressiveCommitBuilderMock.Object
                );
        }
    }
}