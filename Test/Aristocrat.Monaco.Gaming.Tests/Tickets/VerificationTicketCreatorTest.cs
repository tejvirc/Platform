namespace Aristocrat.Monaco.Gaming.Tests.Tickets
{
    using Application.Contracts;
    using Aristocrat.Monaco.Application.Tickets;
    using Aristocrat.Monaco.Hardware.Contracts.Printer;
    using Gaming.Tickets;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Test.Common;

    /// <summary>
    ///     Contains the tests for the VerificationTicketCreator class
    /// </summary>
    [TestClass]
    public class VerificationTicketCreatorTest
    {
        private const long CreditsOnMachine = 150L;

        private static readonly Dictionary<string, bool> VerificationTicketPageMeterNames = new Dictionary<string, bool>
        {
            { "CoinCount1s", false },
            { "CoinCount2s", false },
            { "BillCount1s", true },
            { "BillCount5s", true },
            { "BillCount10s", true },
            { "BillCount20s", true },
            { "BillCount50s", true },
            { "BillCount100s", true },
            { "VoucherOutCashableCount", true },
            { "VoucherOutCashablePromotionalCount", true },
            { "VoucherOutNonCashablePromotionalCount", true },
            { "MainDoorOpen", true },
            { "MainDoorOpenPowerOff", true },
            { "CashDoorOpen", true },
            { "CashDoorOpenPowerOff", true },
            { "LogicDoorOpen", true },
            { "LogicDoorOpenPowerOff", true },
            { "TopBoxDoorOpen", true },
            { "TopBoxDoorOpenPowerOff", true },
            { "UniversalInterfaceBoxDoorOpen", true },
            { "UniversalInterfaceBoxDoorOpenPowerOff", true },
            { "AdministratorAccess", true },
            { "TechnicianAccess", true },
            { "not implemented", false },
            { "TotalIn", true },
            { "TotalOut", true },
            { "WageredAmount", true },
            { "EgmPaidGameWonAmt", true },
        };

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

        private DateTime _expectedLastMasterClear;
        private DateTime _expectedLastPeriodClear;
        private DateTime _expectedNow;
        private Mock<IMeterManager> _meterManagerMock;
        private Dictionary<string, Mock<IMeter>> _meterMocks;

        private CultureInfo _originalCulture;
        private CultureInfo _originalUiCulture;
        private Mock<IPrinter> _printerMock;
        private Mock<IPropertiesManager> _propertiesManager;
        private VerificationTicketCreator _target;
        private Mock<ITime> _timeMock;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            _originalCulture = Thread.CurrentThread.CurrentCulture;
            _originalUiCulture = Thread.CurrentThread.CurrentUICulture;
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            // Don't care about millisecond precision, makes time verifications easier
            var now = DateTime.UtcNow;
            _expectedNow = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            _timeMock = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _timeMock.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(_expectedNow);
            _expectedLastMasterClear = _expectedNow.Subtract(new TimeSpan(365, 0, 0, 0));
            _expectedLastPeriodClear = _expectedNow.Subtract(new TimeSpan(7, 0, 0, 0));

            _printerMock = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);

            _meterManagerMock = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManagerMock.Setup(mock => mock.LastMasterClear).Returns(_expectedLastMasterClear);
            _meterManagerMock.Setup(mock => mock.LastPeriodClear).Returns(_expectedLastPeriodClear);

            _target = new VerificationTicketCreator();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            Thread.CurrentThread.CurrentCulture = _originalCulture;
            Thread.CurrentThread.CurrentUICulture = _originalUiCulture;
        }

        [TestMethod]
        public void ConstructsTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ImplementsIServiceTest()
        {
            Assert.IsNotNull(_target.ServiceTypes.Select(st => st == typeof(VerificationTicketCreator)).First());
            Assert.AreEqual("Verification Ticket Creator", _target.Name);

            // No observable side effects at this time; make sure it doesn't throw.
            _target.Initialize();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void VerificationTicketInvalidPageTest()
        {
            _target.Create(3);
        }

        [TestMethod]
        public void CreateVerificationTicketTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.LocalizationOperatorTicketSelectable, false))
                .Returns(false)
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(PropertyKey.CurrentBalance, It.IsAny<long>()))
                .Returns(CreditsOnMachine)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine1, It.IsAny<string>()))
                .Returns("Test Casino Name")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine2, It.IsAny<string>()))
                .Returns("Test Casino city")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(PropertyKey.TicketTextLine3, It.IsAny<string>()))
                .Returns("123 Test Casino Street")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.Zone, It.IsAny<string>()))
                .Returns("6")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.JurisdictionKey, It.IsAny<int>()))
                .Returns("Jurisdiction")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns("12345")
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns((uint)12345)
                .Verifiable();

            _propertiesManager.Setup(mock => mock.GetProperty(KernelConstants.SystemVersion, It.IsAny<string>()))
                .Returns("1.2.3.4")
                .Verifiable();
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _meterMocks = new Dictionary<string, Mock<IMeter>>();
            foreach (var vtn in VerificationTicketPageMeterNames)
            {
                _meterMocks[vtn.Key] = new Mock<IMeter>(MockBehavior.Strict);
                _meterMocks[vtn.Key].Setup(mock => mock.Lifetime).Returns(0).Verifiable();
                _meterMocks[vtn.Key].Setup(mock => mock.Period).Returns(0).Verifiable();
                _meterMocks[vtn.Key].Setup(mock => mock.Classification).Returns(new OccurrenceMeterClassification());
            }

            foreach (var vtn in VerificationTicketPageMeterNames)
            {
                _meterManagerMock.Setup(mock => mock.IsMeterProvided(vtn.Key)).Returns(vtn.Value);
                _meterManagerMock.Setup(mock => mock.GetMeter(vtn.Key)).Returns(_meterMocks[vtn.Key].Object);
            }

            _printerMock.Setup(mock => mock.PaperState).Returns(PaperStates.Full);
            var _verificationMock = new Mock<VerificationTicket>(0);
            //var _ticketMock = new Mock<ITicketCreator<VerificationTicket>>();
            //_ticketMock.Setup(mock => mock.Get()).Returns(new VerificationTicket(1));
            //var ticket = _target.Create(0);

            //Assert.IsNotNull(ticket);
            //Assert.IsFalse(string.IsNullOrEmpty(ticket["left"]));
            //Assert.IsFalse(string.IsNullOrEmpty(ticket["right"]));
            //Assert.IsFalse(string.IsNullOrEmpty(ticket["center"]));
            //_verificationMock = new Mock<VerificationTicket>(1);
            //ticket = _target.Create(1);
            //Assert.IsFalse(string.IsNullOrEmpty(ticket["left"]));
            //Assert.IsFalse(string.IsNullOrEmpty(ticket["right"]));
            //Assert.IsFalse(string.IsNullOrEmpty(ticket["center"]));
        }
    }
}