namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Sas.Eft;
    using Aristocrat.Monaco.Sas.EftTransferProvider;
    using Aristocrat.Sas.Client.Eft;
    using Contracts.Eft;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Localization.Properties;
    using Test.Common;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Sas.Client;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Application.Contracts;

    /// <summary>
    ///     Contains unit tests for the LP63TransferPromoCreditHandler class.
    /// </summary>
    [TestClass]
    public class LP69TransferCashableCreditHandlerTests
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

        private LP69TransferCashableCreditHandler ConstructHandler(bool nullEftStateController, bool nullEftTransferProvider, bool nullEventBus)
        {
            return new LP69TransferCashableCreditHandler(
                nullEftStateController ? null : _stateController.Object,
                nullEftTransferProvider ? null : _transferProvider.Object,
                nullEventBus ? null : _eventBus.Object
                );
        }

        private LP69TransferCashableCreditHandler InitializeHandler()
        {
            (ulong, bool) returnVar = (50, false);
            _transferProvider.Setup(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>())).Returns(returnVar);
            _transferProvider.Setup(m => m.DoEftOn(It.IsAny<string>(), It.IsAny<AccountType>(), It.IsAny<ulong>())).Returns(true);
            _stateController.Setup(m => m.RecoverIfRequired(It.IsAny<IEftTransferHandler>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ProtocolsInitializedEvent>>()));

            return new LP69TransferCashableCreditHandler(
                _stateController.Object,
                _transferProvider.Object,
                _eventBus.Object
                );
        }
        private EftTransferData ComposeTransferData(string transactionNumber = "01", bool ack = false, ulong amount = 50u)
        {
            return new EftTransferData { TransactionNumber = int.Parse(transactionNumber), Acknowledgement = ack, TransferAmount = amount };
        }
        private EftTransactionResponse ComposeResponseData(string transactionNumber = "01",
            bool ack = false,
            ulong amount = (ulong)50,
            TransactionStatus status = TransactionStatus.OperationSuccessful)
        {
            return new EftTransactionResponse { TransactionNumber = int.Parse(transactionNumber), Acknowledgement = ack, TransferAmount = amount, Status = status };
        }

        [TestMethod]
        public void CommandTest()
        {
            var handler = InitializeHandler();
            var expected = new List<LongPoll> { LongPoll.EftTransferCashableCreditsToMachine };
            Assert.IsTrue(handler.Commands.Count == 1);
            Assert.AreEqual(expected[0], handler.Commands[0]);
        }

        [TestMethod]
        public void TestHandle()
        {
            var handler = InitializeHandler();
            EftTransferData dummyData = ComposeTransferData();
            _stateController.Setup(m => m.Handle(It.IsAny<EftTransferData>(), It.IsAny<IEftTransferHandler>())).Returns(ComposeResponseData());

            var testResponse = handler.Handle(dummyData);

            _transferProvider.Verify(m => m.DoEftOn(It.IsAny<string>(), AccountType.Cashable, It.IsAny<ulong>()), Times.Never());
            _transferProvider.Verify(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>()), Times.Never());
            _stateController.Verify(m => m.Handle(It.IsAny<EftTransferData>(), handler), Times.Once());
        }

        [TestMethod]
        public void TestFullAmountCheckTransferAmount()
        {
            var handler = InitializeHandler();
            ulong initialTransferData = 50;

            var (availableAmount, exceeded) =  handler.CheckTransferAmount(initialTransferData);

            _transferProvider.Verify(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>()), Times.Once());
            _transferProvider.Verify(m => m.DoEftOn(It.IsAny<string>(), It.IsAny<AccountType>(), It.IsAny<ulong>()), Times.Never());
            Assert.AreEqual(availableAmount, initialTransferData);
            Assert.IsFalse(exceeded);
        }

        [TestMethod]
        public void TestPartialAmountCheckTransferAmount()
        {
            var handler = InitializeHandler();
            ulong initialTransferData = 50;
            _transferProvider.Setup(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>())).Returns((25u, true));

            var (availableAmount, exceeded) = handler.CheckTransferAmount(initialTransferData);

            _transferProvider.Verify(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>()), Times.Once());
            _transferProvider.Verify(m => m.DoEftOn(It.IsAny<string>(), It.IsAny<AccountType>(), It.IsAny<ulong>()), Times.Never());
            ulong expectedAmount = 25;
            Assert.AreEqual(availableAmount, expectedAmount);
            Assert.IsTrue(exceeded);
        }

        [TestMethod]
        public void TestTransferAmountExceededCheckTransferAmount()
        {
            var handler = InitializeHandler();
            ulong initialTransferData = 50;
            _transferProvider.Setup(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>())).Returns((0u, true));

            var (availableAmount, exceeded) = handler.CheckTransferAmount(initialTransferData);

            _transferProvider.Verify(m => m.GetAcceptedTransferInAmount(It.IsAny<ulong>()), Times.Once());
            _transferProvider.Verify(m => m.DoEftOn(It.IsAny<string>(), It.IsAny<AccountType>(), It.IsAny<ulong>()), Times.Never());
            ulong expectedAmount = 0;
            Assert.AreEqual(availableAmount, expectedAmount);
            Assert.IsTrue(exceeded);
        }

        [TestMethod]
        public void TestProcessTransfer()
        {
            var handler = InitializeHandler();
            ulong secondPhaseTransferData = 50;

            bool transferResult = handler.ProcessTransfer(secondPhaseTransferData, 1);

            _transferProvider.Verify(m => m.DoEftOn(It.IsAny<string>(), AccountType.Cashable, 50), Times.Once());
            Assert.IsTrue(transferResult);
        }

        [TestMethod]
        public void TestStopTransferIfDisabledByHost()
        {
            var handler = InitializeHandler();

            Assert.IsTrue(handler.StopTransferIfDisabledByHost());
        }

        [TestMethod]
        public void TestGetDisableString()
        {
            var handler = InitializeHandler();

            var testString = handler.GetDisableString();

            Assert.AreEqual(testString, Localizer.For(CultureFor.Player).GetString(ResourceKeys.EftTransferInInProgress));
        }
    }
}