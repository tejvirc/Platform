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
    using Aristocrat.Monaco.Sas.Storage.Models;

    [TestClass]
    public class AftTransferAssociationsTest
    {
        private const uint AssetNumber = 6;
        private const int OneSecondTimeout = 100;
        private AftTransferAssociations _target;
        private Mock<IAftRegistrationProvider> _aftRegistrationProvider;
        private Mock<IHostCashOutProvider> _hostCashoutProvider;
        private Mock<IAftTransferProvider> _aftProvider;
        private Mock<IAftOffTransferProvider> _aftOff;
        private Mock<IAftOnTransferProvider> _aftOn;
        private Mock<IPropertiesManager> _propertiesManager;
        private AftGameLockAndStatusData _data;
        private Mock<IPrinter> _printer;
        private Mock<IFundsTransferDisable> _fundsTransferDisable;
        private SasFeatures _defaultFeatures = new SasFeatures
        {
            AftBonusAllowed = true,
            TransferInAllowed = true,
            TransferOutAllowed = true,
            TransferToTicketAllowed = true,
            WinTransferAllowed = true,
            PartialTransferAllowed = true,
            DebitTransfersAllowed = true
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _aftRegistrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Strict);
            _hostCashoutProvider = new Mock<IHostCashOutProvider>(MockBehavior.Strict);
            _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);
            _aftOff = new Mock<IAftOffTransferProvider>(MockBehavior.Default);
            _aftOn = new Mock<IAftOnTransferProvider>(MockBehavior.Default);
            _fundsTransferDisable = new Mock<IFundsTransferDisable>(MockBehavior.Default);

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printer.Setup(x => x.CanPrint).Returns(true);

            _aftOff.Setup(x => x.IsAftOffAvailable).Returns(true);
            _aftOn.Setup(x => x.IsAftOnAvailable).Returns(true);

            _aftRegistrationProvider.Setup(x => x.IsAftRegistered).Returns(true);

            _hostCashoutProvider.Setup(x => x.ResetCashOutExceptionTimer());
	        _hostCashoutProvider.Setup(x => x.CanCashOut).Returns(true);
            _hostCashoutProvider.Setup(x => x.CashOutWinPending).Returns(true);
            _hostCashoutProvider.Setup(x => x.HostCashOutPending).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(AssetNumber);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, new SasFeatures())).Returns(_defaultFeatures);

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsCashOutDeviceSupportedKey, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.PrinterAsHandPayDeviceSupportedKey, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.AftCustomTicketsSupportedKey, It.IsAny<object>())).Returns(true);

            _target = new AftTransferAssociations(
                _aftProvider.Object,
                _aftOff.Object,
                _aftOn.Object,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);

            _data = new AftGameLockAndStatusData();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAftProviderTest()
        {
            _target = new AftTransferAssociations(
                null,
                _aftOff.Object,
                _aftOn.Object,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAftOffTest()
        {
            _target = new AftTransferAssociations(
                _aftProvider.Object,
                null,
                _aftOn.Object,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAftOnTest()
        {
            _target = new AftTransferAssociations(
                _aftProvider.Object,
                _aftOff.Object,
                null,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullHostCashoutProviderTest()
        {
            _target = new AftTransferAssociations(
                _aftProvider.Object,
                _aftOff.Object,
                _aftOn.Object,
                null,
                _fundsTransferDisable.Object,
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFundsTransferDisableTest()
        {
            _target = new AftTransferAssociations(
                _aftProvider.Object,
                _aftOff.Object,
                _aftOn.Object,
                _hostCashoutProvider.Object,
                null,
                _aftRegistrationProvider.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullAftRegistrationProviderTest()
        {
            _target = new AftTransferAssociations(
                _aftProvider.Object,
                _aftOff.Object,
                _aftOn.Object,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                null,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new AftTransferAssociations(
                _aftProvider.Object,
                _aftOff.Object,
                _aftOn.Object,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                _aftRegistrationProvider.Object,
                null);
        }

        [DataRow(false, true, true, DisplayName = "Aft off not available")]
        [DataRow(true, false, true, DisplayName = "Aft on not available")]
        [DataRow(true, true, false, DisplayName = "Aft to printer not supported")]
        [DataTestMethod]
        public void HandleStatusRequest(bool canTransferOff, bool canTransferOn, bool canTransferToPrinter)
        {
            _aftOff.Setup(x => x.IsAftOffAvailable).Returns(canTransferOff);
            _aftOn.Setup(x => x.IsAftOnAvailable).Returns(canTransferOn);
            _defaultFeatures.TransferToTicketAllowed = canTransferToPrinter;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(_defaultFeatures);
            _hostCashoutProvider.Setup(x => x.CashOutWinPending).Returns(false);

            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            var availableTransfers = _target.GetAvailableTransfers();
            var aftStatus = _target.GetAftStatus();
            var expectedTransfers = (canTransferToPrinter ? AftAvailableTransfers.TransferToPrinterOk : 0) |
                                    AftAvailableTransfers.BonusAwardToGamingMachineOk;
            if (canTransferOff)
            {
                expectedTransfers |= AftAvailableTransfers.TransferFromGamingMachineOk;
            }

            if (canTransferOn)
            {
                expectedTransfers |= AftAvailableTransfers.TransferToGamingMachineOk;
            }

            Assert.AreEqual(expectedTransfers, availableTransfers);
            Assert.AreEqual(
                AftStatus.PrinterAvailableForTransactionReceipts |
                AftStatus.TransferToHostOfLessThanFullAvailableAmountAllowed | AftStatus.CustomTicketDataSupported |
                AftStatus.AftRegistered | AftStatus.InHouseTransfersEnabled | AftStatus.BonusTransfersEnabled |
                AftStatus.DebitTransfersEnabled | AftStatus.AnyAftEnabled,
                aftStatus);
        }

        [DataRow(false, true, true, true, DisplayName = "Aft not supported test")]
        [DataRow(true, false, true, true, DisplayName = "Aft transfer in progress")]
        [DataRow(true, true, false, false, DisplayName = "Aft transfer not available")]
        [DataTestMethod]
        public void HandleStatusRequestAftDisabled(bool aftSupported, bool transferNotInProgress, bool canTransferOff, bool canTransferOn)
        {
            _defaultFeatures.TransferInAllowed = aftSupported;
            _defaultFeatures.TransferOutAllowed = aftSupported;
            _defaultFeatures.WinTransferAllowed = aftSupported;
            _defaultFeatures.AftBonusAllowed = aftSupported;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(_defaultFeatures);
            _aftProvider.Setup(x => x.IsTransferInProgress).Returns(!transferNotInProgress);
            _aftOff.Setup(x => x.IsAftOffAvailable).Returns(canTransferOff);
            _aftOn.Setup(x => x.IsAftOnAvailable).Returns(canTransferOn);
            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            var availableTransfers = _target.GetAvailableTransfers();
            var aftStatus = _target.GetAftStatus();

            Assert.AreEqual((AftAvailableTransfers)0, availableTransfers);
            Assert.AreEqual((AftStatus)0, aftStatus);
        }

        [DataRow(false, false, false, false, true, DisplayName = "Funds Transfer not disabled")]
        [DataRow(true, false, false, false, true, DisplayName = "Funds Transfer on disabled in game")]
        [DataRow(false, true, false, false, true, DisplayName = "Funds Transfer off disabled")]
        [DataRow(false, false, true, false, true, DisplayName = "Funds Transfer on disabled")]
        [DataRow(false, false, false, true, true, DisplayName = "Funds Transfer on overlay")]
        [DataRow(false, false, false, false, false, DisplayName = "Funds Transfer to printer not supported")]
        [DataTestMethod]
        public void HandleStatusRequestFundsTransferDisable(
            bool transferOnDisabledInGame,
            bool winPending,
            bool transferOnDisabledTilt,
            bool transferOnDisabledOverlay,
            bool transferToPrinterSupported)
        {
            _fundsTransferDisable.Setup(x => x.TransferOnDisabledInGame).Returns(transferOnDisabledInGame);
            _fundsTransferDisable.Setup(x => x.TransferOnDisabledTilt).Returns(transferOnDisabledTilt);
            _fundsTransferDisable.Setup(x => x.TransferOnDisabledOverlay).Returns(transferOnDisabledOverlay);
            _defaultFeatures.TransferToTicketAllowed = transferToPrinterSupported;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(_defaultFeatures);
            _hostCashoutProvider.Setup(x => x.CashOutWinPending).Returns(winPending);
            _data.LockCode = AftLockCode.InterrogateCurrentStatusOnly;
            var availableTransfers = _target.GetAvailableTransfers();
            var aftStatus = _target.GetAftStatus();
            Assert.AreEqual(
                (!transferOnDisabledInGame && !transferOnDisabledTilt && !transferOnDisabledOverlay
                    ? AftAvailableTransfers.TransferToGamingMachineOk
                    : 0) |
                (!winPending ? AftAvailableTransfers.TransferFromGamingMachineOk : 0) |
                (!transferOnDisabledInGame && !transferOnDisabledTilt && !transferOnDisabledOverlay &&
                 transferToPrinterSupported
                    ? AftAvailableTransfers.TransferToPrinterOk
                    : 0) |
                0 |
                (!transferOnDisabledTilt && !transferOnDisabledOverlay
                    ? AftAvailableTransfers.BonusAwardToGamingMachineOk
                    : 0) |
                (winPending ? AftAvailableTransfers.WinAmountPendingCashoutToHost : 0),
                availableTransfers);

            Assert.AreEqual(
                AftStatus.PrinterAvailableForTransactionReceipts |
                AftStatus.TransferToHostOfLessThanFullAvailableAmountAllowed | AftStatus.CustomTicketDataSupported |
                AftStatus.AftRegistered | AftStatus.InHouseTransfersEnabled | AftStatus.BonusTransfersEnabled |
                AftStatus.DebitTransfersEnabled | AftStatus.AnyAftEnabled,
                aftStatus);
        }

        [DataRow(0, true, true, true, true, true, true, DisplayName = "No conditions, lock success")]
        [DataRow(0, false, false, false, false, false, false, DisplayName = "No conditions, nothing supported, lock failure")]
        [DataRow(0xFF, true, true, true, true, true, true, DisplayName = "All conditions, all supported, lock success")]
        [DataRow(AftTransferConditions.BonusAwardToGamingMachineOk, true, true, true, false, true, false, DisplayName = "BonusAwardToGamingMachineOk condition, bonus transfers not supported, lock failure")]
        [DataRow(AftTransferConditions.BonusAwardToGamingMachineOk, false, false, false, true, false, true, DisplayName = "BonusAwardToGamingMachineOk condition, bonus transfers supported, lock success")]
        [DataRow(AftTransferConditions.TransferFromGamingMachineOk, true, false, true, true, true, false, DisplayName = "TransferFromGamingMachineOk condition, funds transfer out not supported, lock failure")]
        [DataRow(AftTransferConditions.TransferFromGamingMachineOk, false, true, false, false, false, true, DisplayName = "TransferFromGamingMachineOk condition, funds transfer out supported, lock success")]
        [DataRow(AftTransferConditions.TransferToGamingMachineOk, false, true, true, true, true, false, DisplayName = "TransferToGamingMachineOk condition, funds transfer in not supported, lock failure")]
        [DataRow(AftTransferConditions.TransferToGamingMachineOk, true, true, false, false, false, true, DisplayName = "TransferToGamingMachineOk condition, funds transfer in supported, lock success")]
        [DataRow(AftTransferConditions.TransferToPrinterOk, true, true, false, true, true, false, DisplayName = "TransferToPrinterOk condition, printer as cashout device not supported, lock failure")]
        [DataRow(AftTransferConditions.TransferToPrinterOk, true, true, true, true, false, false, DisplayName = "TransferToPrinterOk condition, printer can not print, lock failure")]
        [DataRow(AftTransferConditions.TransferToPrinterOk, false, false, true, false, true, true, DisplayName = "TransferToPrinterOk condition, printer as cashout device supported and printer can print, lock success")]
        [DataTestMethod]
        public void HandleLockConditionsNotMet(
            AftTransferConditions transferConditions,
            bool aftFundsTransferInSupported,
            bool aftFundsTransferOutSupported,
            bool transferToPrinterSupported,
            bool aftBonusTransfersSupported,
            bool printerCanPrint,
            bool lockSuccess)
        {
            _printer.Setup(x => x.CanPrint).Returns(printerCanPrint);
            _defaultFeatures.TransferToTicketAllowed = transferToPrinterSupported;
            _defaultFeatures.TransferInAllowed = aftFundsTransferInSupported;
            _defaultFeatures.TransferOutAllowed = aftFundsTransferOutSupported;
            _defaultFeatures.AftBonusAllowed = aftBonusTransfersSupported;
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(_defaultFeatures);

            _data.LockCode = AftLockCode.RequestLock;
            _data.LockTimeout = OneSecondTimeout;
            _data.TransferConditions = transferConditions;
            var actual = _target.TransferConditionsMet(_data);
            Assert.AreEqual(lockSuccess, actual);
        }
    }
}
