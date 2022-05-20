namespace Aristocrat.Monaco.Gaming.Tests
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using Aristocrat.Monaco.Test.Common;
    using Contracts;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Unit tests for PlayerBank
    /// </summary>
    [TestClass]
    public class PlayerBankTest
    {
        private readonly Mock<IPlayerService> _players = new Mock<IPlayerService>();
        private Mock<IBank> _bank;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IEventBus> _eventBus;
        private Mock<IGameHistory> _history;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _persistentStorage;

        private PlayerBank _target;
        private Mock<ITransactionCoordinator> _transactionCoordinator;

        private Mock<ILockManager> _lockManager;
        private Mock<IDisposable> _disposable;

        private Mock<ITransferOutHandler> _transferOutHandler;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank = new Mock<IBank>();
            _eventBus = new Mock<IEventBus>();
            _block = new Mock<IPersistentStorageAccessor>();
            _persistentStorage = new Mock<IPersistentStorageManager>();
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Transient, It.IsAny<string>(), 1))
                .Returns(_block.Object);
            _transactionCoordinator = new Mock<ITransactionCoordinator>();
            _transferOutHandler = new Mock<ITransferOutHandler>();
            _meterManager = new Mock<IMeterManager>();
            _history = new Mock<IGameHistory>();

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
            _lockManager = MoqServiceManager.CreateAndAddService<ILockManager>(MockBehavior.Default);
            _lockManager.Setup(l => l.AcquireExclusiveLock(It.IsAny<IEnumerable<ILockable>>())).Returns(_disposable.Object);

            _target = new PlayerBank(
                _bank.Object,
                _transactionCoordinator.Object,
                _transferOutHandler.Object,
                _persistentStorage.Object,
                _meterManager.Object,
                _players.Object,
                _eventBus.Object,
                _history.Object,
                _lockManager.Object);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
        }

        [TestMethod]
        public void BalanceWithBankTest()
        {
            long balance = 100;
            _bank.Setup(m => m.QueryBalance()).Returns(balance);

            Assert.AreEqual(balance, _target.Balance);
        }
    }
}