namespace Aristocrat.Monaco.HHR.UI.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Hhr.Events;
    using Hhr.UI.Consumers;
    using Hhr.UI.Menu;
    using Hhr.UI.Services;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CurrentProgressivesLauncherTests
    {
        public enum EventType
        {
            CreditIn,
            VoucherRedeemed,
            WatOn,
            Other
        }

        private const string ProtocolName = @"HHR";
        private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(2);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);

        private readonly Mock<ISystemDisableManager> _disableManager =
            new Mock<ISystemDisableManager>(MockBehavior.Strict);

        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>(MockBehavior.Strict);
        private readonly Mock<IMenuAccessService> _menu = new Mock<IMenuAccessService>(MockBehavior.Strict);
        private readonly List<ProgressiveLevel> _progressiveLevels = new List<ProgressiveLevel>();

        private readonly Mock<IPropertiesManager>
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        private readonly Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter =
            new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);

        private readonly Mock<ITransactionCoordinator> _transactionCoordinator =
            new Mock<ITransactionCoordinator>(MockBehavior.Strict);

        private Action<BankBalanceChangedEvent> _bankBalanceChangedEvent;

        private Func<CurrencyInCompletedEvent, CancellationToken, Task> _currencyInCompletedEvent;
        private Func<ProgressivesActivatedEvent, CancellationToken, Task> _progressivesActivatedEventHandler;
        private Func<SystemEnabledEvent, CancellationToken, Task> _systemEnabledEventHandler;
        private Func<VoucherRedeemedEvent, CancellationToken, Task> _voucherRedeemedEvent;
        private Func<WatOnCompleteEvent, CancellationToken, Task> _watOnCompleteEvent;
        private Func<BonusAwardedEvent, CancellationToken, Task> _bonusAwardEvent;

        [TestInitialize]
        public void TestInitialization()
        {
            _eventBus.Setup(m =>
                    m.Subscribe(It.IsAny<CurrentProgressivesLauncher>(), It.IsAny<Func<CurrencyInCompletedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<CurrencyInCompletedEvent, CancellationToken, Task>>(
                    (subscriber, callback) => _currencyInCompletedEvent = callback);

            _eventBus.Setup(m =>
                    m.Subscribe(It.IsAny<CurrentProgressivesLauncher>(), It.IsAny<Func<VoucherRedeemedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<VoucherRedeemedEvent, CancellationToken, Task>>(
                    (subscriber, callback) => _voucherRedeemedEvent = callback);

            _eventBus.Setup(m =>
                    m.Subscribe(It.IsAny<CurrentProgressivesLauncher>(), It.IsAny<Func<WatOnCompleteEvent, CancellationToken, Task>>()))
                .Callback<object, Func<WatOnCompleteEvent, CancellationToken, Task>>(
                    (subscriber, callback) => _watOnCompleteEvent = callback);

            _eventBus.Setup(m =>
                    m.Subscribe(It.IsAny<CurrentProgressivesLauncher>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback<object, Action<BankBalanceChangedEvent>>(
                    (subscriber, callback) => _bankBalanceChangedEvent = callback);

            _eventBus.Setup(m =>
                    m.Subscribe(It.IsAny<CurrentProgressivesLauncher>(), It.IsAny<Func<BonusAwardedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<BonusAwardedEvent, CancellationToken, Task>>(
                    (subscriber, callback) => _bonusAwardEvent = callback);

            _transactionCoordinator.Setup(x => x.VerifyCurrentTransaction(It.IsAny<Guid>())).Returns(false);

            _menu.Setup(x => x.Show(It.IsAny<Command>())).Returns(Task.CompletedTask);
        }

        [DataRow(true, false, false, false, false, false, false,false, DisplayName = "Null EventBus")]
        [DataRow(false, true, false, false, false, false, false,false, DisplayName = "Null Bank")]
        [DataRow(false, false, true, false, false, false, false,false, DisplayName = "Null GameHistory")]
        [DataRow(false, false, false, true, false, false, false,false, DisplayName = "Null SystemDisableManager")]
        [DataRow(false, false, false, false, true, false, false,false, DisplayName = "Null MenuAccessService")]
        [DataRow(false, false, false, false, false, true, false,false, DisplayName = "Null PropertiesManager")]
        [DataRow(false, false, false, false, false, false, true, false, DisplayName = "Null ProtocolLinkedProgressiveAdapter")]
        [DataRow(false, false, false, false, false, false, true, false, DisplayName = "Null TransactionCoordinator")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(
            bool nullEventBus,
            bool nullBank,
            bool nullGameHistory,
            bool nullDisableManager,
            bool nullMenuAccessService,
            bool nullPropertiesManager,
            bool nullProtocolLinkedProgressiveAdapter,
            bool nullTransactionCoordinator)
        {
            _ = ConstructCurrentProgressivesLauncher(nullEventBus, nullBank, nullGameHistory, nullDisableManager,
                nullMenuAccessService, nullPropertiesManager, nullProtocolLinkedProgressiveAdapter,
                nullTransactionCoordinator);
        }

        [TestMethod]
        public void SetupProgressiveScreenLaunch_WhenBalanceZeroAndNotInRecovery_ExpectEventsNotSubscribed()
        {
            SetupShouldNotSubscribeEvents();
            _bank.Setup(b => b.QueryBalance()).Returns(0);
            _gameHistory.Setup(g => g.IsRecoveryNeeded).Returns(false);

            ConstructCurrentProgressivesLauncher();

            _eventBus.Verify();
        }

        [TestMethod]
        public void SetupProgressiveScreenLaunch_WhenBalanceNotZeroAndNotInRecovery_ExpectEventsSubscribed()
        {
            SetupShouldSubscribeEvents();
            _bank.Setup(b => b.QueryBalance()).Returns(10);
            _gameHistory.Setup(g => g.IsRecoveryNeeded).Returns(false);
            _ = ConstructCurrentProgressivesLauncher();

            _eventBus.Verify();
        }

        [TestMethod]
        public void SetupProgressiveScreenLaunch_WhenBalanceNonZeroButInRecovery_ExpectEventsNotSubscribed()
        {
            SetupShouldNotSubscribeEvents();
            _bank.Setup(b => b.QueryBalance()).Returns(10);
            _gameHistory.Setup(g => g.IsRecoveryNeeded).Returns(true);

            _ = ConstructCurrentProgressivesLauncher();

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task GetSystemEnabledEvent_WhenProtocolNotInitialized_ExpectScreenNotShown()
        {
            SetupShouldSubscribeEvents();
            _menu.Setup(m => m.Show(Command.CurrentProgressiveMoneyIn))
                .Throws(new Exception("Shouldn't now display progressive screen"));

            ConstructCurrentProgressivesLauncher();

            Assert.IsNotNull(_systemEnabledEventHandler);
            await _systemEnabledEventHandler.Invoke(new SystemEnabledEvent(), default);

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task GetProgressivesActivatedEvent_WhenSystemDisabled_ExpectScreenNotShown()
        {
            SetupShouldSubscribeEvents();
            _disableManager.Setup(d => d.IsDisabled).Returns(true);

            _menu.Setup(m => m.Show(Command.CurrentProgressiveMoneyIn))
                .Throws(new Exception("Shouldn't now display progressive screen"));

            ConstructCurrentProgressivesLauncher();

            Assert.IsNotNull(_progressivesActivatedEventHandler);
            await _progressivesActivatedEventHandler.Invoke(new ProgressivesActivatedEvent(), default);

            _eventBus.Verify();
        }

        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.All, ProgressivePoolCreation.WagerBased)]
        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.Default,
            ProgressivePoolCreation.WagerBased)]
        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.Default, ProgressivePoolCreation.Default)]
        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.All, ProgressivePoolCreation.Default)]
        [DataTestMethod]
        public async Task GetSystemEnabledEvent_WhenProgressivesAlreadyActivated_ExpectShowCalledIfConditionsMet(int levelsToCreate,
            int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int wager,
            LevelCreationType levelCreationType = LevelCreationType.Default,
            ProgressivePoolCreation progressivePoolCreation = ProgressivePoolCreation.WagerBased)
        {
            var waiter = new ManualResetEvent(false);

            SetupShouldSubscribeEvents();
            SetupProgressivePoolCreationType(progressivePoolCreation);
            CreateProgressiveLevels(levelsToCreate, progId,
                progLevelId, linkedLevelId,
                linkedProgressiveGroupId, protocolName, wager, levelCreationType);

            _eventBus.Setup(e => e.Unsubscribe<ProgressivesActivatedEvent>(It.IsAny<CurrentProgressivesLauncher>()))
                .Verifiable();

            _eventBus.Setup(e => e.Unsubscribe<SystemEnabledEvent>(It.IsAny<CurrentProgressivesLauncher>()))
                .Verifiable();

            _disableManager.Setup(d => d.IsDisabled).Returns(false);
            _menu.Setup(m => m.Show(Command.CurrentProgressiveMoneyIn)).Callback(() => waiter.Set());

            ConstructCurrentProgressivesLauncher();

            Assert.IsNotNull(_progressivesActivatedEventHandler);
            Assert.IsNotNull(_systemEnabledEventHandler);

            await _progressivesActivatedEventHandler.Invoke(new ProgressivesActivatedEvent(), default);
            await _systemEnabledEventHandler.Invoke(new SystemEnabledEvent(), default);

            if (levelCreationType == LevelCreationType.Default ||
                progressivePoolCreation == ProgressivePoolCreation.Default)
            {
                Assert.IsFalse(waiter.WaitOne(WaitTime));
                _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Never);
            }
            else
            {
                Assert.IsTrue(waiter.WaitOne(WaitTime));
                _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Once);
            }

            _eventBus.Verify(
                x => x.Unsubscribe<ProgressivesActivatedEvent>(It.IsAny<CurrentProgressivesLauncher>()),
                Times.Once);
            _eventBus.Verify(x => x.Unsubscribe<SystemEnabledEvent>(It.IsAny<CurrentProgressivesLauncher>()),
                Times.Once);

            _eventBus.VerifyAll();
            _menu.Verify();
        }

        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.All, ProgressivePoolCreation.WagerBased)]
        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.Default,
            ProgressivePoolCreation.WagerBased)]
        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.Default, ProgressivePoolCreation.Default)]
        [DataRow(2, 1, 1, 1, 1000, ProtocolName, 12345000, LevelCreationType.All, ProgressivePoolCreation.Default)]
        [DataTestMethod]
        public async Task GetProgressivesActivatedEvent_WhenSystemEnabled_ExpectShowCalledIfConditionsMet(int levelsToCreate,
            int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int wager,
            LevelCreationType levelCreationType = LevelCreationType.Default,
            ProgressivePoolCreation progressivePoolCreation = ProgressivePoolCreation.WagerBased)
        {
            var waiter = new ManualResetEvent(false);

            SetupShouldSubscribeEvents();
            SetupProgressivePoolCreationType(progressivePoolCreation);
            CreateProgressiveLevels(levelsToCreate, progId,
                progLevelId, linkedLevelId,
                linkedProgressiveGroupId, protocolName, wager, levelCreationType);

            _eventBus.Setup(e => e.Unsubscribe<ProgressivesActivatedEvent>(It.IsAny<CurrentProgressivesLauncher>()))
                .Verifiable();

            _eventBus.Setup(e => e.Unsubscribe<SystemEnabledEvent>(It.IsAny<CurrentProgressivesLauncher>()))
                .Verifiable();

            _disableManager.Setup(d => d.IsDisabled).Returns(false);
            _menu.Setup(m => m.Show(Command.CurrentProgressiveMoneyIn)).Callback(() => { waiter.Set(); });

            ConstructCurrentProgressivesLauncher();

            Assert.IsNotNull(_progressivesActivatedEventHandler);

            await _progressivesActivatedEventHandler.Invoke(new ProgressivesActivatedEvent(), default);

            if (levelCreationType == LevelCreationType.Default ||
                progressivePoolCreation == ProgressivePoolCreation.Default)
            {
                Assert.IsFalse(waiter.WaitOne(WaitTime));
                _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Never);
            }
            else
            {
                Assert.IsTrue(waiter.WaitOne(WaitTime));
                _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Once);
            }

            _eventBus.Verify(
                x => x.Unsubscribe<ProgressivesActivatedEvent>(It.IsAny<CurrentProgressivesLauncher>()),
                Times.Once);
            _eventBus.Verify(x => x.Unsubscribe<SystemEnabledEvent>(It.IsAny<CurrentProgressivesLauncher>()),
                Times.Once);

            _eventBus.VerifyAll();
            _menu.Verify();
        }

        [DataRow(EventType.CreditIn)]
        [DataRow(EventType.VoucherRedeemed)]
        [DataRow(EventType.WatOn)]
        [DataTestMethod]
        public async Task InsertCredit_FireEvent_VerifyViewModelCalled(EventType evt = EventType.Other)
        {
            SetupProgressivesAndOtherPrerequisites();

            FireBankBalanceChangedEvent(new Guid("{F4E88C86-F647-486C-9BE8-F30B82B92AFE}"));

            await FireDesiredEvent(evt);

            _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Once);
        }

        [DataRow(EventType.CreditIn)]
        [DataRow(EventType.VoucherRedeemed)]
        [DataRow(EventType.WatOn)]
        [DataTestMethod]
        public async Task WithCredit_ChangeBalanceAgainAndFireEvent_VerifyViewModelNotCalled(EventType evt = EventType.Other)
        {
            SetupProgressivesAndOtherPrerequisites();

            FireBankBalanceChangedEvent(new Guid("{F4E88C86-F647-486C-9BE8-F30B82B92AFE}"),10,20);

            await FireDesiredEvent(evt);

            _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Never);
        }

        [DataRow(EventType.CreditIn)]
        [DataRow(EventType.VoucherRedeemed)]
        [DataRow(EventType.WatOn)]
        [DataTestMethod]
        public async Task InsertCredit_ChangeBalanceAgainAndFireEvent_VerifyViewModelNotCalled(EventType evt = EventType.Other)
        {
            SetupProgressivesAndOtherPrerequisites();

            FireBankBalanceChangedEvent(new Guid("{F4E88C86-F647-486C-9BE8-F30B82B92AFE}"),10,20);

            await FireDesiredEvent(evt);

            _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Never);
        }

        [DataRow(EventType.CreditIn)]
        [DataRow(EventType.VoucherRedeemed)]
        [DataRow(EventType.WatOn)]
        [DataTestMethod]
        public async Task WithoutCredit_ChangeBalanceWithCurrentTransactionAndFireEvent_VerifyViewModelNotCalled(EventType evt = EventType.Other)
        {
            SetupProgressivesAndOtherPrerequisites();

            FireBankBalanceChangedEvent(Guid.Empty,0,20);

            await FireDesiredEvent(evt);

            _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Never);
        }

        [DataRow(EventType.CreditIn)]
        [DataRow(EventType.VoucherRedeemed)]
        [DataRow(EventType.WatOn)]
        [DataTestMethod]
        public async Task WithCredit_FireEventTwice_VerifyViewModelCalledOnce(EventType evt = EventType.Other)
        {
            SetupProgressivesAndOtherPrerequisites();

            FireBankBalanceChangedEvent(new Guid("{F4E88C86-F647-486C-9BE8-F30B82B92AFE}"));

            await FireDesiredEvent(evt);

            FireBankBalanceChangedEvent(new Guid("{F5E88C86-F647-486C-9BE8-F30B82B92AFE}"),10,20);

            await FireDesiredEvent(evt);

            _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Once);
        }

        [DataRow(EventType.CreditIn)]
        [DataRow(EventType.VoucherRedeemed)]
        [DataRow(EventType.WatOn)]
        [DataTestMethod]
        public async Task WithCredit_FireEventTwiceWithSecondTransactionAmountZero_VerifyViewModelCalledOnce(EventType evt = EventType.Other)
        {
            SetupProgressivesAndOtherPrerequisites();

            FireBankBalanceChangedEvent(new Guid("{F4E88C86-F647-486C-9BE8-F30B82B92AFE}"));

            await FireDesiredEvent(evt);

            await FireDesiredEvent(evt, 0);

            _menu.Verify(x => x.Show(Command.CurrentProgressiveMoneyIn), Times.Once);
        }

        private void SetupProgressivesAndOtherPrerequisites()
        {
            SetupProgressivePoolCreationType(ProgressivePoolCreation.WagerBased);
            CreateProgressiveLevels(2, 1, 1, 1,
                1000, ProtocolName, 12345000, LevelCreationType.All);
            SetupShouldNotSubscribeEvents();
            _bank.Setup(b => b.QueryBalance()).Returns(0);
            _gameHistory.Setup(g => g.IsRecoveryNeeded).Returns(false);

            _ = ConstructCurrentProgressivesLauncher();
        }

        private void FireBankBalanceChangedEvent(Guid guid,int oldBalance = 0, int newBalance = 10)
        {
            _bankBalanceChangedEvent(new BankBalanceChangedEvent(oldBalance, newBalance, guid));
        }

        private async Task FireDesiredEvent(EventType evt = EventType.Other, int amount = 10)
        {
            switch (evt)
            {
                case EventType.CreditIn:
                    await _currencyInCompletedEvent(new CurrencyInCompletedEvent(amount, null,
                        new BillTransaction(new List<char> { 'U', 'S', 'D' }, 1, DateTime.MaxValue, amount)), default);
                    break;
                case EventType.VoucherRedeemed:
                    await _voucherRedeemedEvent(new VoucherRedeemedEvent(new VoucherInTransaction { Amount = amount }), default);
                    break;
                case EventType.WatOn:
                    await _watOnCompleteEvent(new WatOnCompleteEvent(new WatOnTransaction { AuthorizedCashableAmount = amount }), default);
                    break;
                case EventType.Other:
                    break;
            }
        }

        private void CreateProgressiveLevels(int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int wager,
            LevelCreationType levelCreationType = LevelCreationType.Default)
        {
            for (var i = 0; i < levelsToCreate; ++i)
            {
                var assignedProgressiveKey = $"{protocolName}, " +
                                             $"Level Id: {linkedLevelId + i}, " +
                                             $"Progressive Group Id: {linkedProgressiveGroupId + i}";

                _progressiveLevels.Add(new ProgressiveLevel
                {
                    ProgressiveId = progId + i,
                    LevelId = progLevelId + i,
                    AssignedProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked,
                        assignedProgressiveKey),
                    WagerCredits = wager,
                    ResetValue = (i + 1) * 10000,
                    CurrentValue = 100,
                    CreationType = levelCreationType
                });
            }
        }

        private void SetupProgressivePoolCreationType(ProgressivePoolCreation type = ProgressivePoolCreation.Default)
        {
            _propertiesManager.Setup(x =>
                    x.GetProperty(GamingConstants.ProgressivePoolCreationType, ProgressivePoolCreation.Default))
                .Returns(type);

            _protocolLinkedProgressiveAdapter.Setup(x => x.GetActiveProgressiveLevels()).Returns(_progressiveLevels);
        }

        private void SetupShouldNotSubscribeEvents()
        {
            _eventBus.Setup(e => e.Subscribe(It.IsAny<CurrentProgressivesLauncher>(),
                It.IsAny<Action<SystemEnabledEvent>>())).Throws(new Exception("Shouldn't subscribe to events"));

            _eventBus.Setup(e => e.Subscribe(It.IsAny<CurrentProgressivesLauncher>(),
                    It.IsAny<Action<ProgressivesActivatedEvent>>()))
                .Throws(new Exception("Shouldn't subscribe to events"));
        }

        private void SetupShouldSubscribeEvents()
        {
            _bank.Setup(b => b.QueryBalance()).Returns(10);
            _gameHistory.Setup(g => g.IsRecoveryNeeded).Returns(false);

            _eventBus.Setup(e => e.Subscribe(It.IsAny<CurrentProgressivesLauncher>(),
                    It.IsAny<Func<SystemEnabledEvent, CancellationToken, Task>>()))
                .Callback<object, Func<SystemEnabledEvent, CancellationToken, Task>>((obj, handler) => _systemEnabledEventHandler = handler)
                .Verifiable();

            _eventBus.Setup(e => e.Subscribe(It.IsAny<CurrentProgressivesLauncher>(),
                    It.IsAny<Func<ProgressivesActivatedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ProgressivesActivatedEvent, CancellationToken, Task>>((obj, handler) =>
                    _progressivesActivatedEventHandler = handler).Verifiable();
        }

        private CurrentProgressivesLauncher ConstructCurrentProgressivesLauncher(
            bool nullEventBus = false,
            bool nullBank = false,
            bool nullGameHistory = false,
            bool nullDisableManager = false,
            bool nullMenuAccessService = false,
            bool nullPropertiesManager = false,
            bool nullProtocolLinkedProgressiveAdapter = false,
            bool nullTransactionCoordinator = false)
        {
            return new CurrentProgressivesLauncher(nullEventBus ? null : _eventBus.Object,
                nullBank ? null : _bank.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullDisableManager ? null : _disableManager.Object,
                nullMenuAccessService ? null : _menu.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullProtocolLinkedProgressiveAdapter ? null : _protocolLinkedProgressiveAdapter.Object);
        }
    }
}