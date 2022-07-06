namespace Aristocrat.Monaco.Application.Tests.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Application.Tickets;
    using Contracts.OperatorMenu;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Tickets;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Aristocrat.Monaco.Application.Contracts.Currency;

    /// <summary>
    ///     Unit Tests for PeriodicResetTicketCreator
    /// </summary>
    [TestClass]
    public class PeriodicResetTicketCreatorTest
    {
        private const decimal Millicents = 100000;

        private const decimal ExpectedLifetimeTotalIn = 4321043m;
        private const decimal ExpectedPeriodTotalIn = 123456m;
        private const decimal ExpectedLifetimeTotalOut = 217348m;
        private const decimal ExpectedPeriodTotalOut = 67344m;

        private const decimal LifetimeNet = ExpectedLifetimeTotalIn - ExpectedLifetimeTotalOut;
        private const decimal PeriodNet = ExpectedPeriodTotalIn - ExpectedPeriodTotalOut;

        private const decimal CreditsOnMachine = 150m;
        private const long CurrentBalance = (long)(CreditsOnMachine * Millicents);
        private const int ExpectedPeriodCount = 1;
        private const int ExpectedLifetimeCount = 1000;

        private const string Retailer = "RETAILER";
        private const string RetailerAddress = "RETAILER ADDRESS";
        private const string License = "6000";
        private const string CabinetSerial = "56789";
        private const string SystemVersion = "98765";
        private const uint SystemId = 9;
        private const string VoucherIn = "System.VoucherIn";
        private readonly TextTicketVerificationHelper _validationHelper = new TextTicketVerificationHelper();
        private IEnumerable<int> _billDenoms;
        private IEnumerable<int> _coinDenoms;
        private DateTime _expectedLastMasterClear;
        private DateTime _expectedLastPeriodClear;
        private DateTime _expectedNow;
        private Mock<IServiceManager> _serviceManager;
        private Mock<IMeterManager> _meterManagerMock;
        private Dictionary<string, Mock<IMeter>> _meterMocks;
        private Mock<INoteAcceptor> _noteAcceptorMock;
        private Mock<IPrinter> _printerMock;
        private Mock<IPropertiesManager> _propertiesManager;
        private PeriodicResetTicketCreator _target;
        private Mock<ITime> _timeMock;

        private readonly Dictionary<string, string> _localizationStrings = new Dictionary<string, string>
        {
            { "LOC_Main Door Open Count", "Ouvertures de la porte principale" },
            { "LOC_Cash Door Open Count", "Ouvertures de la boîte à billets" },
            { "LOC_Logic Door Open Count", "Ouvertures de la porte logique" },
            { "LOC_Top Box Door Open Count", "Ouvertures de la boîte secondare" },
            { "LOC_Retailer Access", "Accès au menu Admin" },
            { "LOC_Technician Access", "Accès au menu Technicien" },
            { "LOC_Amount In", "Montant inséré" },
            { "LOC_Amount Out", "Montant payé" },
            { "LOC_Amount Played", "Montant misé" },
            { "LOC_Amount Won", "Montant gagné" },
            { "LOC_Coin In $1", "Pièces de 1 $" },
            { "LOC_Coin In $2", "Pièces de 2 $" },
            { "LOC_Bill In $1", "Billets de 1 $" },
            { "LOC_Bill In $5", "Billets de 5 $" },
            { "LOC_Bill In $10", "Billets de 10 $" },
            { "LOC_Bill In $20", "Billets de 20 $" },
            { "LOC_Bill In $50", "Billets de 50 $" },
            { "LOC_Bill In $100", "Billets de 100 $" },
            { "LOC_VERIFICATION COUPON - PAGE 1", "COUPON DE VÉRIFICATION — PAGE 1" },
            { "LOC_VERIFICATION COUPON - PAGE 2", "COUPON DE VÉRIFICATION — PAGE 2" },
            { "LOC_License:", "Licence :" },
            { "LOC_MAC Address", "Adresse MAC :" },
            { "LOC_Vignette:", "Vignette :" },
            { "LOC_Firmware Version:", "Version Logicielle :" },
            { "LOC_Master", "Cumulatif" },
            { "LOC_Period", "Période" },
            { "LOC_Number of Coins Inserted", "Nombre de pièces insérées" },
            { "LOC_Total - Coin In", "Total des pièces en" },
            { "LOC_Number of Bills Inserted", "Nombre de billets insérés" },
            { "LOC_Total - Bill In", "Total des billets en" },
            { "LOC_Total - Coins and Bills", "Total des insertions (billets et monnaie)" },
            { "LOC_Cashout Vouchers", "Coupons de remboursement" },
            { "LOC_Credits Balance", "Solde des crédits" },
            { "LOC_Paper Level", "Niveau papier" },
            { "LOC_Cashout Ticket", "COUPON DE REMBOURSEMENT" },
            { "LOC_OK", "BON" },
            { "LOC_LOW", "BAS" },
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new PeriodicResetTicketCreator();

            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            // set up currency
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture, null, null, true, true, minorUnitSymbol);

            // Don't care about millisecond precision, makes time verifications easier
            // Mocked interfaces return UTC values
            var now = DateTime.UtcNow;
            _expectedNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            _expectedLastMasterClear = _expectedNow.Subtract(new TimeSpan(365, 0, 0, 0));
            _expectedLastPeriodClear = _expectedNow.Subtract(new TimeSpan(7, 0, 0, 0));

            // But the ticket will pass UTC values to the mock have them converted to local time.
            _timeMock = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _timeMock.SetupSequence(t => t.GetLocationTime(It.IsAny<DateTime>()))
                .Returns(_expectedNow.ToLocalTime())
                .Returns(_expectedLastMasterClear.ToLocalTime())
                .Returns(_expectedLastPeriodClear.ToLocalTime());

            _noteAcceptorMock = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Strict);

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);

            _meterManagerMock = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);

            _billDenoms = new Collection<int>
            {
                1,
                2,
                5,
                10,
                20,
                50,
                100
            };
            _coinDenoms = new Collection<int> { 1, 2 };

            var config = MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<bool>())).Returns(It.IsAny<bool>());
            config.Setup(m => m.GetSetting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(It.IsAny<bool>());
        }

        [TestMethod]
        public void ConstructsTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ImplementsIServiceTest()
        {
            Assert.IsNotNull(_target.ServiceTypes.Select(st => st == typeof(IPeriodicResetTicketCreator)).First());
            Assert.AreEqual("Periodic Reset Ticket Creator", _target.Name);

            // No observable side effects at this time; make sure it doesn't throw.
            _target.Initialize();
        }

        [Ignore]
        [TestMethod]
        public void CreateTicketTest()
        {
            SetupMeterManager(_billDenoms, _coinDenoms);
            SetupPropertiesManager(_billDenoms, _coinDenoms);
            //_propertiesManager.Setup(
            //        mock => mock.GetProperty(
            //            ApplicationConstants.OperatorMenuMetersScreenCoinsDataVisibility,
            //            It.IsAny<bool>()))
            //    .Returns(true).Verifiable();

            //_propertiesManager.Setup(
            //        mock => mock.GetProperty(
            //            ApplicationConstants.OperatorMenuMetersScreenBillDataInCountState,
            //            It.IsAny<bool>()))
            //    .Returns(true).Verifiable();

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        VoucherIn,
                        It.IsAny<bool>()))
                .Returns(true).Verifiable();

            SetupPrinter(PaperStates.Full);

            var ticket = _target.Create();

            VerifyTicketData(ticket, _billDenoms, _coinDenoms);
            VerifyMocks();
        }
        
        [Ignore]
        [TestMethod]
        public void CreateTicketNoCoinDataNoNoteAcceptorTest()
        {
            SetupMeterManager(_billDenoms, _coinDenoms);
            SetupPropertiesManager(_billDenoms, _coinDenoms);

            //_propertiesManager.Setup(
            //        mock => mock.GetProperty(
            //            ApplicationConstants.OperatorMenuMetersScreenCoinsDataVisibility,
            //            It.IsAny<bool>()))
            //    .Returns(false).Verifiable();

            //_propertiesManager.Setup(
            //        mock => mock.GetProperty(
            //            ApplicationConstants.OperatorMenuMetersScreenBillDataInCountState,
            //            It.IsAny<bool>()))
            //    .Returns(true).Verifiable();

            _propertiesManager.Setup(
                    mock => mock.GetProperty(
                        VoucherIn,
                        It.IsAny<bool>()))
                .Returns(true).Verifiable();

            _meterManagerMock.Setup(mock => mock.IsMeterProvided(It.IsAny<string>())).Returns(false);
            _meterManagerMock.Setup(mock => mock.LastMasterClear)
                .Returns(_expectedLastMasterClear)
                .Verifiable();

            _meterManagerMock.Setup(mock => mock.LastPeriodClear)
                .Returns(_expectedLastPeriodClear)
                .Verifiable();

            _serviceManager.Setup(mock => mock.TryGetService<INoteAcceptor>()).Returns((INoteAcceptor)null);

            SetupPrinter(PaperStates.Full);

            var ticket = _target.Create();

            VerifyTicketData(ticket, _billDenoms, _coinDenoms);
        }

        private void SetupMeterManager(IEnumerable<int> billDenoms = null, IEnumerable<int> coinDenoms = null)
        {
            billDenoms = billDenoms ?? new List<int>();
            coinDenoms = coinDenoms ?? new List<int>();

            _meterMocks = new Dictionary<string, Mock<IMeter>>();
            foreach (var billDenom in billDenoms)
            {
                var meterMocksValue = new Mock<IMeter>(MockBehavior.Strict);
                meterMocksValue.Setup(mock => mock.Period).Returns(ExpectedPeriodCount).Verifiable();
                meterMocksValue.Setup(mock => mock.Lifetime).Returns(ExpectedLifetimeCount).Verifiable();
                var meterName = $"BillCount{billDenom}s";
                _meterManagerMock.Setup(mock => mock.IsMeterProvided(meterName)).Returns(true);
                _meterMocks[meterName] = meterMocksValue;
            }

            foreach (var coinDenom in coinDenoms)
            {
                var meterMocksValue = new Mock<IMeter>(MockBehavior.Strict);
                meterMocksValue.Setup(mock => mock.Period).Returns(ExpectedPeriodCount).Verifiable();
                meterMocksValue.Setup(mock => mock.Lifetime).Returns(ExpectedLifetimeCount).Verifiable();
                var meterName = $"CoinCount{coinDenom}s";
                _meterManagerMock.Setup(mock => mock.IsMeterProvided(meterName)).Returns(true);
                _meterMocks[meterName] = meterMocksValue;
            }

            _meterMocks["TotalOut"] = new Mock<IMeter>(MockBehavior.Strict);
            _meterMocks["TotalOut"].Setup(mock => mock.Lifetime)
                .Returns((long)ExpectedLifetimeTotalOut)
                .Verifiable();
            _meterMocks["TotalOut"].Setup(mock => mock.Period)
                .Returns((long)ExpectedPeriodTotalOut)
                .Verifiable();

            _meterMocks["TotalIn"] = new Mock<IMeter>(MockBehavior.Strict);
            _meterMocks["TotalIn"].Setup(mock => mock.Lifetime)
                .Returns((long)ExpectedLifetimeTotalIn)
                .Verifiable();
            _meterMocks["TotalIn"].Setup(mock => mock.Period)
                .Returns((long)ExpectedPeriodTotalIn)
                .Verifiable();

            AddVoucherMeters("VoucherInCashable");
            AddVoucherMeters("VoucherInCashablePromotional");
            AddVoucherMeters("VoucherInNonCashablePromotional");

            _meterManagerMock.Setup(mock => mock.GetMeter(It.IsAny<string>()))
                .Returns(new Func<string, IMeter>(name => _meterMocks[name].Object))
                .Verifiable();

            //Meters report UTC
            _meterManagerMock.Setup(mock => mock.LastMasterClear)
                .Returns(_expectedLastMasterClear)
                .Verifiable();

            _meterManagerMock.Setup(mock => mock.LastPeriodClear)
                .Returns(_expectedLastPeriodClear)
                .Verifiable();
        }

        private void AddVoucherMeters(string meter)
        {
            _meterMocks[meter + "Count"] = new Mock<IMeter>(MockBehavior.Strict);
            _meterMocks[meter + "Count"].Setup(mock => mock.Lifetime)
                .Returns((long)ExpectedLifetimeTotalIn)
                .Verifiable();
            _meterMocks[meter + "Count"].Setup(mock => mock.Period)
                .Returns((long)ExpectedPeriodTotalIn)
                .Verifiable();

            _meterMocks[meter + "Value"] = new Mock<IMeter>(MockBehavior.Strict);
            _meterMocks[meter + "Value"].Setup(mock => mock.Lifetime)
                .Returns((long)ExpectedLifetimeTotalIn)
                .Verifiable();
            _meterMocks[meter + "Value"].Setup(mock => mock.Period)
                .Returns((long)ExpectedPeriodTotalIn)
                .Verifiable();
        }

        private void SetupPrinter(PaperStates paperState)
        {
            _printerMock.Setup(mock => mock.PaperState).Returns(paperState);
        }

        private void SetupPropertiesManager(IEnumerable<int> billDenoms = null, IEnumerable<int> coinDenoms = null)
        {
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(CurrentBalance)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<string>()))
                .Returns(Retailer)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine2, It.IsAny<string>()))
                .Returns("Test Casino city")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine3, It.IsAny<string>()))
                .Returns(RetailerAddress)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.Zone, It.IsAny<string>()))
                .Returns(License)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(CabinetSerial)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns(SystemId)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>()))
                .Returns(SystemVersion)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty("CoinDenominations", It.IsAny<Collection<int>>()))
                .Returns(
                    new Func<string, object, object>(
                        (prop, defaultValue) => defaultValue)
                )
                .Verifiable();

            billDenoms = billDenoms ?? new Collection<int>();
            _propertiesManager.Setup(mock => mock.GetProperty("Denominations", It.IsAny<IEnumerable<int>>()))
                .Returns(billDenoms)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.CurrencyMultiplierKey, It.IsAny<IEnumerable<double>>()))
                .Returns(10000.0)
                .Verifiable();
        }

        private void VerifyTicketData(Ticket ticket, IEnumerable<int> bills = null, IEnumerable<int> coins = null)
        {
            bills = bills ?? new List<int>();
            coins = coins ?? new List<int>();

            CreateExpectedTicket(_validationHelper, bills, coins);

            _validationHelper.VerifyTextTicket(ticket, "PERIODIC RESET");
        }

        private void CreateExpectedTicket(
            TextTicketVerificationHelper helper,
            IEnumerable<int> bills,
            IEnumerable<int> coins)
        {
            /* null indicates there's nothing on this line or 
            in this column we need to validate. All we really care
            about is the data, not the format*/

            var currency = (ulong)(NumberStyles.AllowCurrencySymbol
                                   | NumberStyles.AllowDecimalPoint
                                   | NumberStyles.AllowThousands);

            var currencyLineConversionStyle = Tuple.Create(currency, currency, currency);

            helper.ConversionStyles["Meter Line"] = currencyLineConversionStyle;
            helper.ConversionStyles["Totals Line"] = currencyLineConversionStyle;
            helper.ConversionStyles["Total In Line"] = currencyLineConversionStyle;
            helper.ConversionStyles["Total Out Line"] = currencyLineConversionStyle;
            helper.ConversionStyles["Net Line"] = currencyLineConversionStyle;
            helper.ConversionStyles["Credit Balance Line"] = currencyLineConversionStyle;

            // Header
            //helper.AddTicketLine(); // Separator
            //helper.AddTicketLine(null, $"{Retailer}", null, "Retailer Line");
            //helper.AddTicketLine(null, $"{RetailerAddress}", null, "Retailer Address Line");
            //helper.AddTicketLine(null, null, $"{License}", "License Line");

            // Time stamp should be local time.
            helper.AddTicketLine(
                _expectedNow.ToLocalTime().TimeOfDay,
                null,
                _expectedNow.ToLocalTime().Date,
                "Report Timestamp Line");

            helper.AddTicketLine(); // MAC Address
            helper.AddTicketLine(null, null, $"{SystemId}", "9");
            helper.AddTicketLine(null, null, $"{CabinetSerial}", "Serial/Vignette Line");
            helper.AddTicketLine(null, null, $"{SystemVersion}", "Firmware Version");
            helper.AddTicketLine(); // Separator

            // Meter clears should be in local time.
            helper.AddTicketLine(
                _expectedLastMasterClear.ToLocalTime(),
                null,
                _expectedLastPeriodClear.ToLocalTime(),
                "Clear Timestamps");

            // Coins Section
            helper.AddTicketLine(); // Separator
            var coinsTotals = AddDenominationLines(helper, coins, "Meter Line");

            // Bills Section
            helper.AddTicketLine(); // Separator
            var billsTotals = AddDenominationLines(helper, bills, "Meter Line");

            // Totals Section
            helper.AddTicketLine(); // Separator
            var totalLifetime = coinsTotals.Item1 + billsTotals.Item1;
            var totalPeriod = coinsTotals.Item2 + billsTotals.Item2;
            helper.AddTicketLine(totalLifetime, null, totalPeriod, "Totals Line");

            var roundAndConvert =
                new Func<decimal, decimal>(l => decimal.Round(l / Millicents, 2, MidpointRounding.ToEven));

            helper.AddTicketLine(
                roundAndConvert(ExpectedLifetimeTotalIn),
                null,
                roundAndConvert(ExpectedPeriodTotalIn),
                "Total In Line");
            helper.AddTicketLine(
                roundAndConvert(ExpectedLifetimeTotalOut),
                null,
                roundAndConvert(ExpectedPeriodTotalOut),
                "Total Out Line");
            helper.AddTicketLine(roundAndConvert(LifetimeNet), null, roundAndConvert(PeriodNet), "Net Line");
            helper.AddTicketLine(); // Separator

            // Extra Summary Section
            helper.AddTicketLine("Credit Balance", null, CreditsOnMachine, "Credit Balance Line");

            // Adding this since the ticket creator implements a mapping
            // to resources, instead of reflection or something else.
            string expectedPaperState = null;
            switch (_printerMock.Object.PaperState)
            {
                case PaperStates.Full:
                    expectedPaperState = "Paper Full";
                    break;
                case PaperStates.Low:
                    expectedPaperState = "Paper Low";
                    break;
                case PaperStates.Empty:
                    expectedPaperState = "Paper Out";
                    break;
                case PaperStates.Jammed:
                    expectedPaperState = "Paper Jam";
                    break;
            }

            helper.AddTicketLine(null, null, expectedPaperState, "Paper Status line");
        }

        private Tuple<decimal, decimal> AddDenominationLines(
            TextTicketVerificationHelper helper,
            IEnumerable<int> denoms,
            string desc)
        {
            var denomArray = denoms.ToArray();
            var lifetimeCount = ExpectedLifetimeCount * denomArray.Count();
            var periodCount = ExpectedPeriodCount * denomArray.Count();
            helper.AddTicketLine(lifetimeCount, null, periodCount);
            foreach (var denom in denomArray)
            {
                helper.AddTicketLine(
                    (decimal)denom * ExpectedLifetimeCount,
                    null,
                    (decimal)denom * ExpectedPeriodCount,
                    desc);
            }

            decimal lifetimeTotal = denomArray.Sum(d => d * ExpectedLifetimeCount);
            decimal periodTotal = denomArray.Sum(d => d * ExpectedPeriodCount);

            helper.AddTicketLine(lifetimeTotal, null, periodTotal, desc);

            return Tuple.Create(lifetimeTotal, periodTotal);
        }

        private void VerifyMocks()
        {
            _meterManagerMock.VerifyAll();
            foreach (var meterMock in _meterMocks)
            {
                meterMock.Value.VerifyAll();
            }

            _propertiesManager.VerifyAll();
            _printerMock.VerifyAll();
            _timeMock.VerifyAll();
        }

        //[TestMethod]
        //public void FromMillicentsTest()
        //{
        //    // Why these tests? Sanity checking.
        //    // The bank returns values in millicents and a FromMillicents routine
        //    // exists in another area of the project that uses a strange multi-step
        //    // algorithm, possibly anticipating rounding or maybe overflow errors
        //    // converting millicents to decimal. At some point, it seems somebody thought
        //    // the extra steps were important.
        //    // Just wanted here to demonstrate that rounding/overflow are not issues
        //    // with just a plain division of value / (decimal)Millcents
        //    // and that the extra steps removed our implementation don't affect
        //    // the outcome of the conversion.
        //    var privateObject = new PrivateObject(_target);
        //    var fromMillicentsMethod =
        //        new Func<long, decimal>(balance => (decimal)privateObject.Invoke("FromMillicents", balance));

        //    // Test the largest whole dollar input
        //    Assert.AreEqual(92233720368547, fromMillicentsMethod(long.MaxValue - 75807));

        //    // And the largest, most precise millicent value we can have
        //    Assert.AreEqual(92233720368547.75807m, fromMillicentsMethod(long.MaxValue));

        //    // And a decent range of practically available values....
        //    foreach (var dollar in from dollars in new[]
        //        {
        //            0,
        //            9,
        //            99,
        //            999,
        //            9999,
        //            99999,
        //            999999,
        //            9999999
        //        }
        //        select dollars)
        //    {
        //        for (long cents = 0; cents < 99999; cents++)
        //        {
        //            // Converting the combined values to formatted strings
        //            // and then to decimal/long format keeps math out of the
        //            // unit test, since we're testing the math in the method.
        //            var decimalVal = decimal.Parse($"{dollar}.{cents:D5}");
        //            var longVal = long.Parse($"{dollar:D7}{cents:D5}");
        //            Assert.AreEqual(decimalVal, fromMillicentsMethod(longVal));
        //        }
        //    }
        //}
    }
}
