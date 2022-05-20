namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Contracts.SelfAudit;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts.Events;
    using Kernel.Contracts.LockManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using SelfAudit;
    using Test.Common;

    /// <summary>
    ///     Summary description for SelfAuditErrorCheckingServiceTests
    /// </summary>
    [TestClass]
    public class SelfAuditErrorCheckingServiceTests
    {
        private const string SelfAuditFailedFieldName = "SelfAuditFailed";
        private const string TotalCashCoinTicketInAmount = "AccountingMeters.TotalCashCoinTicketInAmount";
        private const string ElectronicTransfersOnTotalAmount = "AccountingMeters.ElectronicTransfersOnTotalAmount";
        private const string EgmPaidGameWonAmount = "GamingMeters.EgmPaidGameWonAmount";
        private const string WageredAmount = "GamingMeters.WageredAmount";
        private const string ElectronicTransfersOffTotalAmount = "AccountingMeters.ElectronicTransfersOffTotalAmount";
        private const string TotalCancelCreditAmount = "AccountingMeters.TotalCancelCreditAmount";

        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IPersistentStorageManager> _storage;
        private Mock<IPersistentStorageAccessor> _accessor;
        private Mock<IPersistentStorageTransaction> _storageTransaction;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<ILockManager> _lockManager;
        private Mock<IDisposable> _disposable;
        private Mock<IAddinHelper> _addinHelper;
        private Mock<ICreditMetersProvider> _creditMetersProvider;
        private Mock<IDebitMetersProvider> _debitMetersProvider;
        private Mock<ISelfAuditRunAdviceProvider> _selfAuditRunAdviceProvider;

        //Mock meters
        private Mock<IMeter> _currentCredits;
        private Mock<IMeter> _totalCashCoinTicketInAmount;
        private Mock<IMeter> _electronicTransfersOnTotalAmount;
        private Mock<IMeter> _egmPaidGameWonAmount;
        private Mock<IMeter> _wageredAmount;
        private Mock<IMeter> _electronicTransfersOffTotalAmount;
        private Mock<IMeter> _totalCancelCreditAmount;

        private SelfAuditErrorCheckingService _target;
        private Action<InitializationCompletedEvent> _initializationCompletedEventHandler;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);

            SetupAddinHelper();
            SetupStorageManager();
            SetupPropertiesManager();
            SetupLockManager();
            SetupEventBus();
            SetupSystemDisableManager(false);
            SetupService();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _target?.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNull_ExpectException(
            bool nullMeter = false,
            bool nullProperties = false,
            bool nullEvent = false,
            bool nullSystemDisable = false,
            bool nullLockManager = false)
        {
            SetupService(nullMeter, nullProperties, nullEvent, nullSystemDisable, nullLockManager);
            Assert.IsNull(_target);
        }

        [TestMethod]
        public void WhenParamsAreValid_ExpectSuccess()
        {
            Assert.IsNotNull(_target);
        }

        [DataRow(10, 10, 10, 10, 10, 10, 10, "Once", "Once")]
        [DataRow(10, 10, 10, 10, 10, 10, 10, "Once", "Once")]
        [DataRow(10, 10, 10, 10, 10, 0, 10, "Never", "Never")]
        [DataRow(0, long.MaxValue, 10, 10, 10, 0, 9, "Never", "Never")]
        [DataTestMethod]
        public void WhenServiceEnabled_SelfAuditFails_ExpectLockup(
            long currentCredits,
            long totalCashCoinTicketInAmount,
            long electronicTransfersOnTotalAmount,
            long egmPaidGameWonAmount,
            long wageredAmount,
            long electronicTransfersOffTotalAmount,
            long totalCancelCreditAmount,
            string disableTimes,
            string commitTimes)
        {
            SetupSelfAuditMetersForLifetimeValue(
                currentCredits,
                totalCashCoinTicketInAmount,
                electronicTransfersOnTotalAmount,
                egmPaidGameWonAmount,
                wageredAmount,
                electronicTransfersOffTotalAmount,
                totalCancelCreditAmount);

            _ = _target.CheckSelfAuditPassing();
            VerifySystemDisableManager(GetTimesFromString(disableTimes));
            VerifySetProperty(GetTimesFromString(commitTimes), true);
        }

        [TestMethod]
        public void ServiceDisabled_ExpectNoLockup()
        {
            _propertiesManager
                .Setup(p => p.GetProperty(AccountingConstants.SelfAuditErrorCheckingEnabled, It.IsAny<object>()))
                .Returns(false);
            SetupSelfAuditMetersForLifetimeValue(10, 10, 10, 10, 10, 10, 10);

            _ = _target.CheckSelfAuditPassing();
            VerifySystemDisableManager(Times.Never());
            VerifySetProperty(Times.Never(), true);
        }

        [TestMethod]
        public void SelfAuditFailed_OnPowerCycle_ExpectLockup()
        {
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.SelfAuditErrorOccurred, It.IsAny<bool>()))
                .Returns(true);
            SetupService();
            VerifySystemDisableManager(Times.Once());
            VerifySetProperty(Times.Never(), true);
        }

        private void SetupAddinHelper()
        {
            _addinHelper = MoqServiceManager.CreateAndAddService<IAddinHelper>(MockBehavior.Strict);
            SetupMockExtensions();
        }

        private void SetupMockExtensions()
        {
            //Create and setup meters
            SetupSelfAuditMeters(10, 10, 10, 10, 10, 10, 10);

            _creditMetersProvider = new Mock<ICreditMetersProvider>(MockBehavior.Default);
            _creditMetersProvider.Setup(x => x.GetMeters()).Returns(
                new List<IMeter>
                {
                    _meterManager.Object.GetMeter(_totalCashCoinTicketInAmount.Object.Name),
                    _meterManager.Object.GetMeter(_electronicTransfersOnTotalAmount.Object.Name),
                    _meterManager.Object.GetMeter(_egmPaidGameWonAmount.Object.Name)
                });
            var creditMeterList = new List<TypeExtensionNode>();
            creditMeterList.Add(new TestTypeExtensionNode(_creditMetersProvider.Object));
            _addinHelper.Setup(m => m.GetSelectedNodes<TypeExtensionNode>("/Accounting/CreditMetersProvider"))
                .Returns(creditMeterList);

            _debitMetersProvider = new Mock<IDebitMetersProvider>(MockBehavior.Default);
            _debitMetersProvider.Setup(x => x.GetMeters()).Returns(
                new List<IMeter>
                {
                    _meterManager.Object.GetMeter(_wageredAmount.Object.Name),
                    _meterManager.Object.GetMeter(_electronicTransfersOffTotalAmount.Object.Name),
                    _meterManager.Object.GetMeter(_totalCancelCreditAmount.Object.Name)
                });
            var debitMeterList = new List<TypeExtensionNode>();
            debitMeterList.Add(new TestTypeExtensionNode(_debitMetersProvider.Object));
            _addinHelper.Setup(m => m.GetSelectedNodes<TypeExtensionNode>("/Accounting/DebitMetersProvider"))
                .Returns(debitMeterList);

            _selfAuditRunAdviceProvider = new Mock<ISelfAuditRunAdviceProvider>(MockBehavior.Default);
            _selfAuditRunAdviceProvider.Setup(x => x.SelfAuditOkToRun()).Returns(true);
            var runAdviceProviderList = new List<TypeExtensionNode>();
            runAdviceProviderList.Add(new TestTypeExtensionNode(_selfAuditRunAdviceProvider.Object));
            _addinHelper.Setup(m => m.GetSelectedNodes<TypeExtensionNode>("/Accounting/SelfAuditRunAdviceProvider"))
                .Returns(runAdviceProviderList);
        }

        private void SetupPropertiesManager()
        {
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager
                .Setup(p => p.GetProperty(AccountingConstants.SelfAuditErrorCheckingEnabled, It.IsAny<object>()))
                .Returns(true);
            _propertiesManager
                .Setup(p => p.GetProperty(AccountingConstants.SelfAuditErrorOccurred, It.IsAny<bool>()))
                .Returns(false);
            _propertiesManager
                .Setup(p => p.SetProperty(AccountingConstants.SelfAuditErrorOccurred, It.IsAny<bool>()))
                .Verifiable();
        }

        private void SetupLockManager()
        {
            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
            _lockManager = MoqServiceManager.CreateAndAddService<ILockManager>(MockBehavior.Default);
            _lockManager.Setup(l => l.AcquireExclusiveLock(It.IsAny<IEnumerable<ILockable>>()))
                .Returns(_disposable.Object);
        }

        private void SetupStorageManager()
        {
            _storageTransaction =
                MoqServiceManager.CreateAndAddService<IPersistentStorageTransaction>(MockBehavior.Strict);
            _storageTransaction.Setup(m => m.Commit());
            _storageTransaction.Setup(m => m.Dispose());
            _storageTransaction.SetupSet(m => m[SelfAuditFailedFieldName] = true).Verifiable();

            _accessor = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _accessor.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);
            _accessor.SetupGet(m => m[SelfAuditFailedFieldName]).Returns(false);

            _storage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _storage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _storage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_accessor.Object);
            _storage.Setup(m => m.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_accessor.Object);
        }

        private void SetupEventBus()
        {
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<SelfAuditErrorCheckingService>(),
                        It.IsAny<Action<InitializationCompletedEvent>>()))
                .Callback<object, Action<InitializationCompletedEvent>
                >((y, x) => _initializationCompletedEventHandler = x);
        }

        private void SetupSystemDisableManager(bool disabled)
        {
            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Strict);
            _systemDisableManager.Setup(m => m.IsDisabled).Returns(disabled);
            _systemDisableManager.Setup(
                m => m.Disable(
                    ApplicationConstants.SelfAuditErrorGuid,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    null));
        }

        private void VerifySystemDisableManager(Times times)
        {
            _systemDisableManager.Verify(
                m => m.Disable(
                    ApplicationConstants.SelfAuditErrorGuid,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    null),
                times);
        }

        private void VerifySetProperty(Times times, bool value)
        {
            _propertiesManager.Verify(m => m.SetProperty(It.IsAny<string>(), value), times);
        }

        private void SetupSelfAuditMeters(
            long currentCredits,
            long totalCashCoinTicketInAmount,
            long electronicTransfersOnTotalAmount,
            long egmPaidGameWonAmount,
            long wageredAmount,
            long electronicTransfersOffTotalAmount,
            long totalCancelCreditAmount)
        {
            //Only current credits is referred via accounting constant so using same constant here
            _currentCredits = SetupMeter(AccountingMeters.CurrentCredits, currentCredits);

            //Rest of the meters are provided via nodes and in testing we are using these strings for mock meter names
            _totalCashCoinTicketInAmount = SetupMeter(TotalCashCoinTicketInAmount, totalCashCoinTicketInAmount);
            _electronicTransfersOnTotalAmount = SetupMeter(
                ElectronicTransfersOnTotalAmount,
                electronicTransfersOnTotalAmount);
            _egmPaidGameWonAmount = SetupMeter(EgmPaidGameWonAmount, egmPaidGameWonAmount);
            _wageredAmount = SetupMeter(WageredAmount, wageredAmount);
            _electronicTransfersOffTotalAmount = SetupMeter(
                ElectronicTransfersOffTotalAmount,
                electronicTransfersOffTotalAmount);
            _totalCancelCreditAmount = SetupMeter(TotalCancelCreditAmount, totalCancelCreditAmount);
        }

        private Mock<IMeter> SetupMeter(string meterName, long lifetimeValue)
        {
            var met = new Mock<IMeter>(MockBehavior.Strict);
            met.Setup(m => m.Lifetime).Returns(lifetimeValue);
            met.Setup(m => m.Name).Returns(meterName);

            _meterManager.Setup(m => m.GetMeter(meterName)).Returns(met.Object);
            return met;
        }

        private void SetupSelfAuditMetersForLifetimeValue(
            long currentCredits,
            long totalCashCoinTicketInAmount,
            long electronicTransfersOnTotalAmount,
            long egmPaidGameWonAmount,
            long wageredAmount,
            long electronicTransfersOffTotalAmount,
            long totalCancelCreditAmount)
        {
            //Only current credits is referred via accounting constant so using same constant here
            _currentCredits.Setup(m => m.Lifetime).Returns(currentCredits);
            _totalCashCoinTicketInAmount.Setup(m => m.Lifetime).Returns(totalCashCoinTicketInAmount);
            _electronicTransfersOnTotalAmount.Setup(m => m.Lifetime).Returns(electronicTransfersOnTotalAmount);
            _egmPaidGameWonAmount.Setup(m => m.Lifetime).Returns(egmPaidGameWonAmount);
            _wageredAmount.Setup(m => m.Lifetime).Returns(wageredAmount);
            _electronicTransfersOffTotalAmount.Setup(m => m.Lifetime).Returns(electronicTransfersOffTotalAmount);
            _totalCancelCreditAmount.Setup(m => m.Lifetime).Returns(totalCancelCreditAmount);
        }

        private void SetupService(
            bool nullMeter = false,
            bool nullProperties = false,
            bool nullEvent = false,
            bool nullSystemDisable = false,
            bool nullLockManager = false)
        {
            _target = new SelfAuditErrorCheckingService(
                nullMeter ? null : _meterManager.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullEvent ? null : _eventBus.Object,
                nullSystemDisable ? null : _systemDisableManager.Object,
                nullLockManager ? null : _lockManager.Object);
            _target.Initialize();
            _initializationCompletedEventHandler.Invoke(new InitializationCompletedEvent());
        }

        private Times GetTimesFromString(string times)
        {
            if (times == "Once")
            {
                return Times.Once();
            }

            return Times.Never();
        }

        private class TestTypeExtensionNode : TypeExtensionNode
        {
            private readonly object _createInstanceReturn;

            public TestTypeExtensionNode(object createInstanceReturn)
            {
                _createInstanceReturn = createInstanceReturn;
            }

            public override object CreateInstance()
            {
                return _createInstanceReturn;
            }
        }
    }
}