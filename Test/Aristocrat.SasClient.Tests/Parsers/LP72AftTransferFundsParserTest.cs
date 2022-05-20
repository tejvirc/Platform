namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.Aft;
    using Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Contains tests for the LP72AftTransferFundsParser class
    /// </summary>
    [TestClass]
    public class LP72AftTransferFundsParserTest
    {
        private readonly Mock<ISasLongPollHandler<AftResponseData, AftTransferData>> _handler = new Mock<ISasLongPollHandler<AftResponseData, AftTransferData>>();
        private LP72AftTransferFundsParser _target;

        private const byte TransactionIndex = 0x22;

        private readonly byte[] _notValidTransferResponse =
        {
            TestConstants.SasAddress, 0x72, 65, 0x00, (byte)AftTransferStatusCode.NotAValidTransferFunction,
            (byte)ReceiptStatus.NoReceiptRequested,
            0,
            0, 0, 0, 0, 0, // cashable
            0, 0, 0, 0, 0, // restricted
            0, 0, 0, 0, 0, // non-restricted
            0, // transfer flag
            0, 0, 0, 0,  // asset number - sent LSB first
            0, // transaction id length
            0x01, 0x01, 0x00, 0x01, // date 
            0x00, 0x00, 0x00, // time
            0, 0, 0, 0, // expiration
            00, 00, // pool id - sent LSB first
            8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
            8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
            8, 0, 0, 0, 0, 0, 0, 0, 0 // cumulative amounts 
        };

        private readonly byte[] _notAValidAmountOrExpirationResponse =
        {
            TestConstants.SasAddress, 0x72, 65, 0x00, (byte)AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate,
            (byte)ReceiptStatus.NoReceiptRequested, 0, 0, 0, 0, 0, 0, // cashable
            0, 0, 0, 0, 0, // restricted
            0, 0, 0, 0, 0, // non-restricted
            0, // transfer flag
            0, 0, 0, 0, // asset number - sent LSB first
            0, // transaction id length
            0x01, 0x01, 0x00, 0x01, // date 
            0x00, 0x00, 0x00, // time
            0, 0, 0, 0, // expiration
            00, 00, // pool id - sent LSB first
            8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
            8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
            8, 0, 0, 0, 0, 0, 0, 0, 0 // cumulative amounts 
        };

        private readonly byte[] _notAValidTransactionIdResponse =
        {
            TestConstants.SasAddress, 0x72, 65, 0x00, (byte)AftTransferStatusCode.TransactionIdNotValid,
            (byte)ReceiptStatus.NoReceiptRequested, 0, 0, 0, 0, 0, 0, // cashable
            0, 0, 0, 0, 0, // restricted
            0, 0, 0, 0, 0, // non-restricted
            0, // transfer flag
            0, 0, 0, 0, // asset number - sent LSB first
            0, // transaction id length
            0x01, 0x01, 0x00, 0x01, // date 
            0x00, 0x00, 0x00, // time
            0, 0, 0, 0, // expiration
            00, 00, // pool id - sent LSB first
            8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
            8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
            8, 0, 0, 0, 0, 0, 0, 0, 0 // cumulative amounts 
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP72AftTransferFundsParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void ParseWithLengthWrongTest()
        {
            var longPoll = new Collection<byte> { TestConstants.SasAddress, 0x72, 0x01, 0x00, 0x00, TestConstants.FakeCrc, TestConstants.FakeCrc };

            CollectionAssert.AreEqual(_notValidTransferResponse, _target.Parse(longPoll).ToList());
        }

        [TestMethod]
        public void ParseCancelTransferTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                0x72,
                0x01,
                (byte)AftTransferCode.CancelTransferRequest,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x02, 0x00, (byte)AftTransferStatusCode.TransferCanceledByHost
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferCanceledByHost, TransactionIndex = 0x00
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);

            CollectionAssert.AreEqual(expected, _target.Parse(longPoll).ToList());
        }

        [TestMethod]
        public void ParseInterrogationRequestStatusOnlyTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                0x72,
                0x02,
                (byte)AftTransferCode.InterrogationRequestStatusOnly,
                TransactionIndex,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x44, TransactionIndex, (byte)AftTransferStatusCode.FullTransferSuccessful,
                (byte)ReceiptStatus.NoReceiptRequested, (byte)AftTransferType.HostToGameInHouse,
                0, 0, 0, 0x12, 0x34, // cashable
                0, 0, 0, 0x23, 0x45, // restricted
                0, 0, 0, 0x34, 0x56, // non-restricted
                0, // transfer flag
                0x44, 0x33, 0x22, 0x11,  // asset number
                3, (byte)'v', (byte)'g', (byte)'t', // transaction id length, transaction id
                0x01, 0x01, 0x20, 0x00, // date
                0x15, 0x30, 0x20, // time
                0x01, 0x20, 0x20, 0x00, // expiration
                01, 00, // pool id
                08, 0, 0, 0, 0, 0, 0, 0x12, 0x34, // cumulative cash
                08, 0, 0, 0, 0, 0, 0, 0x23, 0x45, // cumulative restricted
                08, 0, 0, 0, 0, 0, 0, 0x34, 0x56  // cumulative non-restricted
            };

            SetupHandlerMock(TransactionIndex);

            CollectionAssert.AreEqual(expected, _target.Parse(longPoll).ToList());
        }

        [TestMethod]
        public void ParseInterrogationRequestTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                0x72,
                0x02,
                (byte)AftTransferCode.InterrogationRequest,
                TransactionIndex,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x44, TransactionIndex, (byte)AftTransferStatusCode.FullTransferSuccessful,
                (byte)ReceiptStatus.NoReceiptRequested, (byte)AftTransferType.HostToGameInHouse,
                0, 0, 0, 0x12, 0x34, // cashable
                0, 0, 0, 0x23, 0x45, // restricted
                0, 0, 0, 0x34, 0x56, // non-restricted
                0, // transfer flag
                0x44, 0x33, 0x22, 0x11,  // asset number - sent LSB first
                3, (byte)'v', (byte)'g', (byte)'t', // transaction id length, transaction id
                0x01, 0x01, 0x20, 0x00, // date
                0x15, 0x30, 0x20, // time
                0x01, 0x20, 0x20, 0x00, // expiration
                01, 00, // pool id - sent LSB first
                08, 0, 0, 0, 0, 0, 0, 0x12, 0x34, // cumulative cash
                08, 0, 0, 0, 0, 0, 0, 0x23, 0x45, // cumulative restricted
                08, 0, 0, 0, 0, 0, 0, 0x34, 0x56  // cumulative non-restricted
            };

            SetupHandlerMock(TransactionIndex);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x8A, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x52, // receipt data length
                0x00, // Transfer source/dest
                0x14, // string length
                (byte)'F', (byte)'r', (byte)'o', (byte)'m', (byte)' ', (byte)'P', (byte)'r', (byte)'i', (byte)'m',
                (byte)'a', (byte)'r', (byte)'y', (byte)' ', (byte)'A', (byte)'c', (byte)'c', (byte)'o', (byte)'u',
                (byte)'n', (byte)'t', 0x01, // date and time
                0x07, // length
                0x07, 0x17, 0x20, 0x03, 0x01, 0x02, 0x03, // date/time 7/17/2003 1:02:03
                0x10, // Patron name
                0x0A, // length
                (byte)'I', (byte)'m', (byte)'a', (byte)' ', (byte)'W', (byte)'i', (byte)'n', (byte)'n', (byte)'e',
                (byte)'r', 0x11, // Patron Account number
                0x0C, // length
                (byte)'1', (byte)'2', (byte)'3', (byte)'A', (byte)'B', (byte)'C', (byte)'4', (byte)'5', (byte)'6',
                (byte)'D', (byte)'E', (byte)'F', 0x13, // Account Balance
                0x05, // length
                0x00, 0x00, 0x01, 0x00, 0x00, // $100 in cents
                0x41, // Debit card number (last 4 digits)
                0x02, // length
                0x12, 0x34, 0x42, // transaction fee in cents
                0x05, // length
                0x00, 0x00, 0x00, 0x00, 0x32, // 32 cents fee
                0x43, // total debit amount
                0x05, // length
                0x00, 0x00, 0x00, 0x01, 0x32, // $1.32
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x44, TransactionIndex, (byte)AftTransferStatusCode.FullTransferSuccessful,
                (byte)ReceiptStatus.NoReceiptRequested, (byte)AftTransferType.HostToGameInHouse,
                0, 0, 0, 0x12, 0x34, // cashable
                0, 0, 0, 0x23, 0x45, // restricted
                0, 0, 0, 0x34, 0x56, // non-restricted
                0, // transfer flag
                0x44, 0x33, 0x22, 0x11,  // asset number - sent LSB first
                3, (byte)'v', (byte)'g', (byte)'t', // transaction id length, transaction id
                0x01, 0x01, 0x20, 0x00, // date
                0x15, 0x30, 0x20, // time
                0x01, 0x20, 0x20, 0x00, // expiration
                01, 00, // pool id - sent LSB first
                08, 0, 0, 0, 0, 0, 0, 0x12, 0x34, // cumulative cash
                08, 0, 0, 0, 0, 0, 0, 0x23, 0x45, // cumulative restricted
                08, 0, 0, 0, 0, 0, 0, 0x34, 0x56  // cumulative non-restricted
            };

            SetupHandlerMock(TransactionIndex);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandInvalidCashableTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x08, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x1A, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notAValidAmountOrExpirationResponse, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandInvalidRestrictedTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x0D, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x1A, 0x22, 0x33, 0x44, 0x55, // restricted amount
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notAValidAmountOrExpirationResponse, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandInvalidNonRestrictedTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x12, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x11, 0x22, 0x33, 0x44, 0x55, // restricted amount
                0x1A, 0x22, 0x33, 0x44, 0x55, // non-restricted amount
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notAValidAmountOrExpirationResponse, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandInvalidExpirationTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x33, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x11, 0x22, 0x33, 0x44, 0x55, // restricted amount
                0x11, 0x22, 0x33, 0x44, 0x55, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x0A, 0x00, 0x00, 0x10, // invalid expiration
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notAValidAmountOrExpirationResponse, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandInvalidTransactionTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x30, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x11, 0x22, 0x33, 0x44, 0x55, // restricted amount
                0x11, 0x22, 0x33, 0x44, 0x55, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x00, // transaction id length
                0x0A, 0x00, 0x00, 0x10, // invalid expiration
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notAValidTransactionIdResponse, actual);
        }

        [TestMethod]
        public void GetAftTransferDataFromCommandInvalidTransactionLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x33, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x11, 0x22, 0x33, 0x44, 0x55, // restricted amount
                0x11, 0x22, 0x33, 0x44, 0x55, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0xF3, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x0A, 0x00, 0x00, 0x10, // invalid expiration
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notAValidTransactionIdResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataZeroLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x38, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x52, // receipt data length
                0x00, // Transfer source/dest
                0x0, // string length
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataTransactionSourceToBigTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x52, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x19, // receipt data length
                0x00, // Transfer source/dest
                0x18, // string length
                (byte)'F', (byte)'r', (byte)'o', (byte)'m', (byte)' ', (byte)'P', (byte)'r', (byte)'i', (byte)'m',
                (byte)'a', (byte)'r', (byte)'y', (byte)' ', (byte)'A', (byte)'c', (byte)'c', (byte)'o', (byte)'u',
                (byte)'n', (byte)'t', (byte)'1', (byte)'2', (byte)'3', (byte)'4', 0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataDateTimeWrongLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x40, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x08, // receipt data length
                0x01, // date and time
                0x06, // wrong length
                0x17, 0x20, 0x03, 0x01, 0x02, 0x03, // date/time 7/17/2003 1:02:03
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataInvalidLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x51, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x18, // receipt data length
                0x10, // Patron name
                0xF7, // length
                (byte)'I', (byte)'m', (byte)'a', (byte)' ', (byte)'W', (byte)'i', (byte)'n', (byte)'n', (byte)'e',
                (byte)'r', (byte)' ', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)' ', (byte)'2',
                (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataMissingFieldsTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x38, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x2, // receipt data length
                0x10, // Patron name
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataNameToBigTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x51, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x18, // receipt data length
                0x10, // Patron name
                0x17, // length
                (byte)'I', (byte)'m', (byte)'a', (byte)' ', (byte)'W', (byte)'i', (byte)'n', (byte)'n', (byte)'e',
                (byte)'r', (byte)' ', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)' ', (byte)'2',
                (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', 0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataAccountToBigTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x4B, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x13, // receipt data length
                0x11, // Patron Account number
                0x11, // length
                (byte)'1', (byte)'2', (byte)'3', (byte)'A', (byte)'B', (byte)'C', (byte)'4', (byte)'5', (byte)'6',
                (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'7', (byte)'8', (byte)'9', (byte)'0', 0x00,
                0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataAccountBalanceWrongLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x3E, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x06, // receipt data length
                0x13, // Account Balance
                0x04, // wrong length
                0x00, 0x01, 0x00, 0x00, // $100 in cents
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataDebitNumberWrongLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x3B, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x03, // receipt data length
                0x41, // Debit card number (last 4 digits)
                0x01, // wrong length
                0x34, 0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataTransactionFeeWrongLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x3E, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x06, // receipt data length
                0x42, // transaction fee in cents
                0x04, // wrong length
                0x00, 0x00, 0x00, 0x32, // 32 cents fee
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataDebitAmountWrongLengthTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x3E, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x06, // receipt data length
                0x43, // total debit amount
                0x04, // wrong length
                0x00, 0x00, 0x00, 0x01, // 
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void ParseReceiptDataAccountBalanceNotBcdTest()
        {
            var longPoll = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x3F, (byte)AftTransferCode.TransferRequestFullTransferOnly,
                TransactionIndex, (byte)AftTransferType.HostToGameInHouse, 0x11, 0x22, 0x33, 0x44,
                0x55, // cashable amount
                0x00, 0x00, 0x00, 0x12, 0x34, // restricted amount
                0x00, 0x00, 0x00, 0x45, 0x67, // non-restricted amount
                0x00, // transfer flags
                0x00, 0x00, 0x99, 0x99, // asset number 99990000
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x20, // 20 byte registration key
                0x03, // transaction id length
                (byte)'v', (byte)'g', (byte)'t', // transaction id
                0x00, 0x00, 0x00, 0x10, // expiration in 10 days
                0x00, 0x00, // pool id
                0x07, // receipt data length
                0x13, // Account Balance
                0x05, // length
                0x0A, 0x00, 0x01, 0x00, 0x00, // invalid
                0x00, 0x00, // lock timeout
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(_notValidTransferResponse, actual);
        }

        [TestMethod]
        public void GenerateLongPollResponseUnsupportedTransferCodeTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                0x72,
                0x01,
                (byte)AftTransferCode.CancelTransferRequest,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x02, 0x00, (byte)AftTransferStatusCode.UnsupportedTransferCode
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.UnsupportedTransferCode, TransactionIndex = 0x00
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);

            CollectionAssert.AreEqual(expected, _target.Parse(longPoll).ToList());
        }

        [TestMethod]
        public void GenerateLongPollResponseTransferPendingTest()
        {
            var longPoll = new Collection<byte> { TestConstants.SasAddress, 0x72, 0x01,
                (byte)AftTransferCode.CancelTransferRequest, TestConstants.FakeCrc, TestConstants.FakeCrc };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x29, 0x00, (byte)AftTransferStatusCode.TransferPending,
                0, 0,
                0, 0, 0, 0, 0, // cashable
                0, 0, 0, 0, 0, // restricted
                0, 0, 0, 0, 0, // non-restricted
                0, // transfer flag
                0, 0, 0, 0,  // asset number - sent LSB first
                0, // transaction id length
                0, 0, 0, 0, // date 1/1/0001
                0, 0, 0, // time
                0, 0, 0, 0, // expiration
                00, 00, // pool id - sent LSB first
                0, // cumulative amounts not shown
                0,
                0
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending, TransactionIndex = 0x00
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GenerateLongPollResponseAssetDoesNotMatchTest()
        {
            var longPoll = new Collection<byte> { TestConstants.SasAddress, 0x72, 0x01,
                (byte)AftTransferCode.CancelTransferRequest, TestConstants.FakeCrc, TestConstants.FakeCrc };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72, 0x41, 0x00, (byte)AftTransferStatusCode.AssetNumberZeroOrDoesNotMatch,
                0, 0,
                0, 0, 0, 0, 0, // cashable
                0, 0, 0, 0, 0, // restricted
                0, 0, 0, 0, 0, // non-restricted
                0, // transfer flag
                0, 0, 0, 0,  // asset number - sent LSB first
                0, // transaction id length
                0x12, 0x31, 0x99, 0x99, // date 
                0x23, 0x59, 0x59, // time
                0, 0, 0, 0, // expiration
                00, 00, // pool id - sent LSB first
                8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
                8, 0, 0, 0, 0, 0, 0, 0, 0, // cumulative amounts 
                8, 0, 0, 0, 0, 0, 0, 0, 0 // cumulative amounts 
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.AssetNumberZeroOrDoesNotMatch,
                TransactionIndex = 0x00,
                TransactionDateTime = DateTime.MaxValue
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GenerateLongPollResponseDefaultTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                0x72,
                0x01,
                (byte)AftTransferCode.CancelTransferRequest,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var response = new AftResponseData
            {
                TransferStatus = (AftTransferStatusCode)0x02, TransactionIndex = 0x00
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GenerateLongPollResponseNoTransferInfoAvailableTest()
        {
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress,
                0x72,
                0x02,
                (byte)AftTransferCode.InterrogationRequest,
                TransactionIndex,
                TestConstants.FakeCrc,
                TestConstants.FakeCrc
            };

            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x72,
                0x03,  // length
                TransactionIndex,  // transaction index requested above
                (byte)AftTransferStatusCode.NoTransferInfoAvailable,
                (byte)ReceiptStatus.NoReceiptRequested
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.NoTransferInfoAvailable,
                TransactionIndex = TransactionIndex,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);

            var actual = _target.Parse(longPoll).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        private void SetupHandlerMock(byte transactionIndex)
        {
            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                TransactionIndex = transactionIndex,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested,
                TransferType = AftTransferType.HostToGameInHouse,
                CashableAmount = 1234,
                RestrictedAmount = 2345,
                NonRestrictedAmount = 3456,
                TransferFlags = 0,
                AssetNumber = 0x11223344,
                TransactionId = "vgt",
                TransactionDateTime = new DateTime(2000, 1, 1, 15, 30, 20), // 1/1/2000 15:30.20
                Expiration = 01202000, // 1/20/2000
                PoolId = 0001,
                CumulativeCashableAmount = 1234,
                CumulativeRestrictedAmount = 2345,
                CumulativeNonRestrictedAmount = 3456
            };

            _handler.Setup(m => m.Handle(It.IsAny<AftTransferData>())).Returns(response);
        }
    }
}
