namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     Contains the unit tests for the AftTransferInHouseFromHostToTicket class
    /// </summary>
    [TestClass]
    public class AftTransferInHouseFromHostToTicketTest
    {
        private AftTransferInHouseFromHostToTicket _target;
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);

        private readonly AftResponseData _data = new AftResponseData
        {
            TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
            TransferStatus = AftTransferStatusCode.UnexpectedError
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(0);
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(_data);
            _aftProvider.Setup(x => x.TransactionIdUnique).Returns(false);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(false);
            _aftProvider.Setup(x => x.TransferFailure).Returns(false);
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(0);
            _aftProvider.Setup(x => x.IsPrinterAvailable).Returns(false);
            _aftProvider.SetupErrorHandler(_data);

            _target = new AftTransferInHouseFromHostToTicket(_aftProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullProviderTest()
        {
            _target = new AftTransferInHouseFromHostToTicket(null);
        }

        [TestMethod]
        public void ProcessNoPrinterTest()
        {
            _aftProvider.Setup(m => m.IsPrinterAvailable).Returns(false);
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            var result = _target.Process(new AftResponseData());

            Assert.AreEqual(AftTransferStatusCode.TransferToTicketDeviceNotAvailable, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
        }

        // remove this test once we support host to ticket transfers
        [TestMethod]
        public void ProcessNotSupportedTest()
        {
            _aftProvider.Setup(m => m.IsPrinterAvailable).Returns(true);
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            var result = _target.Process(new AftResponseData());

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
        }
    }
}
