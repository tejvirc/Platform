namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Sas.Contracts.Eft;
    using Aristocrat.Monaco.Sas.Eft;
    using Aristocrat.Monaco.Sas.EftTransferProvider;
    using Aristocrat.Monaco.Test.Common;
    using Aristocrat.Sas.Client.Eft;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Localization.Properties;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Sas.Client;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Application.Contracts;

    /// <summary>
    ///     Contains unit tests for the LP6BTransferPromotionalCreditsToHostHandlerTests class.
    /// </summary>
    [TestClass]
    public class LP6BTransferPromotionalCreditsToHostHandlerTests
    {
        private Mock<IEftStateController> _stateController;
        private Mock<IEftTransferProvider> _transferProvider;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _stateController = new Mock<IEftStateController>(MockBehavior.Strict);
            _transferProvider = new Mock<IEftTransferProvider>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        }

        [DataRow(true, false, false, DisplayName = "Null IEftStateController")]
        [DataRow(false, true, false, DisplayName = "Null IEftTransferProvider")]
        [DataRow(false, false, true, DisplayName = "Null IEventBus")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(bool nullEftStateController, bool nullEftTransferProvider, bool nullEventBus)
        {
            _ = ConstructHandler(nullEftStateController, nullEftTransferProvider, nullEventBus);
        }

        private LP6BTransferPromotionalCreditsToHostHandler ConstructHandler(bool nullEftStateController, bool nullEftTransferProvider, bool nullEventBus)
        {
            return new LP6BTransferPromotionalCreditsToHostHandler(
                nullEftStateController ? null : _stateController.Object,
                nullEftTransferProvider ? null : _transferProvider.Object,
                nullEventBus ? null : _eventBus.Object
                );
        }

        private LP6BTransferPromotionalCreditsToHostHandler InitializeHandler()
        {
            _transferProvider.Setup(m => m.GetAcceptedTransferOutAmount(AccountType.NonCash)).Returns((50u, false));
            _transferProvider.Setup(m => m.DoEftOff(It.IsAny<string>(), AccountType.NonCash, It.IsAny<ulong>())).Returns(true);
            _stateController.Setup(m => m.RecoverIfRequired(It.IsAny<IEftTransferHandler>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ProtocolsInitializedEvent>>()));

            return new LP6BTransferPromotionalCreditsToHostHandler(
                _stateController.Object,
                _transferProvider.Object,
                _eventBus.Object
                );
        }
        private EftTransferData ComposeTransferData(string transactionNumber = "01", bool ack = false)
        {
            return new EftTransferData { TransactionNumber = int.Parse(transactionNumber), Acknowledgement = ack };
        }
        private EftTransactionResponse ComposeResponseData(TransactionStatus status = TransactionStatus.OperationSuccessful, ulong amount = 50, string transactionNumber = "01")
        {
            return new EftTransactionResponse { TransferAmount = amount, Status = status };
        }

        [TestMethod]
        public void TestHandle()
        {
            var handler = InitializeHandler();
            EftTransferData dummyData = ComposeTransferData();
            _stateController.Setup(m => m.Handle(It.IsAny<EftTransferData>(), It.IsAny<IEftTransferHandler>())).Returns(It.IsAny<EftTransactionResponse>());

            var testResponse = handler.Handle(dummyData);

            _transferProvider.Verify(m => m.DoEftOff(It.IsAny<string>(), AccountType.NonCash, It.IsAny<ulong>()), Times.Never());
            _transferProvider.Verify(m => m.GetAcceptedTransferOutAmount(It.IsAny<AccountType>()), Times.Never());
            _stateController.Verify(m => m.Handle(It.IsAny<EftTransferData>(), handler), Times.Once());
        }

        [TestMethod]
        public void CommandsTest()
        {
            var handler = InitializeHandler();
            Assert.AreEqual(1, handler.Commands.Count);
            Assert.IsTrue(handler.Commands.Contains(LongPoll.EftTransferPromotionalCreditsToHost));
        }

        [TestMethod]
        public void CheckTransferAmountTest()
        {
            var handler = InitializeHandler();
            ulong initialTransferData = 50;
            _transferProvider.Setup(m => m.GetAcceptedTransferOutAmount(AccountType.NonCash)).Returns((50u, true));
            var testResponse = handler.CheckTransferAmount(initialTransferData);

            _transferProvider.Verify(m => m.GetAcceptedTransferOutAmount(AccountType.NonCash), Times.Once());
            _transferProvider.Verify(m => m.DoEftOff(It.IsAny<string>(), AccountType.NonCash, It.IsAny<ulong>()), Times.Never());
        }

        [TestMethod]
        public void ProcessTransferTest()
        {
            var handler = InitializeHandler();
            EftTransactionResponse secondPhaseTransferData = ComposeResponseData();

            var transferResult = handler.ProcessTransfer(50,01);

            _transferProvider.Verify(m => m.DoEftOff(It.IsAny<string>(), AccountType.NonCash, 50), Times.Once());
            Assert.IsTrue(transferResult);
        }

        [TestMethod]
        public void TestStopTransferIfDisabledByHost()
        {
            var handler = InitializeHandler();

            Assert.IsFalse(handler.StopTransferIfDisabledByHost());
        }

        [TestMethod]
        public void TestGetDisableString()
        {
            var handler = InitializeHandler();

            var testString = handler.GetDisableString();

            Assert.AreEqual(Localizer.For(CultureFor.Player).GetString(ResourceKeys.EftTransferOutInProgress), testString);
        }
    }
}
