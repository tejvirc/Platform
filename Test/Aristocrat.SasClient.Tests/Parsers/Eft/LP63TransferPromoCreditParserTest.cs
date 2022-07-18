﻿namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Sas.Client.EFT;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;

    [TestClass]
    public class LP63TransferPromoCreditParserTest
    {
        private LP63TransferPromoCreditParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionResponse, EftTransferData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<EftTransferData>())).Returns(new EftTransactionResponse { Status = TransactionStatus.OperationSuccessful, TransferAmount = 20 });

            _target = new LP63TransferPromoCreditParser();
            _target.InjectHandler(handler.Object);
        }

        [TestMethod]
        public void CommandTest() => Assert.AreEqual(LongPoll.EftTransferPromotionalCreditsToMachine, _target.Command);

        [TestMethod]
        public void HandleInvalidAckTest()
        {
            const int ACK = 0x09;
            const int TransactionNumber = 0x20;
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftTransferPromotionalCreditsToMachine, TransactionNumber, ACK };
            command.AddRange(Utilities.ToBcd(20, SasConstants.Bcd8Digits));
            var response = _target.Parse(command).ToArray();
            CollectionAssert.AreEqual(command.Take(4).ToArray(), response.Take(4).ToArray());
            Assert.AreEqual(response[4], (byte)TransactionStatus.InvalidAck);
        }

        [TestMethod]
        public void HandleInvalidTransactionNumberTest()
        {
            const int ACK = 0x01;
            const int TransactionNumber = 0x00;
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftTransferPromotionalCreditsToMachine, TransactionNumber, ACK };
            command.AddRange(Utilities.ToBcd(20, SasConstants.Bcd8Digits));
            var response = _target.Parse(command).ToArray();
            CollectionAssert.AreEqual(command.Take(4).ToArray(), response.Take(4).ToArray());
            Assert.AreEqual(response[4], (byte)TransactionStatus.InvalidTransactionNumber);
        }

        [TestMethod]
        public void HandleInvalidTransferAmountTest()
        {
            const int ACK = 0x01;
            const int TransactionNumber = 0x0F;
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftTransferPromotionalCreditsToMachine, TransactionNumber, ACK };
            command.Add(0x20);
            command.Add(0xF9);
            var response = _target.Parse(command).ToArray();
            CollectionAssert.AreEqual(command.Take(4).ToArray(), response.Take(4).ToArray());
            Assert.AreEqual(response[4], (byte)TransactionStatus.ContainsNonBcdData);
        }

        [TestMethod]
        public void HandleTest()
        {
            const int ACK = 0x01;
            const int TransactionNumber = 0x20;
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftTransferPromotionalCreditsToMachine, TransactionNumber, ACK };
            command.AddRange(Utilities.ToBcd(20, SasConstants.Bcd8Digits));
            var response = _target.Parse(command).ToArray();
            CollectionAssert.AreEqual(command.Take(4).ToArray(), response.Take(4).ToArray());
            Assert.AreEqual(response[4], (byte)TransactionStatus.OperationSuccessful);
            (ulong number, bool validBcd) = Utilities.FromBcdWithValidation(response.Skip(5).ToArray());
            Assert.IsTrue(validBcd);
            Assert.AreEqual(20ul, number);
        }
    }
}