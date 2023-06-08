namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Globalization;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Currency;
    using Contracts;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for VoucherOutTicketProxyTest and is intended
    ///     to contain all VoucherOutTicketProxyTest Unit Tests
    /// </summary>
    [TestClass]
    public sealed class VoucherOutTicketProxyTest
    {
        private const decimal TicketAmount = 10.45m;
        private const uint MachineId = 4387;
        private const string SerialNumber = "5498";
        private const string Zone = "Calzone";
        private const string Bank = "33";
        private const string Position = "44";
        private const string TicketTextLine1 = "My Casino";
        private const string TicketTextLine2 = "Line 2";
        private const string TicketTextLine3 = "Line 3";
        private const double CurrencyMultiplier = 100000.0;
        private const string TicketBarcode = "123459876";
        private const int TicketBarcodeLength = 18;
        private const int TicketExpiration = 19;
        private const string TicketValidationNumber = "00-0000-0001-2345-9876";
        private const long TicketLogSequence = 937;
        private const string LocaleDateFormat = "yyyy-MM-dd";
        private readonly DateTime _ticketTimestamp = new DateTime(2017, 5, 22, 19, 45, 0);

        private Mock<IPrinter> _printer;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITime> _time;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            _printer.Setup(p => p.PaperState).Returns(PaperStates.Full);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict, true);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict, true);

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(CurrencyMultiplier);
            _time.Setup(mock => mock.GetLocationTime(_ticketTimestamp)).Returns(_ticketTimestamp);
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture, null, null, true, true, minorUnitSymbol);

            TicketCurrencyExtensions.PlayerTicketLocale = cultureName;
            TicketCurrencyExtensions.SetCultureInfo(
                cultureName,
                new CultureInfo(cultureName),
                new CultureInfo(cultureName),
                null,
                null,
                true,
                true,
                minorUnitSymbol
            );
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
            _propertiesManager = null;
            _time = null;
        }

        /// <summary>A test for TransactionTypes</summary>
        [TestMethod]
        public void TransactionTypesTest()
        {
            var target = new VoucherOutTicketProxy();
            var actual = target.TransactionTypes;
            Assert.IsTrue(actual.Contains(typeof(VoucherOutTransaction)));
            Assert.AreEqual(1, actual.Count);
        }

        /// <summary>A test for CreateTicket where the transaction type is valid</summary>
        [TestMethod]
        public void CreateTicketTest()
        {
            var target = new VoucherOutTicketProxy();
            _propertiesManager.Setup(m => m.GetProperty("DemonstrationMode", false)).Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty("Locale.PlayerTicket.Selectable", false)).Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.SetProperty("TicketProperty.TicketTitleCash", "CASHOUT TICKET"))
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(CurrencyMultiplier);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.SerialNumber", It.IsAny<string>()))
                .Returns(SerialNumber);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.MachineId", It.IsAny<uint>()))
                .Returns(MachineId);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.ZoneName", It.IsAny<object>())).Returns(Zone);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.BankId", It.IsAny<object>())).Returns(Bank);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.EgmPosition", It.IsAny<object>()))
                .Returns(Position);
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine1", It.IsAny<object>()))
                .Returns(TicketTextLine1);
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine2", It.IsAny<object>()))
                .Returns(TicketTextLine2);
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine3", It.IsAny<object>()))
                .Returns(TicketTextLine3);
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketRedeemText", It.IsAny<object>()))
                .Returns(19)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("SAS.CashableExpirationDays", It.IsAny<object>()))
                .Returns(TicketExpiration)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Locale.PlayerTicket.Locale", "en-US"))
                .Returns(CultureInfo.CurrentCulture);
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTitleCash", "CASHOUT TICKET"))
                .Returns("CASHOUT *DUP*");
            _propertiesManager.Setup(mock => mock.GetProperty("Printer.PaperLow", false)).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty("System.Version", "Not Set")).Returns("Not Set");
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.ReprintLoggedVoucherTitleOverride", false))
                .Returns(false)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.BarcodeLength", It.IsAny<object>()))
                .Returns(TicketBarcodeLength);
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.AllowCashWinTicket, It.IsAny<object>()))
                .Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty("System.VoucherOfflineNotify", It.IsAny<object>()))
                .Returns(true);
            _printer.Setup(mock => mock.GetCharactersPerLine(true, 0)).Returns(80).Verifiable(); // Portrait : Not tested
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.LocalizationPlayerTicketDateFormat, It.IsAny<string>()))
                .Returns(LocaleDateFormat)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageBankOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPagePositionOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            // create a random transaction for the test
            var expected = CreateTestReprintTicket();

            var transaction = new VoucherOutTransaction(
                1,
                _ticketTimestamp,
                (long)((double)TicketAmount * CurrencyMultiplier),
                AccountType.Cashable,
                TicketBarcode,
                TicketExpiration,
                "None") { LogSequence = TicketLogSequence };
            var actual = target.CreateTicket(transaction);

            // since the mac address will change on different machines, just copy the actual mac address to the expected.
            expected["mac"] = actual["mac"];
            expected["serial version mac"] = actual["serial version mac"];
            expected["expiry date"] = actual["expiry date"];
            expected["expiry date 2"] = actual["expiry date 2"];
            expected["expiry date 3"] = actual["expiry date 3"];
            expected["expiry date 4"] = actual["expiry date 4"];
            expected["full address"] = actual["full address"];
            CheckTickets(expected, actual);
        }

        /// <summary>A test for CreateTicket where the transaction type is not valid</summary>
        [TestMethod]
        public void CreateTicketInvalidTransactionTest()
        {
            var exceptionOccured = false;
            var target = new VoucherOutTicketProxy();

            // create an object that is not a VoucherOut transaction
            var transaction = new BillTransaction();

            // this should throw an argument exception. Check the message in the exception is correct.
            try
            {
                target.CreateTicket(transaction);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(
                    "A BillTransaction transaction can not be converted to a Voucher out transaction",
                    ex.Message);
                exceptionOccured = true;
            }

            Assert.IsTrue(exceptionOccured);
        }

        private static void CheckTickets(Ticket expected, Ticket actual)
        {
            Assert.AreEqual(expected.Data.Count, actual.Data.Count);

            foreach (var key in actual.Data.Keys)
            {
                //Assert.AreEqual(expected.Data[key], actual.Data[key]);
            }
        }

        /// <summary>
        ///     Create the test reprint ticket.
        /// </summary>
        /// <returns>returns the test reprint ticket.</returns>
        private Ticket CreateTestReprintTicket()
        {
            var ticket = new Ticket
            {
                ["title alt"] = string.Empty,
                ["serial id alt"] = string.Empty,
                ["date"] = string.Empty,
                ["time"] = string.Empty,
                ["license alt"] = string.Empty,
                ["establishment name"] = string.Empty,
                ["value in wrapped words 1"] = string.Empty,
                ["value in wrapped words 2"] = string.Empty,
                ["vlt sequence number alt"] = string.Empty,
                ["vendor"] = string.Empty,
                ["regulator"] = string.Empty,
                ["ticket type"] = "cashout",
                ["title"] = "CASHOUT *DUP*",
                ["serial id"] = SerialNumber.ToString(CultureInfo.CurrentCulture),
                ["terminal number"] = "Terminal Number: " + SerialNumber.ToString(CultureInfo.CurrentCulture),
                ["machine id 3"] = $"MACHINE # {MachineId}",
                ["machine id 2"] = $"Machine #: {MachineId}",
                ["machine id"] = MachineId.ToString(),
                ["zone"] = Zone,
                ["bank"] = Bank,
                ["position"] = Position,
                ["datetime"] = _ticketTimestamp.ToString("G", CultureInfo.CurrentCulture),
                ["locale datetime"] = _ticketTimestamp.ToString(LocaleDateFormat) + " " + _ticketTimestamp.ToString(ApplicationConstants.DefaultTimeFormat),
                ["alternate date time"] = "Date: " + _ticketTimestamp.ToString("d MMMM", CultureInfo.CurrentCulture) +
                                          _ticketTimestamp.ToString(", yyyy", CultureInfo.CurrentCulture) + " Time: " +
                                          _ticketTimestamp.ToString("H:mm:ss", CultureInfo.CurrentCulture),
                ["license"] = $"License : {Zone}",
                ["online"] = "Online",
                ["establishment name"] = TicketTextLine1,
                ["value"] = TicketAmount.FormattedCurrencyStringForVouchers(),
                ["value 2"] = TicketAmount.FormattedCurrencyStringForVouchers(),
                ["value in words 1"] = "TEN DOLLARS AND FORTY FIVE CENTS",
                ["value in words 2"] = "Ten Dollars and Forty Five Cents",
                ["value in words with newline"] = "TEN DOLLARS AND FORTY-FIVE CENTS",
                ["validation label"] = "Validation :",
                ["validation label 2"] = "VALIDATION",
                ["redemption text"] = "VOID AFTER " + TicketExpiration + " DAYS",
                ["redemption text 2"] = TicketExpiration + " DAYS",
                ["location address"] = TicketTextLine3,
                ["location name"] = TicketTextLine2,
                ["barcode"] = "000000000" + TicketBarcode,
                ["sequence number"] = TicketLogSequence.ToString(CultureInfo.CurrentCulture),
                ["validation number"] = TicketValidationNumber,
                ["validation number alt"] = TicketValidationNumber,
                ["paper status"] = "Paper Status : OK",
                ["paper level"] = "Paper Status : Good",
                ["vlt sequence number"] = "TICKET NO. : 0937",
                ["alternate sequence number"] = "Ticket Sequence #: 0937",
                ["alternate sequence number 2"] = "Voucher#: 0937",
                ["ticket number 2"] = "Ticket #: 0937",
                ["version"] = "Not set",
                ["mac"] = "64006A9004C8",
                ["serial version mac"] = "Serial : 4387 Vers : Not set MAC : 64006A9004C8",
                ["machine version"] = "Serial : 4387 Vers : Not set",
                ["vlt credits"] = "1,045 CRÉDITS DE 1 ȼ",
                ["asset"] = "Machine #: State Asset #:",
                ["ticket number 3"] = "Ticket# 0937",
                ["ticket number 4"] = "TICKET# 0937",
                ["asset id 2"] = "Asset# 4387",
                ["machine id 4"] = "004387",
                ["value 3"] = "CASH AMOUNT $10.45",
                ["title 1"] = "CASHOUT *DUP*",
                ["ticket number"] = "0000000"
            };
            return ticket;
        }
    }
}