namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts.Lobby;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CurrencyInValidatorTest
    {
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IBank> _bank;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _properties;
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IMessageDisplay> _messageDisplay;
        private Action<BankBalanceChangedEvent> _onBankBalanceChangedEvent;
        private Action<LobbyInitializedEvent> _onLobbyInitializedEvent;
        private CurrencyInValidator _target;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _storageManager = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Default);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _persistentStorageAccessor =
                MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Default);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            SetupCurrencyInValidator();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        /// <summary>
        ///     Tests for BankBalanceChanged event with Bank balance = MaxCreditLimit and DisableBankNoteAcceptorOnMaxCreditLimit
        ///     is set to true
        /// </summary>
        [TestMethod]
        public void BankBalanceAdditionToMaxCreditLimitDisablesBankNoteAcceptorTest()
        {
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(true);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 1000;
            SetupBank(currentBankBalance);
            var bankBalanceChangedEvent = new BankBalanceChangedEvent(980, 1000, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            VerifyStatusDisabledForNoteAcceptor();
            _bank.Setup(m => m.QueryBalance()).Returns(980);
            _noteAcceptor.Setup(x => x.Enabled).Returns(false);
            _noteAcceptor.Setup(x => x.ReasonDisabled).Returns(DisabledReasons.Configuration);
            bankBalanceChangedEvent = new BankBalanceChangedEvent(1000, 980, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            _noteAcceptor.Verify(x => x.Enable(EnabledReasons.Configuration), Times.Once);
            _messageDisplay.Verify(x => x.RemoveMessage(It.IsAny<DisplayableMessage>()), Times.Exactly(1));
        }

        /// <summary>
        ///     Tests for BankBalanceChanged event with Bank balance = MaxCreditLimit and DisableBankNoteAcceptorOnMaxCreditLimit
        ///     is set to false
        /// </summary>
        [TestMethod]
        public void BankBalanceAdditionToMaxCreditLimitNoChangeToBankNoteAcceptorTest()
        {
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(false);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 1000;
            SetupBank(currentBankBalance);
            var bankBalanceChangedEvent = new BankBalanceChangedEvent(980, currentBankBalance, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            _noteAcceptor.Verify(x => x.Disable(DisabledReasons.Configuration), Times.Never);
            _bank.Setup(m => m.QueryBalance()).Returns(980);
            bankBalanceChangedEvent = new BankBalanceChangedEvent(currentBankBalance, 980, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            _noteAcceptor.Verify(x => x.Enable(EnabledReasons.Configuration), Times.Never);
        }

        /// <summary>
        ///     Tests for Bank Balance > MaxCreditLimit
        /// </summary>
        [TestMethod]
        public void BankBalanceChangeGreaterThanMaxCreditLimitDisablesBankNoteAcceptorTest()
        {
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(true);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 1020;
            SetupBank(currentBankBalance);
            var bankBalanceChangedEvent = new BankBalanceChangedEvent(currentBankBalance, 1030, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            VerifyStatusDisabledForNoteAcceptor();
        }

        /// <summary>
        ///     Tests for Bank Balance < MaxCreditLimit
        /// </summary>
        [TestMethod]
        public void BankBalanceChangeLessThanMaxCreditLimitEnablesBankNoteAcceptorTest()
        {
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(true);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 950;
            SetupBank(currentBankBalance);
            var bankBalanceChangedEvent = new BankBalanceChangedEvent(currentBankBalance, 980, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            VerifyNoStatusChangeForNoteAcceptor();
        }

        /// <summary>
        ///     Tests for Credit above Max Credit Limit is allowed
        /// </summary>
        [TestMethod]
        public void BankBalanceAboveMaxCreditIsAllowedWithFlagSetTest()
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(true);
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(true);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 1000;
            SetupBank(currentBankBalance);

            // Balance > MaxCreditLimit
            var bankBalanceChangedEvent = new BankBalanceChangedEvent(currentBankBalance, 1200, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            VerifyNoStatusChangeForNoteAcceptor();

            // Balance = MaxCreditLimit
            bankBalanceChangedEvent = new BankBalanceChangedEvent(980, 1000, new Guid());
            _onBankBalanceChangedEvent(bankBalanceChangedEvent);
            VerifyNoStatusChangeForNoteAcceptor();
        }

        /// <summary>
        ///     Tests for MaxCreditLimit on Power reset, BankBalance = MaxCreditLimit
        /// </summary>
        [TestMethod]
        public void BankBalanceGreaterThanMaxCreditLimitOnPowerResetDisablesBankNoteAcceptorTest()
        {
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(true);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 1000;
            SetupBank(currentBankBalance);
            var evt = new LobbyInitializedEvent();
            _onLobbyInitializedEvent(evt);
            VerifyStatusDisabledForNoteAcceptor();
        }

        /// <summary>
        ///     Tests for MaxCreditLimit on Power reset, BankBalance < MaxCreditLimit
        /// </summary>
        [TestMethod]
        public void BankBalanceLessThanMaxCreditLimitOnPowerResetNoChangeToBankNoteAcceptorTest()
        {
            _properties.Setup(
                    x =>
                        x.GetProperty(
                            AccountingConstants.DisableBankNoteAcceptorWhenCreditLimitReached,
                            It.IsAny<bool>()))
                .Returns(true);
            _target.Initialize();
            Assert.IsNotNull(_onBankBalanceChangedEvent);
            const long currentBankBalance = 999;
            SetupBank(currentBankBalance);
            var evt = new LobbyInitializedEvent();
            _onLobbyInitializedEvent(evt);
            VerifyNoStatusChangeForNoteAcceptor();
        }

        private CurrencyInValidator GetTarget()
        {
            return new CurrencyInValidator();
        }

        private void VerifyStatusDisabledForNoteAcceptor()
        {
            _noteAcceptor.Verify(x => x.Disable(DisabledReasons.Configuration), Times.Once);
            _messageDisplay.Verify(x => x.DisplayMessage(It.IsAny<DisplayableMessage>()), Times.Once);
        }

        private void VerifyNoStatusChangeForNoteAcceptor()
        {
            _noteAcceptor.Verify(x => x.Disable(DisabledReasons.Configuration), Times.Never);
            _messageDisplay.Verify(x => x.DisplayMessage(It.IsAny<DisplayableMessage>()), Times.Never);
        }

        private void SetupBank(long currentBankBalance)
        {
            const long bankLimit = 1000;
            _bank.Setup(x => x.Limit).Returns(bankLimit);
            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance);
        }

        private void SetupCurrencyInValidator()
        {
            SetupStorage();
            SetupProperties();
            _target = GetTarget();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback<object, Action<BankBalanceChangedEvent>>(
                    (tar, func) => { _onBankBalanceChangedEvent = func; });
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<LobbyInitializedEvent>>()))
                .Callback<object, Action<LobbyInitializedEvent>>((tar, func) => { _onLobbyInitializedEvent = func; });
            SetupNoteAcceptor();
        }

        private void SetupNoteAcceptor()
        {
            _noteAcceptor.Setup(m => m.AcceptTicket()).Returns(Task.FromResult(true));
            _noteAcceptor.Setup(m => m.Return()).Returns(Task.FromResult(true));
            _noteAcceptor.Setup(m => m.DenomIsValid(It.IsAny<int>())).Returns(true);
            _noteAcceptor.Setup(m => m.AcceptNote()).Returns(Task.FromResult(true));
        }

        private void SetupStorage()
        {
            _storageManager.Setup(x => x.BlockExists(It.IsAny<string>())).Returns(true);
            _storageManager.Setup(x => x.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);
            _persistentStorageAccessor.Setup(x => x.StartTransaction())
                .Returns(new Mock<IPersistentStorageTransaction>().Object);
        }

        private void SetupProperties()
        {
            _properties.Setup(
                x => x.GetProperty(
                    ApplicationConstants.CurrencyMultiplierKey,
                    ApplicationConstants.DefaultCurrencyMultiplier)).Returns(1d);
            _properties.Setup(x => x.GetProperty(ApplicationConstants.CurrencyId, string.Empty)).Returns(string.Empty);
            _properties.Setup(x => x.GetProperty(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None))
                .Returns(CheckCreditsStrategy.None);
            _properties.Setup(
                    x =>
                        x.GetProperty(AccountingConstants.ShowMessageWhenCreditLimitReached, It.IsAny<bool>()))
                .Returns(true);
            _properties.Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Returns(false);
        }
    }
}