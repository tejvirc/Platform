namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Threading;
    using Application.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     Contains unit tests for the AftTransferDebitFromHostToTicket class
    /// </summary>
    [TestClass]
    public class AftTransferDebitFromHostToTicketTest
    {
        private AftTransferDebitFromHostToTicket _target;
        private readonly Mock<IAftRegistrationProvider> _registrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Strict);
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

            _target = new AftTransferDebitFromHostToTicket(_aftProvider.Object, _registrationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullProviderTest()
        {
            _target = new AftTransferDebitFromHostToTicket(null, _registrationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullRegistrationProviderTest()
        {
            _target = new AftTransferDebitFromHostToTicket(_aftProvider.Object, null);
        }

        [TestMethod]
        public void ProcessTest()
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(true);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(true);
            _registrationProvider.Setup(m => m.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(true);
            _aftProvider.Setup(x => x.PosIdZero).Returns(false);
            _aftProvider.Setup(m => m.IsPrinterAvailable).Returns(true);

            var result = _target.Process(_data);

            // NOTE: Once the DoAftToTicket() code is filled in, this should be waiting
            // for AftTransferStatusCode.FullTransferSuccessful or PartialTransferSuccessful
            var timeout = 100;
            while (_data.TransferStatus != AftTransferStatusCode.TransferPending && timeout > 0)
            {
                Thread.Sleep(1);
                timeout--;
            }

            Assert.IsTrue(timeout > 0);
            Assert.AreEqual(AftTransferStatusCode.TransferPending, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPending, result.ReceiptStatus);
        }

        [DataRow(false, true, true, 0, 0, 0, false, AftTransferStatusCode.NotAValidTransferFunction, DisplayName = "Debit Transfers Not Supported")]
        [DataRow(true, false, true, 0, 0, 0, false, AftTransferStatusCode.GamingMachineNotRegistered, DisplayName = "Not Registered")]
        [DataRow(true, true, false, 0, 0, 0, false, AftTransferStatusCode.RegistrationKeyDoesNotMatch, DisplayName = "Registration Key Mismatch")]
        [DataRow(true, true, true, 0, 0, 0, false, AftTransferStatusCode.NoPosId, DisplayName = "Pos Id Zero")]
        [DataRow(true, true, true, 1, 1, 0, false, AftTransferStatusCode.NotAValidTransferFunction, DisplayName = "Restricted Transfer")]
        [DataRow(true, true, true, 1, 0, 1, false, AftTransferStatusCode.NotAValidTransferFunction, DisplayName = "Non Restricted Transfer")]
        [DataRow(true, true, true, 1, 0, 0, false, AftTransferStatusCode.TransferToTicketDeviceNotAvailable, DisplayName = "No Printer")]
        [DataTestMethod]
        public void ProcessErrorConditionsTests(
            bool debitEnabled,
            bool registered,
            bool keyMatches,
            int posId,
            long restricted,
            long nonRestricted,
            bool canPrint,
            AftTransferStatusCode expectedStatus)
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(debitEnabled);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(registered);
            _registrationProvider.Setup(m => m.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(keyMatches);
            _aftProvider.Setup(x => x.PosIdZero).Returns(posId == 0);
            _aftProvider.Setup(m => m.IsPrinterAvailable).Returns(canPrint);
            _data.RestrictedAmount = (ulong)restricted;
            _data.NonRestrictedAmount = (ulong)nonRestricted;
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            var result = _target.Process(_data);

            Assert.AreEqual(expectedStatus, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
        }
    }
}
