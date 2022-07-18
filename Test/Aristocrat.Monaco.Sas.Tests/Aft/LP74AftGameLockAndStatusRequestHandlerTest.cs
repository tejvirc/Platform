namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;
    using Sas.AftTransferProvider;
    using Test.Common;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;

    [TestClass]
    public class LP74AftGameLockAndStatusRequestHandlerTest
    {
        private const uint AssetNumber = 6;
        private const int OneSecondTimeout = 100;
        private const int MaxLockTimeout = 9999;
        private const long TransferLimit = 119999;
        private LP74AftGameLockAndStatusRequestHandler _target;
        private Mock<IAftLockHandler> _aftLockHandler;
        private Mock<IAftRegistrationProvider> _aftRegistrationProvider;
        private Mock<IAftHostCashOutProvider> _hostCashoutProvider;
        private Mock<ITicketingCoordinator> _ticketingCoordinator;
        private Mock<IAftOffTransferProvider> _aftOff;
        private Mock<IAftOnTransferProvider> _aftOn;
        private Mock<IBank> _bank;
        private Mock<IPropertiesManager> _propertiesManager;
        private AftGameLockAndStatusData _data;
        private AftGameLockAndStatusResponseData _response;
        private Mock<IPrinter> _printer;
        private Mock<IFundsTransferDisable> _fundsTransferDisable;
        private Mock<IAftTransferAssociations> _aftTransferAssociations;
        private Mock<IStorageDataProvider<AftTransferOptions>> _aftOptionsDataProvider;
        private AftTransferOptions _defaultTransferOptions = new AftTransferOptions
        {
            CurrentTransferFlags = AftTransferFlags.None
        };

        private SasFeatures _defaultFeatures = new SasFeatures
        {
            TransferInAllowed = true,
            TransferOutAllowed = true,
            AftBonusAllowed = true,
            WinTransferAllowed = true,
            PartialTransferAllowed = true,
            DebitTransfersAllowed = true,
            TransferLimit = TransferLimit
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _aftRegistrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Strict);
            _aftLockHandler = new Mock<IAftLockHandler>(MockBehavior.Strict);
            _hostCashoutProvider = new Mock<IAftHostCashOutProvider>(MockBehavior.Strict);
            _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _aftOff = new Mock<IAftOffTransferProvider>(MockBehavior.Default);
            _aftOn = new Mock<IAftOnTransferProvider>(MockBehavior.Default);
            _fundsTransferDisable = new Mock<IFundsTransferDisable>(MockBehavior.Default);
            _aftTransferAssociations = new Mock<IAftTransferAssociations>(MockBehavior.Default);
            _aftOptionsDataProvider = new Mock<IStorageDataProvider<AftTransferOptions>>(MockBehavior.Default);

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printer.Setup(x => x.CanPrint).Returns(true);

            _aftLockHandler.Setup(x => x.LockStatus).Returns(AftGameLockStatus.GameNotLocked);
            _aftLockHandler.Setup(x => x.AftLock(It.IsAny<bool>(), It.IsAny<uint>()));
            _aftLockHandler.Setup(x => x.AftLockTransferConditions).Returns(AftTransferConditions.None);
            _aftLockHandler.SetupSet(x => x.AftLockTransferConditions=It.IsAny<AftTransferConditions>()).Verifiable();

            _aftOff.Setup(x => x.IsAftOffAvailable).Returns(true);
            _aftOn.Setup(x => x.IsAftOnAvailable).Returns(true);

            _aftRegistrationProvider.Setup(x => x.IsAftRegistered).Returns(true);

            _bank.Setup(x => x.Limit).Returns(TransferLimit.CentsToMillicents());
            _bank.Setup(x => x.QueryBalance()).Returns(0);
            _bank.Setup(x => x.QueryBalance(It.IsAny<AccountType>())).Returns(0);

            _hostCashoutProvider.Setup(x => x.ResetCashOutExceptionTimer()).Verifiable();
	        _hostCashoutProvider.Setup(x => x.CanCashOut).Returns(true);
            _hostCashoutProvider.Setup(x => x.CashOutWinPending).Returns(true);
            _aftOptionsDataProvider.Setup(x => x.GetData()).Returns(_defaultTransferOptions);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(AssetNumber);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(_defaultFeatures);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsCashOutDeviceSupportedKey, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsHandPayDeviceSupportedKey, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.AftCustomTicketsSupportedKey, It.IsAny<object>())).Returns(true);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());

            _target = CreateTarget();

            _data = new AftGameLockAndStatusData();
        }

        [TestMethod]
        [DataRow(true, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(
            bool nullAftLock,
            bool nullBank,
            bool nullHostCashout,
            bool nullTicketingCoordinator,
            bool nullAftOptions,
            bool nullProperties,
            bool nullTransferAssociations)
        {
            _target = CreateTarget(
                nullAftLock,
                nullBank,
                nullHostCashout,
                nullTicketingCoordinator,
                nullAftOptions,
                nullProperties,
                nullTransferAssociations);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.AftGameLockAndStatusRequest));
        }

        [DataRow(false, true, DisplayName = "Aft off not available")]
        [DataRow(true, false, DisplayName = "Aft on not available")]
        [DataTestMethod]
        public void HandleStatusRequest(bool canTransferOff, bool canTransferOn)
        {
            _aftOff.Setup(x => x.IsAftOffAvailable).Returns(canTransferOff);
            _aftOn.Setup(x => x.IsAftOnAvailable).Returns(canTransferOn);
            _hostCashoutProvider.Setup(x => x.CashOutWinPending).Returns(false);

            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            _response = _target.Handle(_data);
            Assert.AreEqual(AftGameLockStatus.GameNotLocked, _response.GameLockStatus);

            Assert.AreEqual(AftTransferFlags.None, _response.HostCashoutStatus);
            Assert.AreEqual(SasConstants.MaximumTransactionBufferIndex, _response.MaxBufferIndex);
            Assert.AreEqual((ulong)0, _response.CurrentCashableAmount);
            Assert.AreEqual((ulong)0, _response.CurrentRestrictedAmount);
            Assert.AreEqual((ulong)0, _response.CurrentNonRestrictedAmount);
            Assert.AreEqual((ulong)TransferLimit, _response.CurrentGamingMachineTransferLimit);
            Assert.AreEqual((uint)0, _response.RestrictedExpiration);
            Assert.AreEqual((ushort)0, _response.RestrictedPoolId);

            _hostCashoutProvider.Verify();
        }

        [DataRow(false, true, true, DisplayName = "Aft not supported test")]
        [DataRow(true, false, false, DisplayName = "Aft transfers not available")]
        [DataTestMethod]
        public void HandleStatusRequestAftDisabled(bool aftSupported, bool canTransferOff, bool canTransferOn)
        {
            _defaultFeatures.TransferInAllowed = aftSupported;
            _defaultFeatures.TransferOutAllowed = aftSupported;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(_defaultFeatures);
            _aftOff.Setup(x => x.IsAftOffAvailable).Returns(canTransferOff);
            _aftOn.Setup(x => x.IsAftOnAvailable).Returns(canTransferOn);
            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            _response = _target.Handle(_data);
            Assert.AreEqual((AftAvailableTransfers)0, _response.AvailableTransfers);
            Assert.AreEqual((AftStatus)0, _response.AftStatus);

            _hostCashoutProvider.Verify();
        }

        [DataRow(false, false, false, false, DisplayName = "Funds Transfer not disabled")]
        [DataRow(true, false, false, false, DisplayName = "Funds Transfer on disabled in game")]
        [DataRow(false, true, false, false, DisplayName = "Funds Transfer off disabled")]
        [DataRow(false, false, true, false, DisplayName = "Funds Transfer on disabled")]
        [DataRow(false, false, false, true, DisplayName = "Funds Transfer on overlay")]
        [DataTestMethod]
        public void HandleStatusRequestFundsTransferDisable(bool transferOnDisabledInGame, bool transferOffDisabled, bool transferOnDisabledTilt, bool transferOnDisabledOverlay)
        {
            _fundsTransferDisable.Setup(x => x.TransferOnDisabledInGame).Returns(transferOnDisabledInGame);
            _fundsTransferDisable.Setup(x => x.TransferOffDisabled).Returns(transferOffDisabled);
            _fundsTransferDisable.Setup(x => x.TransferOnDisabledTilt).Returns(transferOnDisabledTilt);
            _fundsTransferDisable.Setup(x => x.TransferOnDisabledTilt).Returns(transferOnDisabledOverlay);
            _hostCashoutProvider.Setup(x => x.CashOutWinPending).Returns(false);
            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            _response = _target.Handle(_data);

            _hostCashoutProvider.Verify();
        }

        [DataRow(0L, 0L, (uint)30, (uint)0, DisplayName = "Bank Balance 0")]
        [DataRow(0L, 30L, (uint)30, (uint)30, DisplayName = "Only restricted balance of 30")]
        [DataRow(1L, 1L, (uint)30, (uint)30, DisplayName = "Bank Balance 2")]
        [DataRow(1L, 0L, (uint)30, (uint)0, DisplayName = "Only cashable balance of 1")]
        [DataRow(10L, 10L, (uint)30, (uint)30, DisplayName = "Bank Balance 20")]
        [DataRow(12L, 12L, (uint)30, (uint)30, DisplayName = "Bank Balance 24")]
        [DataRow(99L, 99L, (uint)30, (uint)30, DisplayName = "Bank Balance 198")]
        [DataRow(100L, 100L, (uint)30, (uint)30, DisplayName = "Bank Balance 200")]
        [DataRow(1000L, 1000L, (uint)30, (uint)30, DisplayName = "Bank Balance 2000")]
        [DataTestMethod]
        public void HandleStatusRequestWithBankBalanceLessThanTransferLimit(long cashable, long restricted, uint expirationDate, uint expectedExpiration)
        {
            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            _bank.Setup(x => x.QueryBalance()).Returns((cashable + restricted).CentsToMillicents());
            _bank.Setup(x => x.QueryBalance(AccountType.Cashable)).Returns(cashable.CentsToMillicents());
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(restricted.CentsToMillicents());
            _ticketingCoordinator.Setup(x => x.TicketExpirationRestricted).Returns(expirationDate);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData { PoolId = 0 });

            _response = _target.Handle(_data);

            Assert.AreEqual((ulong)(TransferLimit - (cashable + restricted)), _response.CurrentGamingMachineTransferLimit);
            Assert.AreEqual((ulong)cashable, _response.CurrentCashableAmount);
            Assert.AreEqual((ulong)restricted, _response.CurrentRestrictedAmount);
            Assert.AreEqual(expectedExpiration, _response.RestrictedExpiration);

            _hostCashoutProvider.Verify();
        }

        [DataRow(TransferLimit, DisplayName = "Bank Balance at transfer limit")]
        [DataRow(TransferLimit + 1, DisplayName = "Bank Balance at transfer limit + 1")]
        [DataRow(TransferLimit + 10, DisplayName = "Bank Balance at transfer limit + 10")]
        [DataRow(TransferLimit * 2, DisplayName = "Bank Balance at transfer limit * 2")]
        [DataTestMethod]
        public void HandleStatusRequestWithBankBalanceGreaterThanOrEqualToTransferLimit(long balance)
        {
            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            _bank.Setup(x => x.QueryBalance()).Returns(balance.CentsToMillicents());
            _response = _target.Handle(_data);
            Assert.AreEqual((ulong)0, _response.CurrentGamingMachineTransferLimit);

            _hostCashoutProvider.Verify();
        }

        [DataRow(10_00, 100_00, 0, 10_00, DisplayName = "AftLimit $10, Credit Limit $100, Balance $0, expect Transfer Limit $10")]
        [DataRow(100_00, 10_00, 0, 10_00, DisplayName = "AftLimit $100, Credit Limit $10, Balance $0, expect Transfer Limit $10")]
        [DataRow(10_00, 100_00, 95_00, 5_00, DisplayName = "AftLimit $10, Credit Limit $100, Balance $95, expect Transfer Limit $5")]
        [DataRow(1_000_00, 100_00, 0, 100_00, DisplayName = "AftLimit $1000, Credit Limit $100, Balance $0, expect Transfer Limit $100")]
        [DataRow(10_00, 100_00, 200_00, 0, DisplayName = "AftLimit $10, Credit Limit $100, Balance $200, expect Transfer Limit $0")]
        [DataTestMethod]
        public void HandleStatusRequestWithVaryingAftTransferLimitCreditLimitAndBalance(long aftLimit, long creditLimit, long balance, long expectedTransferLimit)
        {
            _bank.Setup(x => x.Limit).Returns(creditLimit.CentsToMillicents());
            _bank.Setup(x => x.QueryBalance()).Returns(balance.CentsToMillicents());
            _defaultFeatures.TransferLimit = aftLimit;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(_defaultFeatures);

            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            _response = _target.Handle(_data);
            Assert.AreEqual((ulong)expectedTransferLimit, _response.CurrentGamingMachineTransferLimit);

            _hostCashoutProvider.Verify();
        }

        [TestMethod]
        public void HandleLock()
        {
            _aftTransferAssociations.Setup(a => a.TransferConditionsMet(It.IsAny<AftGameLockAndStatusData>()))
                .Returns(true);

            _data.LockCode = AftLockCode.RequestLock;
            _data.LockTimeout = OneSecondTimeout;
            _data.TransferConditions = AftTransferConditions.None;

            _response = _target.Handle(_data);
            _aftLockHandler.Verify(x => x.AftLock(true, It.IsAny<uint>()));

            _hostCashoutProvider.Verify();
        }

        [TestMethod]
        public void HandleUnlock()
        {
            _aftTransferAssociations.Setup(a => a.TransferConditionsMet(It.IsAny<AftGameLockAndStatusData>()))
                .Returns(true);

            _data.LockCode = AftLockCode.RequestLock;
            _data.LockTimeout = MaxLockTimeout;
            _response = _target.Handle(_data);
            _aftLockHandler.Verify(x => x.AftLock(true, It.IsAny<uint>()));
            _data.LockCode = AftLockCode.CancelLockOrPendingLockRequest;
            _response = _target.Handle(_data);
            _aftLockHandler.Verify(x => x.AftLock(false, It.IsAny<uint>()));

            _hostCashoutProvider.Verify();
        }

        [TestMethod]
        public void HandleLockOverride()
        {
            _aftTransferAssociations.Setup(a => a.TransferConditionsMet(It.IsAny<AftGameLockAndStatusData>()))
                .Returns(true);

            _data.LockCode = AftLockCode.RequestLock;
            _data.LockTimeout = MaxLockTimeout;
            _response = _target.Handle(_data);
            _aftLockHandler.Verify(x => x.AftLock(true, It.IsAny<uint>()));
            _data.LockCode = AftLockCode.RequestLock;
            _data.LockTimeout = OneSecondTimeout;
            _response = _target.Handle(_data);
            _aftLockHandler.Verify(x => x.AftLock(true, It.IsAny<uint>()));

            _hostCashoutProvider.Verify();
        }

        [DataRow(0, true, true, true, true, true, DisplayName = "No conditions, lock success")]
        [DataRow(0, false, false, false, false, false, DisplayName = "No conditions, nothing supported, lock success")]
        [DataRow(0xFF, true, true, true, true, true, DisplayName = "All conditions, all supported, lock success")]
        [DataRow(AftTransferConditions.BonusAwardToGamingMachineOk, true, true, true, false, true, DisplayName = "BonusAwardToGamingMachineOk condition, bonus transfers not supported, lock failure")]
        [DataRow(AftTransferConditions.BonusAwardToGamingMachineOk, false, false, false, true, false, DisplayName = "BonusAwardToGamingMachineOk condition, bonus transfers supported, lock success")]
        [DataRow(AftTransferConditions.TransferFromGamingMachineOk, true, false, true, true, true, DisplayName = "TransferFromGamingMachineOk condition, funds transfer out not supported, lock failure")]
        [DataRow(AftTransferConditions.TransferFromGamingMachineOk, false, true, false, false, false, DisplayName = "TransferFromGamingMachineOk condition, funds transfer out supported, lock success")]
        [DataRow(AftTransferConditions.TransferToGamingMachineOk, false, true, true, true, true, DisplayName = "TransferToGamingMachineOk condition, funds transfer in not supported, lock failure")]
        [DataRow(AftTransferConditions.TransferToGamingMachineOk, true, false, false, false, false, DisplayName = "TransferToGamingMachineOk condition, funds transfer in supported, lock success")]
        [DataRow(AftTransferConditions.TransferToPrinterOk, true, true, false, true, true, DisplayName = "TransferToPrinterOk condition, printer as cashout device not supported, lock failure")]
        [DataRow(AftTransferConditions.TransferToPrinterOk, true, true, true, true, false, DisplayName = "TransferToPrinterOk condition, printer can not print, lock failure")]
        [DataRow(AftTransferConditions.TransferToPrinterOk, false, false, true, false, true, DisplayName = "TransferToPrinterOk condition, printer as cashout device supported and printer can print, lock success")]
        [DataTestMethod]
        public void HandleLockConditionsNotMet(
            AftTransferConditions transferConditions,
            bool aftFundsTransferInSupported,
            bool aftFundsTransferOutSupported,
            bool printerAsCashOutDeviceSupported,
            bool aftBonusTransfersSupported,
            bool printerCanPrint)
        {
            _printer.Setup(x => x.CanPrint).Returns(printerCanPrint);
            _defaultFeatures.TransferInAllowed = aftFundsTransferInSupported;
            _defaultFeatures.TransferOutAllowed = aftFundsTransferOutSupported;
            _defaultFeatures.AftBonusAllowed = aftBonusTransfersSupported;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(_defaultFeatures);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsCashOutDeviceSupportedKey, It.IsAny<object>())).Returns(printerAsCashOutDeviceSupported);
            _data.LockCode = AftLockCode.RequestLock;
            _data.LockTimeout = OneSecondTimeout;
            _data.TransferConditions = transferConditions;
            _response = _target.Handle(_data);
            _hostCashoutProvider.Verify();
        }

        private LP74AftGameLockAndStatusRequestHandler CreateTarget(
            bool nullAftLock = false,
            bool nullBank = false,
            bool nullHostCashout = false,
            bool nullTicketingCoordinator = false,
            bool nullAftOptions = false,
            bool nullProperties = false,
            bool nullTransferAssociations = false)
        {
            return new LP74AftGameLockAndStatusRequestHandler(
                nullAftLock ? null : _aftLockHandler.Object,
                nullBank ? null : _bank.Object,
                nullHostCashout ? null : _hostCashoutProvider.Object,
                nullTicketingCoordinator ? null : _ticketingCoordinator.Object,
                nullAftOptions ? null : _aftOptionsDataProvider.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullTransferAssociations ? null : _aftTransferAssociations.Object);
        }
    }
}
