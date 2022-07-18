namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sas.Client.Eft;
    using Aristocrat.Sas.Client.EFT;
    using Aristocrat.Sas.Client.Eft.Response;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;

    [TestClass]
    public class LP28TransferLogsParserTest
    {
        const int ExpectedTotalByteArraySize = SasConstants.EftHistoryLogsSize * SasConstants.EftBytesSizeOfSingleHistoryLog + 2;

        private LP28TransferLogsParser _target;

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ShouldThrowExceptionIfReturnNullFromHandlerTest()
        {
            var theParser = new LP28TransferLogsParser();
            theParser.Parse(new List<byte>());
        }

        [TestMethod]
        public void CommandTest()
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftTransactionLogsResponse(new IEftHistoryLogEntry[0]));
            _target = new LP28TransferLogsParser();
            _target.InjectHandler(handler.Object);
            Assert.AreEqual(LongPoll.EftSendTransferLogs, _target.Command);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Never);
        }

        [TestMethod]
        public void ShouldIncludeCommandsIfNoResultsReturnedTest()
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftTransactionLogsResponse(new IEftHistoryLogEntry[0]));
            _target = new LP28TransferLogsParser();
            _target.InjectHandler(handler.Object);

            var commands = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftSendTransferLogs };
            var response = _target.Parse(commands).ToArray();
            Assert.AreEqual(ExpectedTotalByteArraySize, response.Length);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Once);
            Assert.AreEqual(TestConstants.SasAddress, response[0], $"Should have correct Address field");
            Assert.AreEqual((byte)LongPoll.EftSendTransferLogs, response[1], "Should have correct Command field");
        }

        [TestMethod]
        public void ShouldReturnDefaultValuesIfThereIsNoLogsTest()
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftTransactionLogsResponse(new List<IEftHistoryLogEntry>().ToArray()));
            _target = new LP28TransferLogsParser();
            _target.InjectHandler(handler.Object);

            var commands = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftSendTransferLogs };
            var responseByteArray = _target.Parse(commands).ToArray();
            Assert.AreEqual(ExpectedTotalByteArraySize, responseByteArray.Length);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Once);
            Assert.AreEqual(TestConstants.SasAddress, responseByteArray[0], "Should have correct Address field");
            Assert.AreEqual((byte)LongPoll.EftSendTransferLogs, responseByteArray[1], "Should have correct Command field");

            int index = 2; //log data start index
            for (; index < ExpectedTotalByteArraySize;)
            {
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
                var amountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
                index += SasConstants.Bcd10Digits;
                Assert.IsTrue(amountResult.validBcd);
                Assert.AreEqual((ulong)0, amountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.RequestedTransactionAmount)} field");
            }
        }

        [TestMethod]
        public void ShouldIncludeOneValidLogIfThereIsOnlyOneLogTest()
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>>(MockBehavior.Strict);
            var theLogsReturned = new List<IEftHistoryLogEntry>();
            var theLog = new Mock<IEftHistoryLogEntry>();
            theLog.SetupGet(x => x.Command).Returns(LongPoll.EftTransferCashableCreditsToMachine);
            theLog.SetupGet(x => x.TransactionNumber).Returns(0x01);
            theLog.SetupGet(x => x.Acknowledgement).Returns(true);
            theLog.SetupGet(x => x.ReportedTransactionStatus).Returns(TransactionStatus.EgmOutOfService);
            theLog.SetupGet(x => x.RequestedTransactionAmount).Returns(10000);
            theLogsReturned.Add(theLog.Object);

            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftTransactionLogsResponse(theLogsReturned.ToArray()));
            _target = new LP28TransferLogsParser();
            _target.InjectHandler(handler.Object);

            var commands = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftSendTransferLogs };
            var responseByteArray = _target.Parse(commands).ToArray();

            Assert.AreEqual(ExpectedTotalByteArraySize, responseByteArray.Length);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Once);

            //verify first log in the response
            int index = 2; //log data start index
            Assert.AreEqual((byte)theLog.Object.Command, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
            Assert.AreEqual((byte)theLog.Object.TransactionNumber, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
            Assert.AreEqual((byte)(theLog.Object.Acknowledgement ? 1 : 0), responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
            Assert.AreEqual((byte)theLog.Object.ReportedTransactionStatus, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
            var amountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
            index += SasConstants.Bcd10Digits;
            Assert.IsTrue(amountResult.validBcd);
            Assert.AreEqual(theLog.Object.RequestedTransactionAmount, amountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.RequestedTransactionAmount)} field");

            //verify the results
            //From the 11th: one log is 9 bytes, plus Command and Address = 9 + 1 + 1 = 11
            //index start with 0
            for (; index < responseByteArray.Length;)
            {
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
                var theAmountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
                index += SasConstants.Bcd10Digits;
                Assert.IsTrue(theAmountResult.validBcd);
                Assert.AreEqual((ulong)0, theAmountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionAmount)} field");
            }
        }


        /// <summary>
        /// Test the amount field for Type U Commands
        /// </summary>
        /// <param name="longPollCommand"></param>
        [TestMethod]
        [DataRow(LongPoll.EftTransferCashAndNonCashableCreditsToHost, DisplayName = "Test Amount field for Type U " + nameof(LongPoll.EftTransferCashAndNonCashableCreditsToHost) + " command")]
        [DataRow(LongPoll.EftTransferPromotionalCreditsToHost, DisplayName = "Test Amount field for Type U " + nameof(LongPoll.EftTransferPromotionalCreditsToHost) + " command")]
        public void ShouldReturnCorrectAmountForTypeUCommandsTest(LongPoll longPollCommand)
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>>(MockBehavior.Strict);
            var theLogsReturned = new List<IEftHistoryLogEntry>();
            var theLog = new Mock<IEftHistoryLogEntry>();
            theLog.SetupGet(x => x.Command).Returns(longPollCommand);
            theLog.SetupGet(x => x.TransactionNumber).Returns(0x01);
            theLog.SetupGet(x => x.Acknowledgement).Returns(true);
            theLog.SetupGet(x => x.ReportedTransactionStatus).Returns(TransactionStatus.EgmOutOfService);
            theLog.SetupGet(x => x.ReportedTransactionAmount).Returns(10000);
            theLog.SetupGet(x => x.TransferType).Returns(EftTransferType.Out);
            theLogsReturned.Add(theLog.Object);

            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftTransactionLogsResponse(theLogsReturned.ToArray()));
            _target = new LP28TransferLogsParser();
            _target.InjectHandler(handler.Object);

            var commands = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftSendTransferLogs };
            var responseByteArray = _target.Parse(commands).ToArray();

            Assert.AreEqual(ExpectedTotalByteArraySize, responseByteArray.Length);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Once);

            //verify first log in the response
            int index = 2; //log data start index
            Assert.AreEqual((byte)theLog.Object.Command, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
            Assert.AreEqual((byte)theLog.Object.TransactionNumber, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
            Assert.AreEqual((byte)(theLog.Object.Acknowledgement ? 1 : 0), responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
            Assert.AreEqual((byte)theLog.Object.ReportedTransactionStatus, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
            var amountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
            index += SasConstants.Bcd10Digits;
            Assert.IsTrue(amountResult.validBcd);
            Assert.AreEqual(theLog.Object.ReportedTransactionAmount, amountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionAmount)} field");

            //verify the results
            //From the 11th: one log is 9 bytes, plus Command and Address = 9 + 1 + 1 = 11
            //index start with 0
            for (; index < responseByteArray.Length;)
            {
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
                var theAmountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
                index += SasConstants.Bcd10Digits;
                Assert.IsTrue(theAmountResult.validBcd);
                Assert.AreEqual((ulong)0, theAmountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionAmount)} field");
            }
        }

        /// <summary>
        /// Test the amount field for Type D commands
        /// Can use parameter in above test (set type U or D as parameters), but just encapauste too many logics (too many if-else)
        /// so, just create another test for Type D
        /// </summary>
        /// <param name="longPollCommand"></param>
        [TestMethod]
        [DataRow(LongPoll.EftTransferPromotionalCreditsToMachine, DisplayName = "Test Amount field for Type D " + nameof(LongPoll.EftTransferPromotionalCreditsToMachine) + " command")]
        [DataRow(LongPoll.EftTransferCashableCreditsToMachine, DisplayName = "Test Amount field for Type D " + nameof(LongPoll.EftTransferCashableCreditsToMachine) + " command")]
        public void ShouldReturnCorrectAmountForTypeDCommandsTest(LongPoll longPollCommand)
        {
            var handler = new Mock<ISasLongPollHandler<EftTransactionLogsResponse, LongPollData>>(MockBehavior.Strict);
            var theLogsReturned = new List<IEftHistoryLogEntry>();
            var theLog = new Mock<IEftHistoryLogEntry>();
            theLog.SetupGet(x => x.Command).Returns(longPollCommand);
            theLog.SetupGet(x => x.TransactionNumber).Returns(0x01);
            theLog.SetupGet(x => x.Acknowledgement).Returns(true);
            theLog.SetupGet(x => x.ReportedTransactionStatus).Returns(TransactionStatus.EgmOutOfService);
            theLog.SetupGet(x => x.RequestedTransactionAmount).Returns(10000);
            theLogsReturned.Add(theLog.Object);

            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(new EftTransactionLogsResponse(theLogsReturned.ToArray()));
            _target = new LP28TransferLogsParser();
            _target.InjectHandler(handler.Object);

            var commands = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EftSendTransferLogs };
            var responseByteArray = _target.Parse(commands).ToArray();

            Assert.AreEqual(ExpectedTotalByteArraySize, responseByteArray.Length);
            handler.Verify(m => m.Handle(It.IsAny<LongPollData>()), Times.Once);

            //verify first log in the response
            int index = 2; //log data start index
            Assert.AreEqual((byte)theLog.Object.Command, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
            Assert.AreEqual((byte)theLog.Object.TransactionNumber, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
            Assert.AreEqual((byte)(theLog.Object.Acknowledgement ? 1 : 0), responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
            Assert.AreEqual((byte)theLog.Object.ReportedTransactionStatus, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
            var amountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
            index += SasConstants.Bcd10Digits;
            Assert.IsTrue(amountResult.validBcd);
            Assert.AreEqual(theLog.Object.RequestedTransactionAmount, amountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionAmount)} field");

            //verify the results
            //From the 11th: one log is 9 bytes, plus Command and Address = 9 + 1 + 1 = 11
            //index start with 0
            for (; index < responseByteArray.Length;)
            {
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Command)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.TransactionNumber)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.Acknowledgement)} field");
                Assert.AreEqual(SasConstants.EftDefaultByteValue, responseByteArray[index++], $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionStatus)} field");
                var theAmountResult = Utilities.FromBcdWithValidation(responseByteArray.Skip(index).Take(SasConstants.Bcd10Digits).ToArray());
                index += SasConstants.Bcd10Digits;
                Assert.IsTrue(theAmountResult.validBcd);
                Assert.AreEqual((ulong)0, theAmountResult.number, $"Should have correct {nameof(IEftHistoryLogEntry.ReportedTransactionAmount)} field");
            }
        }
    }
}