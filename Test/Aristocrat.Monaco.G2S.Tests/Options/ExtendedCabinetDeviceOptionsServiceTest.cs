namespace Aristocrat.Monaco.G2S.Tests.Options
{
    using System.Globalization;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using G2S.Options;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Constants = G2S.Constants;

    [TestClass]
    public class ExtendedCabinetDeviceOptionsServiceTest
    {
        private const int ConfigurationId = 123;

        private const int MachineNumber = 12345;

        private const string MachineId = "machine-id";

        private const string CurrencyId = "usd";

        private const long ReportDenomId = 151515;

        private const string LocaleId = "de-DE";

        private const string AreaId = "area_id";

        private const string ZoneId = "zone_id";

        private const string BankId = "bank_Id";

        private const string EgmPosition = "egm_position";

        private const string MachineLocation = "machine_loc";

        private const string CabinetStyle = "G2S_cabinet_style";

        private const int IdleTimePeriod = 55;

        private const long LargeWinLimit = 123123;

        private const long MaxCreditMeter = 456456;

        private const string PropertyId = "099";

        private readonly Mock<IDeviceObserver> _deviceObserverMock = new Mock<IDeviceObserver>();

        private readonly Mock<IEgmStateObserver> _egmStateObserverMock = new Mock<IEgmStateObserver>();
        private Mock<ILocalization> _localeProvider;

        private Mock<IPropertiesManager> _propertiesManagerMock;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _propertiesManagerMock = new Mock<IPropertiesManager>();
            MoqServiceManager.AddService(_propertiesManagerMock);
            _localeProvider = new Mock<ILocalization>();
            MoqServiceManager.AddService(_localeProvider);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void WhenMatchesExpectSuccess()
        {
            var service = new CabinetDeviceOptions();

            Assert.IsTrue(service.Matches(DeviceClass.Cabinet));
            Assert.IsFalse(service.Matches(DeviceClass.Gat));
        }

        [TestMethod]
        public void WhenApplyExtendedOptionValuesWithCorrectDataExpectSuccess()
        {
            var service = new CabinetDeviceOptions();

            service.ApplyProperties(CreateDevice(), CreateDeviceOptionConfigWithCorrectValues());

            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.MachineId, (uint)MachineNumber), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.SerialNumber, MachineId), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.CurrencyId, CurrencyId), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(Constants.ReportDenomId, ReportDenomId), Times.Once);

            _localeProvider.VerifySet(l => l.CurrentCulture = CultureInfo.GetCultureInfo(LocaleId), Times.Once());

            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Area, AreaId), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Zone, ZoneId), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Bank, BankId), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Position, EgmPosition), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Location, MachineLocation), Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(Constants.CabinetStyle, CabinetStyle), Times.Once);
            _propertiesManagerMock.Verify(
                x => x.SetProperty(GamingConstants.IdleTimePeriod, IdleTimePeriod),
                Times.Once);
            _propertiesManagerMock.Verify(
                x => x.SetProperty(AccountingConstants.LargeWinLimit, LargeWinLimit),
                Times.Once);
            _propertiesManagerMock.Verify(
                x => x.SetProperty(AccountingConstants.MaxCreditMeter, MaxCreditMeter),
                Times.Once);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.PropertyId, PropertyId), Times.Once);
        }

        [TestMethod]
        public void WhenApplyExtendedOptionValuesWithNotExistsDataExpectSuccess()
        {
            var service = new CabinetDeviceOptions();

            service.ApplyProperties(CreateDevice(), new DeviceOptionConfigValues(ConfigurationId));

            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.MachineId, MachineNumber), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.SerialNumber, MachineId), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.CurrencyId, CurrencyId), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(Constants.ReportDenomId, ReportDenomId), Times.Never);

            Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, CultureInfo.CurrentUICulture);

            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Area, AreaId), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Zone, ZoneId), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Bank, BankId), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Position, EgmPosition), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.Location, MachineLocation), Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(Constants.CabinetStyle, CabinetStyle), Times.Never);
            _propertiesManagerMock.Verify(
                x => x.SetProperty(GamingConstants.IdleTimePeriod, IdleTimePeriod),
                Times.Never);
            _propertiesManagerMock.Verify(
                x => x.SetProperty(AccountingConstants.LargeWinLimit, LargeWinLimit),
                Times.Never);
            _propertiesManagerMock.Verify(
                x => x.SetProperty(AccountingConstants.MaxCreditMeter, MaxCreditMeter),
                Times.Never);
            _propertiesManagerMock.Verify(x => x.SetProperty(ApplicationConstants.PropertyId, PropertyId), Times.Never);
        }

        [TestMethod]
        public void WhenLocaleIdIsEmptyExpectSuccess()
        {
            var service = new CabinetDeviceOptions();

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_localeId", string.Empty);

            service.ApplyProperties(CreateDevice(), new DeviceOptionConfigValues(ConfigurationId));
            Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, CultureInfo.CurrentUICulture);
        }

        [TestMethod]
        public void WhenLocaleIdIsIncorrectExpectSuccess()
        {
            var service = new CabinetDeviceOptions();

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_localeId", "aaa");

            service.ApplyProperties(CreateDevice(), new DeviceOptionConfigValues(ConfigurationId));
            Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, CultureInfo.CurrentUICulture);
        }

        [TestMethod]
        public void WhenCabinetStyleIsIncorrectExpectSuccess()
        {
            var service = new CabinetDeviceOptions();

            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_cabinetStyle", "cabinetStyle");

            service.ApplyProperties(CreateDevice(), new DeviceOptionConfigValues(ConfigurationId));
            _propertiesManagerMock.Verify(
                x => x.SetProperty(Constants.CabinetStyle, It.IsAny<string>(), true),
                Times.Never);
        }

        private CabinetDevice CreateDevice()
        {
            return new CabinetDevice(_deviceObserverMock.Object, _egmStateObserverMock.Object);
        }

        private DeviceOptionConfigValues CreateDeviceOptionConfigWithCorrectValues()
        {
            var deviceOptionConfigValues = new DeviceOptionConfigValues(ConfigurationId);
            deviceOptionConfigValues.AddOption("G2S_machineNum", MachineNumber.ToString());
            deviceOptionConfigValues.AddOption("G2S_machineId", MachineId);
            deviceOptionConfigValues.AddOption("G2S_currencyId", CurrencyId);
            deviceOptionConfigValues.AddOption("G2S_reportDenomId", ReportDenomId.ToString());
            deviceOptionConfigValues.AddOption("G2S_localeId", LocaleId);
            deviceOptionConfigValues.AddOption("G2S_areaId", AreaId);
            deviceOptionConfigValues.AddOption("G2S_zoneId", ZoneId);
            deviceOptionConfigValues.AddOption("G2S_bankId", BankId);
            deviceOptionConfigValues.AddOption("G2S_egmPosition", EgmPosition);
            deviceOptionConfigValues.AddOption("G2S_machineLoc", MachineLocation);
            deviceOptionConfigValues.AddOption("G2S_cabinetStyle", CabinetStyle);
            deviceOptionConfigValues.AddOption("G2S_idleTimePeriod", IdleTimePeriod.ToString());
            deviceOptionConfigValues.AddOption("G2S_largeWinLimit", LargeWinLimit.ToString());
            deviceOptionConfigValues.AddOption("G2S_maxCreditMeter", MaxCreditMeter.ToString());
            deviceOptionConfigValues.AddOption("G2S_propertyId", PropertyId);

            return deviceOptionConfigValues;
        }
    }
}