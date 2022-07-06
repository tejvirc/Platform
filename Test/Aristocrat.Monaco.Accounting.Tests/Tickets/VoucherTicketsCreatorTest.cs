namespace Aristocrat.Monaco.Accounting.Tests.Tickets
{
    using System;
    using System.Globalization;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.Currency;
    using Contracts;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for VoucherTicketsCreatorTest
    /// </summary>
    [TestClass]
    public class VoucherTicketsCreatorTest
    {
        private const string LocaleDateFormat = "yyyy-MM-dd";

        // Mock Services
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

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.BankId", It.IsAny<string>())).Returns("1");
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.EgmPosition", It.IsAny<string>())).Returns("1");
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.MachineId", It.IsAny<uint>())).Returns((uint)1);
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.SerialNumber", It.IsAny<string>())).Returns("1");
            _propertiesManager.Setup(mock => mock.GetProperty("Cabinet.ZoneName", It.IsAny<string>()))
                .Returns("Test Zone");
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<object>()))
                .Returns(10000.0);
            _propertiesManager.Setup(mock => mock.GetProperty("Locale.PlayerTicket.Locale", It.IsAny<string>()))
                .Returns(CultureInfo.CurrentCulture);
            _propertiesManager.Setup(mock => mock.GetProperty("Printer.PaperLow", It.IsAny<bool>())).Returns(false);
            _propertiesManager.Setup(mock => mock.GetProperty("System.Version", It.IsAny<string>())).Returns("1.0.0.0");
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine1", It.IsAny<string>()))
                .Returns("Test Line 1");
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine2", It.IsAny<string>()))
                .Returns("Test Line 2");
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTextLine3", It.IsAny<string>()))
                .Returns("Test Line 3");
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTitleCash", It.IsAny<string>()))
                .Returns("Test Title Cash");
            _propertiesManager.Setup(mock => mock.GetProperty("TicketProperty.TicketTitleNonCash", It.IsAny<string>()))
                .Returns("Test Title Non Cash");
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.LocalizationPlayerTicketDateFormat, It.IsAny<string>()))
                .Returns(LocaleDateFormat)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageZoneOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPageBankOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ConfigWizardIdentityPagePositionOverride, It.IsAny<IdentityFieldOverride>()))
                .Returns((IdentityFieldOverride)null);

            // set up currency
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);

            TicketCurrencyExtensions.PlayerTicketLocale = cultureName;
            TicketCurrencyExtensions.SetCultureInfo(
                cultureName,
                new CultureInfo(cultureName),
                new CultureInfo(cultureName),
                "Cent",
                "Cents",
                true,
                true,
                minorUnitSymbol
            );

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict, true);
            _time.Setup(mock => mock.GetLocationTime(It.IsAny<DateTime>()))
                .Returns(new DateTime(2012, 7, 22, 19, 45, 0, 0));

            _time.Setup(
                    mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), ApplicationConstants.DefaultDateTimeFormat))
                .Returns(
                    (DateTime dateTime) => dateTime.ToString("G", CultureInfo.CurrentCulture));

             CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture, null, null, true, true, minorUnitSymbol);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void TestDayExpiration()
        {
            const int DeviceId = 11;
            long Amount = 199;
            DateTime nowTime = DateTime.Now;
            const AccountType AccountType = AccountType.Promo;
            const string Barcode = "999888777666555444";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var transaction = new VoucherOutTransaction(
                DeviceId,
                nowTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            _propertiesManager.Setup(m => m.GetProperty("DemonstrationMode", false)).Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketSelectable, new[] { CultureInfo.CurrentCulture.Name })).Returns(It.IsAny<string[]>())
                .Verifiable();
            _propertiesManager.Setup(m => m.SetProperty("TicketProperty.TicketTitleCash", "CASHOUT TICKET"))
                .Verifiable();
            _propertiesManager.Setup(
                    m => m.SetProperty("TicketProperty.TicketTitleCashReprint", "CASHOUT TICKET REPRINT"))
                .Verifiable();
            _propertiesManager.Setup(
                    m => m.SetProperty("TicketProperty.TicketTitleNonCashReprint", "PLAYABLE ONLY REPRINT"))
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.ReprintLoggedVoucherTitleOverride", false))
                .Returns(true)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.TicketTitleCashReprint, It.IsAny<string>()))
                .Returns("CASHOUT REPRINT")
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.TicketTitleNonCashReprint, It.IsAny<string>()))
                .Returns("PLAYABLE ONLY REPRINT")
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.TicketBarcodeLength, It.IsAny<object>()))
                .Returns(18)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.AllowCashWinTicket, It.IsAny<object>()))
                .Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.VoucherOfflineNotify, It.IsAny<object>()))
                .Returns(true)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Sas.ValidationComPort", It.IsAny<object>()))
                .Returns(1)
                .Verifiable();

            _printer.Setup(mock => mock.GetCharactersPerLine(true, 0)).Returns(80).Verifiable(); // Landscape

            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutReprintTicket(transaction)["value"]);
            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutRestrictedReprintTicket(transaction)["value"]);
            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutRestrictedTicket(transaction)["value"]);
            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutTicket(transaction)["value"]);
        }

        [TestMethod]
        public void TestDateExpiration()
        {
            const int DeviceId = 11;
            long Amount = 199;
            DateTime nowTime = DateTime.Now;
            const AccountType AccountType = AccountType.Promo;
            const string Barcode = "999888777666555444";
            const int Expiration = 07081992;
            const string ManualVerification = "999888777666555444";

            var transaction = new VoucherOutTransaction(
                DeviceId,
                nowTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            _propertiesManager.Setup(m => m.GetProperty("DemonstrationMode", false)).Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationPlayerTicketSelectable, new[] { CultureInfo.CurrentCulture.Name })).Returns(It.IsAny<string[]>())
                .Verifiable();
            _propertiesManager.Setup(m => m.SetProperty("TicketProperty.TicketTitleCash", "CASHOUT TICKET"))
                .Verifiable();
            _propertiesManager.Setup(
                    m => m.SetProperty("TicketProperty.TicketTitleCashReprint", "CASHOUT TICKET REPRINT"))
                .Verifiable();
            _propertiesManager.Setup(
                    m => m.SetProperty("TicketProperty.TicketTitleNonCashReprint", "PLAYABLE ONLY REPRINT"))
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty("Cabinet.ReprintLoggedVoucherTitleOverride", false))
                .Returns(true)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.TicketTitleCashReprint, It.IsAny<string>()))
                .Returns("CASHOUT REPRINT")
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.TicketTitleNonCashReprint, It.IsAny<string>()))
                .Returns("PLAYABLE ONLY REPRINT")
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.TicketBarcodeLength, It.IsAny<object>()))
                .Returns(18)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.AllowCashWinTicket, It.IsAny<object>()))
                .Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.VoucherOfflineNotify, It.IsAny<object>()))
                .Returns(true)
                .Verifiable();
            _propertiesManager.Setup(mock => mock.GetProperty("Sas.ValidationComPort", It.IsAny<object>()))
                .Returns(1)
                .Verifiable();

            _printer.Setup(mock => mock.GetCharactersPerLine(true, 0)).Returns(80).Verifiable(); // Landscape

            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutReprintTicket(transaction)["value"]);
            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutRestrictedReprintTicket(transaction)["value"]);
            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutRestrictedTicket(transaction)["value"]);
            Assert.AreEqual("$0.02", VoucherTicketsCreator.CreateCashOutTicket(transaction)["value"]);
        }
    }
}