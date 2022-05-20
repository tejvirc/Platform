namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using System;
    using System.Globalization;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for the AftTransferProvider class
    /// </summary>
    [TestClass]
    public class SasAftReceiptCreatorTest
    {
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IServiceManager> _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
        private readonly Mock<IAftRegistrationProvider> _aftRegistration = new Mock<IAftRegistrationProvider>(MockBehavior.Default);
        private AftData _aftData;
        private string _ticketContent;
        private const string TestLocation = "Test Location";
        private const string TestAddress1 = "Test Address1";
        private const string TestAddress2 = "Test Address2";
        private const uint TestAssetNumber = 12;
        private const uint TestCurrencyMultiplier = 100;
        private const uint TestSasPosIdKey = 3;
        private const ulong TestAftDebitTransactionFee = 23ul;
        private const string TestAftTransferReceiptInHouseLine1 = "Test In House 1";
        private const string TestAftTransferReceiptInHouseLine2 = "Test In House 2";
        private const string TestAftTransferReceiptInHouseLine3 = "Test In House 3";
        private const string TestAftTransferReceiptInHouseLine4 = "Test In House 4";
        private const string TestAftTransferReceiptDebitLine1 = "Test Debit 1";
        private const string TestAftTransferReceiptDebitLine2 = "Test Debit 2";
        private const string TestAftTransferReceiptDebitLine3 = "Test Debit 3";
        private const string TestAftTransferReceiptDebitLine4 = "Test Debit 4";
        private const string TestDateFormat = "MM/dd/yyyy";
        private const string TestTransferSource = "Test Transfer Source";
        private readonly DateTime _testReceiptTime = DateTime.Now;
        private const string TestTransactionId = "Test Transaction Id 47";
        private const string TestPatronName = "Test Patron Name";
        private const string TestPatronAccount = "Test Patron Account";
        private const ulong TestCashableAmount = 1000;
        private const ulong TestRestrictedAmount = 900;
        private const ulong TestNonRestrictedAmount = 800;
        private const ulong TestAccountBalance = 123456;
        private const string TestDebitCardNumber = "5146 55468 7084 0684";
        private const ulong TestDebitAmount = 9876;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptLocationLine, It.IsAny<string>())).Returns(TestLocation);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptAddressLine1, It.IsAny<string>())).Returns(TestAddress1);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptAddressLine2, It.IsAny<string>())).Returns(TestAddress2);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(TestAssetNumber);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<int>())).Returns(TestCurrencyMultiplier);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptInHouseLine1, It.IsAny<string>())).Returns(TestAftTransferReceiptInHouseLine1);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptInHouseLine2, It.IsAny<string>())).Returns(TestAftTransferReceiptInHouseLine2);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptInHouseLine3, It.IsAny<string>())).Returns(TestAftTransferReceiptInHouseLine3);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptInHouseLine4, It.IsAny<string>())).Returns(TestAftTransferReceiptInHouseLine4);
            _aftRegistration.Setup(x => x.PosId).Returns(TestSasPosIdKey);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptDebitLine1, It.IsAny<string>())).Returns(TestAftTransferReceiptDebitLine1);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptDebitLine2, It.IsAny<string>())).Returns(TestAftTransferReceiptDebitLine2);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptDebitLine3, It.IsAny<string>())).Returns(TestAftTransferReceiptDebitLine3);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferReceiptDebitLine4, It.IsAny<string>())).Returns(TestAftTransferReceiptDebitLine4);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketDateFormat, It.IsAny<string>())).Returns(TestDateFormat);
            _serviceManager.Setup(m => m.GetService<IPropertiesManager>()).Returns(_propertiesManager.Object);

            CurrencyExtensions.SetCultureInfo(CultureInfo.CurrentCulture, null, null, true, true, "c");

            _aftData = new AftData();
        }

        [DataRow(AftTransferType.TransferUnknown)]
        [DataRow(AftTransferType.HostToGameBonusCoinOut)]
        [DataRow(AftTransferType.HostToGameBonusJackpot)]
        [DataTestMethod]
        public void ConstructorBadTransferTypeTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            Ticket ticket = SasAftReceiptCreator.CreateAftReceipt(_aftData, _aftRegistration.Object);
            Assert.IsNull(ticket);
        }

        [DataRow(AftTransferType.HostToGameInHouse)]
        [DataRow(AftTransferType.HostToGameInHouseTicket)]
        [DataRow(AftTransferType.HostToGameDebit)]
        [DataRow(AftTransferType.HostToGameDebitTicket)]
        [DataRow(AftTransferType.GameToHostInHouse)]
        [DataRow(AftTransferType.GameToHostInHouseWin)]
        [DataTestMethod]
        public void CommonLinesTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            ProcessTicket();


            // Line 1
            Assert.IsTrue(_ticketContent.Contains(TestLocation));

            // Line 2
            Assert.IsTrue(_ticketContent.Contains(TestAddress1));

            // Line 3
            Assert.IsTrue(_ticketContent.Contains(TestAddress2));

            // Line 6
            Assert.IsTrue(_ticketContent.Contains(TestTransferSource));

            // Line 8
            Assert.IsTrue(_ticketContent.Contains(_testReceiptTime.ToString($"{TestDateFormat} {ApplicationConstants.DefaultTimeFormat}", CultureInfo.InvariantCulture)));

            // Line 10
            Assert.IsTrue(_ticketContent.Contains(TestAssetNumber.ToString()));

            // Line 15
            Assert.IsTrue(_ticketContent.Contains(TestTransactionId));

            // Line 16
            Assert.IsTrue(_ticketContent.Contains(((TestCashableAmount + TestNonRestrictedAmount) / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.HostToGameInHouse)]
        [DataRow(AftTransferType.HostToGameInHouseTicket)]
        [DataRow(AftTransferType.HostToGameDebit)]
        [DataRow(AftTransferType.HostToGameDebitTicket)]
        [DataRow(AftTransferType.GameToHostInHouse)]
        [DataRow(AftTransferType.GameToHostInHouseWin)]
        [DataTestMethod]
        public void CommonLinesNoCashTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            _aftData.CashableAmount = 0;
            _aftData.NonRestrictedAmount = 0;
            ProcessTicket();

            // Line 16
            Assert.IsFalse(_ticketContent.Contains(((TestCashableAmount + TestNonRestrictedAmount) / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.HostToGameInHouse)]
        [DataRow(AftTransferType.HostToGameInHouseTicket)]
        [DataRow(AftTransferType.GameToHostInHouse)]
        [DataRow(AftTransferType.GameToHostInHouseWin)]
        [DataTestMethod]
        public void InHouseLinesTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            ProcessTicket();

            // Line 12
            Assert.IsTrue(_ticketContent.Contains(TestPatronName));

            // Line 13
            Assert.IsTrue(_ticketContent.Contains(TestPatronAccount));

            // Line 17
            Assert.IsTrue(_ticketContent.Contains((TestRestrictedAmount / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));

            // Lines 21-24
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptInHouseLine1));
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptInHouseLine2));
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptInHouseLine3));
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptInHouseLine4));
        }


        [DataRow(AftTransferType.HostToGameInHouse)]
        [DataRow(AftTransferType.HostToGameInHouseTicket)]
        [DataRow(AftTransferType.GameToHostInHouse)]
        [DataRow(AftTransferType.GameToHostInHouseWin)]
        [DataTestMethod]
        public void InHouseLinesNoRestrictedAmountTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            _aftData.RestrictedAmount = 0;
            ProcessTicket();

            // Line 17
            Assert.IsFalse(_ticketContent.Contains((TestRestrictedAmount / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.HostToGameDebit)]
        [DataRow(AftTransferType.HostToGameDebitTicket)]
        [DataTestMethod]
        public void DebitLinesTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            ProcessTicket();

            // Line 11
            Assert.IsTrue(_ticketContent.Contains(TestSasPosIdKey.ToString()));

            // Line 13
            Assert.IsTrue(_ticketContent.Contains(TestDebitCardNumber));

            // Line 18
            Assert.IsTrue(_ticketContent.Contains((TestAftDebitTransactionFee / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));

            // Line 19 with provided total
            Assert.IsTrue(_ticketContent.Contains((TestDebitAmount / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));

            // Lines 21-24
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptDebitLine1));
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptDebitLine2));
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptDebitLine3));
            Assert.IsTrue(_ticketContent.Contains(TestAftTransferReceiptDebitLine4));
        }

        [DataRow(AftTransferType.HostToGameDebit)]
        [DataRow(AftTransferType.HostToGameDebitTicket)]
        [DataTestMethod]
        public void DebitLinesNoDebitTotalProvidedTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            _aftData.ReceiptData.DebitAmount = 0;
            ProcessTicket();

            // Line 19 without provided total
            Assert.IsTrue(_ticketContent.Contains(((TestCashableAmount + TestNonRestrictedAmount + TestAftDebitTransactionFee) / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.HostToGameDebit)]
        [DataRow(AftTransferType.HostToGameDebitTicket)]
        [DataTestMethod]
        public void DebitLinesNoDebitTotalNorFeeProvidedTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            _aftData.ReceiptData.DebitAmount = 0;
            _aftData.ReceiptData.TransactionFee = 0;
            ProcessTicket();

            // Line 19 without provided total nor fee
            Assert.IsFalse(_ticketContent.Contains(((TestCashableAmount + TestNonRestrictedAmount + TestAftDebitTransactionFee) / (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.HostToGameInHouse)]
        [DataRow(AftTransferType.HostToGameInHouseTicket)]
        [DataTestMethod]
        public void HostToGameInHouseLinesTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            ProcessTicket();

            // Line 19 transfer from player account to game so subtract from balance
            Assert.IsTrue(_ticketContent.Contains(((TestAccountBalance - TestCashableAmount - TestRestrictedAmount - TestNonRestrictedAmount)/ (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.HostToGameInHouse)]
        [DataRow(AftTransferType.HostToGameInHouseTicket)]
        [DataTestMethod]
        public void HostToGameInHouseLinesNotEnoughInAccountTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            _aftData.AccountBalance = 0;
            ProcessTicket();

            // Line 19 without enough balance for transfer
            Assert.IsFalse(_ticketContent.Contains(((TestAccountBalance - TestCashableAmount - TestRestrictedAmount - TestNonRestrictedAmount)/ (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [DataRow(AftTransferType.GameToHostInHouse)]
        [DataRow(AftTransferType.GameToHostInHouseWin)]
        [DataTestMethod]
        public void GameToHostInHouseLinesTest(AftTransferType transferType)
        {
            SetupAftData(transferType);
            ProcessTicket();

            // Line 19 transfer from game to player account so add to balance
            Assert.IsTrue(_ticketContent.Contains(((TestAccountBalance + TestCashableAmount + TestRestrictedAmount + TestNonRestrictedAmount)/ (decimal)TestCurrencyMultiplier).FormattedCurrencyString()));
        }

        [TestMethod]
        public void HostToGameInHouseTransferDescriptiveTextTest()
        {
            SetupAftData(AftTransferType.HostToGameInHouse);
            ProcessTicket();

            // Line 5, 16, 17
            MockLocalization.Localizer.Verify(m => m.GetString("TransferToGame"));
            MockLocalization.Localizer.Verify(m => m.GetString("CashInText"));
            MockLocalization.Localizer.Verify(m => m.GetString("PromoIn"));
        }

        [TestMethod]
        public void HostToGameInHouseTicketTransferDescriptiveTextTest()
        {
            SetupAftData(AftTransferType.HostToGameInHouseTicket);
            ProcessTicket();

            // Line 5, 16, 17
            MockLocalization.Localizer.Verify(m => m.GetString("TransferToGame"));
            MockLocalization.Localizer.Verify(m => m.GetString("CashTicket"));
            MockLocalization.Localizer.Verify(m => m.GetString("PromoTicket"));
        }

        [TestMethod]
        public void HostToGameDebitTransferDescriptiveTextTest()
        {
            SetupAftData(AftTransferType.HostToGameDebit);
            ProcessTicket();

            // Line 5, 16
            MockLocalization.Localizer.Verify(m => m.GetString("DebitCardWithdrawal"));
            MockLocalization.Localizer.Verify(m => m.GetString("DebitIn"));
        }

        [TestMethod]
        public void HostToGameDebitTicketTransferDescriptiveTextTest()
        {
            SetupAftData(AftTransferType.HostToGameDebitTicket);
            ProcessTicket();

            // Line 5, 16
            MockLocalization.Localizer.Verify(m => m.GetString("DebitCardWithdrawal"));
            MockLocalization.Localizer.Verify(m => m.GetString("DebitTicket"));
        }

        [TestMethod]
        public void GameToHostInHouseTransferDescriptiveTextTest()
        {
            SetupAftData(AftTransferType.GameToHostInHouse);
            ProcessTicket();

            // Line 5, 16, 17
            MockLocalization.Localizer.Verify(m => m.GetString("TransferFromGame"));
            MockLocalization.Localizer.Verify(m => m.GetString("CashOutText"));
            MockLocalization.Localizer.Verify(m => m.GetString("PromoOut"));
        }

        [TestMethod]
        public void GameToHostInHouseWinTransferDescriptiveTextTest()
        {
            SetupAftData(AftTransferType.GameToHostInHouseWin);
            ProcessTicket();

            // Line 5, 16
            MockLocalization.Localizer.Verify(m => m.GetString("TransferFromGame"));
            MockLocalization.Localizer.Verify(m => m.GetString("CashOutText"));
        }

        [TestMethod]
        public void ReceiptDateTimeNotProvidedTest()
        {
            SetupAftData(AftTransferType.GameToHostInHouseWin);
            _aftData.ReceiptData.ReceiptTime = DateTime.MinValue;
            _aftData.TransactionDateTime = DateTime.MaxValue;
            ProcessTicket();

            // Line 8
            Assert.IsTrue(_ticketContent.Contains("12/31/9999 23:59:59"));
        }

        [TestMethod]
        public void ReceiptDateTimeProvidedTest()
        {
            SetupAftData(AftTransferType.GameToHostInHouseWin);
            _aftData.ReceiptData.ReceiptTime = new DateTime(2020, 11, 23, 10, 15, 24); // Nov 23, 2020, 10:15:24
            _aftData.TransactionDateTime = DateTime.MaxValue;
            ProcessTicket();

            // Line 8
            Assert.IsTrue(_ticketContent.Contains("11/23/2020 10:15:24"));
        }

        private void SetupAftData(AftTransferType transferType)
        {
            _aftData.TransferType = transferType;
            _aftData.AccountBalance = TestAccountBalance;
            _aftData.CashableAmount = TestCashableAmount;
            _aftData.NonRestrictedAmount = TestNonRestrictedAmount;
            _aftData.ReceiptData.DebitCardNumber = TestDebitCardNumber;
            _aftData.ReceiptData.DebitAmount = TestDebitAmount;
            _aftData.ReceiptData.PatronAccount = TestPatronAccount;
            _aftData.ReceiptData.PatronName = TestPatronName;
            _aftData.ReceiptData.ReceiptTime = _testReceiptTime;
            _aftData.ReceiptData.TransactionFee = TestAftDebitTransactionFee;
            _aftData.ReceiptData.TransferSource = TestTransferSource;
            _aftData.RestrictedAmount = TestRestrictedAmount;
            _aftData.TransactionId = TestTransactionId;
        }

        private void ProcessTicket()
        {
            Ticket ticket = SasAftReceiptCreator.CreateAftReceipt(_aftData, _aftRegistration.Object);
            _ticketContent = ticket["left"] + ticket["center"] + ticket["right"];
        }
    }
}
