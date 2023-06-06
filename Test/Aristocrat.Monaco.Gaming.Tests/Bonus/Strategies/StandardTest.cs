using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.Bonus;
using Aristocrat.Monaco.Gaming.Contracts.Meters;
using Aristocrat.Monaco.Gaming.Contracts.Payment;
using Aristocrat.Monaco.Gaming.Contracts.Session;
using Aristocrat.Monaco.Gaming.Runtime;
using Aristocrat.Monaco.Gaming.UI;
using Aristocrat.Monaco.Hardware.Contracts.Door;
using Aristocrat.Monaco.Hardware.Contracts.Persistence;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aristocrat.Monaco.Gaming.Tests.Bonus.Strategies
{
    [TestClass]
    public class StandardTest
    {
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IDoorService> _doorService;
        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IBank> _bank;
        private Mock<ITransferOutHandler> _transferOutHandler;
        private readonly Mock<IPaymentDeterminationProvider> _largeWinDetermination = new Mock<IPaymentDeterminationProvider>();
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<ITransactionHistory> _transactionHistory;
        private readonly Mock<IPlayerService> _players = new Mock<IPlayerService>();
        private Mock<IRuntime> _runtime;
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>();
        private readonly Mock<IGameMeterManager> _meterManager = new Mock<IGameMeterManager>(MockBehavior.Default);

        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<IMaxWinOverlayService> _maxWinOverlayService = new Mock<IMaxWinOverlayService>();
        private Gaming.Bonus.Strategies.Standard _standard;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Loose);

            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);

            _persistentStorageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _persistentStorageManager.Setup(m => m.ScopedTransaction()).Returns(_scopedTransaction.Object);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, It.IsAny<object>())).Returns(0);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<object>())).Returns(0L);

            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(m => m.Publish(It.IsAny<BonusPendingEvent>()));

            _doorService = MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Strict);
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);

            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Strict);
            _transferOutHandler = MoqServiceManager.CreateAndAddService<ITransferOutHandler>(MockBehavior.Default);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _runtime = new Mock<IRuntime>(MockBehavior.Default);

            _eventBus.Setup(m => m.Publish(It.IsAny<BonusFailedEvent>()));

            _standard = new Gaming.Bonus.Strategies.Standard(_persistentStorageManager.Object,
                 _transactionHistory.Object, _gameHistory.Object,
                 _gamePlayState.Object, _meterManager.Object, _runtime.Object, _eventBus.Object,
                 _propertiesManager.Object, _bank.Object,
                 _transferOutHandler.Object,
                 _messageDisplay.Object, _players.Object, _systemDisableManager.Object,
                 _largeWinDetermination.Object,
                 _maxWinOverlayService.Object);

        }

        [TestMethod]
        public void CreateTransactionTest()
        {
            var request = GetBonusRequest();

            var tx = CreateTransactionCore(request, _standard, false, false);
            Assert.AreEqual(tx.Exception, (int)BonusException.Failed);

            tx = CreateTransactionCore(request, _standard, true, true);
            Assert.AreEqual(tx.Exception, (int)BonusException.None);

            tx = CreateTransactionCore(request, _standard, false, true);
            Assert.AreEqual(tx.Exception, (int)BonusException.None);

            tx = CreateTransactionCore(request, _standard, true, false);
            Assert.AreEqual(tx.Exception, (int)BonusException.Failed);

            StandardBonus GetBonusRequest() => new StandardBonus(string.Empty, 0, 0, 0, PayMethod.Any);
        }

        private BonusTransaction CreateTransactionCore(StandardBonus request, Gaming.Bonus.Strategies.Standard standard, bool isDoorOpen, bool isGameRunning)
        {
            _doorService.Setup(m => m.GetDoorOpen(It.IsAny<int>())).Returns(isDoorOpen);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(isGameRunning);
            return standard.CreateTransaction(0, request);
        }
    }
}
