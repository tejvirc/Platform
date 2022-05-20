namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Aristocrat.Monaco.Sas.Storage.Models;

    [TestClass]
    public class LP3DSendCashOutTicketInformationHandlerTest
    {
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IPropertiesManager> _propertiesManager;

        private const long TransactionAmount = 801250000;
        private const string ValidationNumber = "33761280";
        private const long DefaultTransactionAmount = 0;
        private const long DefaultValidationNumber = 0;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Loose);
        }

        [TestMethod]
        public void CommandsTest()
        {
            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.None });

            var target = new LP3DSendCashOutTicketInformationHandler(_transactionHistory.Object, _propertiesManager.Object);

            var commands = target.Commands;
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual(LongPoll.SendCashOutTicketInformation, commands[0]);
        }

        [TestMethod]
        [DataRow(
            SasValidationType.None,
            true,
            DisplayName = "Return transaction when configured for None Validation")]
        [DataRow(
            SasValidationType.SecureEnhanced,
            false,
            DisplayName = "Return transaction when configured for Secure Validation")]
        [DataRow(
            SasValidationType.System,
            false,
            DisplayName = "Return transaction when configured for System Validation")]
        public void HandleTest(SasValidationType validationType, bool shouldReturn)
        {
            _transactionHistory.Setup(x => x.RecallTransactions()).Returns(new List<ITransaction>().OrderBy(x => x));
            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { ValidationType = validationType });

            var target = new LP3DSendCashOutTicketInformationHandler(_transactionHistory.Object, _propertiesManager.Object);
            var output = target.Handle(null);

            if (shouldReturn)
            {
                Assert.AreEqual(DefaultValidationNumber, output.ValidationNumber);
                Assert.AreEqual(DefaultTransactionAmount, output.TicketAmount);
            }
            else
            {
                Assert.IsNull(output);
            }
        }

        [TestMethod]
        public void HandleVoucherOutTest()
        {
            var transaction = new VoucherOutTransaction(
                0,
                new DateTime(),
                TransactionAmount,
                AccountType.Cashable,
                ValidationNumber,
                0,
                0,
                string.Empty
            );

            var transactionList = new List<ITransaction> { transaction }.OrderBy(x => x);

            _transactionHistory.Setup(x => x.RecallTransactions()).Returns(transactionList);
            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.None });

            var target = new LP3DSendCashOutTicketInformationHandler(_transactionHistory.Object, _propertiesManager.Object);
            var output = target.Handle(null);

            Assert.AreEqual(TransactionAmount.MillicentsToCents(), output.TicketAmount);
            Assert.AreEqual(Convert.ToInt64(ValidationNumber), output.ValidationNumber);
        }

        [TestMethod]
        public void HandleHandPayTest()
        {
            var transaction = new HandpayTransaction(
                0,
                new DateTime(),
                0,
                0,
                0,
                HandpayType.CancelCredit,
                true,
                Guid.Empty
            )
            {
                Barcode = ValidationNumber,
                KeyOffCashableAmount = TransactionAmount
            };

            var transactionList = new List<ITransaction> { transaction }.OrderBy(x => x);

            _transactionHistory.Setup(x => x.RecallTransactions()).Returns(transactionList);
            _propertiesManager
                .Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.None });

            var target = new LP3DSendCashOutTicketInformationHandler(_transactionHistory.Object, _propertiesManager.Object);
            var output = target.Handle(null);

            Assert.AreEqual(TransactionAmount.MillicentsToCents(), output.TicketAmount);
            Assert.AreEqual(Convert.ToInt64(ValidationNumber), output.ValidationNumber);
        }
    }
}