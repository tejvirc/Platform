namespace Aristocrat.Monaco.G2S.Tests.Handlers.Cabinet
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.Cabinet;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common;
    using Moq;

    [TestClass]
    public class CabinetProfileCommandBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesManagerExpectException()
        {
            var builder = new CabinetProfileCommandBuilder(null, null);

            Assert.IsNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLocaleProviderExpectException()
        {
            var properties = new Mock<IPropertiesManager>();

            var builder = new CabinetProfileCommandBuilder(properties.Object, null);

            Assert.IsNull(builder);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var properties = new Mock<IPropertiesManager>();
            var localeProvider = new Mock<ILocalization>();

            var builder = new CabinetProfileCommandBuilder(properties.Object, localeProvider.Object);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullDeviceExpectException()
        {
            var properties = new Mock<IPropertiesManager>();
            var localeProvider = new Mock<ILocalization>();

            var builder = new CabinetProfileCommandBuilder(properties.Object, localeProvider.Object);

            await builder.Build(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullCabinetProfileExpectException()
        {
            var properties = new Mock<IPropertiesManager>();
            var localeProvider = new Mock<ILocalization>();

            var builder = new CabinetProfileCommandBuilder(properties.Object, localeProvider.Object);

            var device = new Mock<ICabinetDevice>();

            await builder.Build(device.Object, null);
        }

        [TestMethod]
        public async Task WhenBuildExpectSuccess()
        {
            var properties = new Mock<IPropertiesManager>();
            var localeProvider = new Mock<ILocalization>();

            const string serialNumber = "AT_TEST";
            const int machineId = 7;
            const long reportDenomId = 10000L;
            const string currency = "USD";
            const string area = "West";
            const string zone = "A";
            const string bank = "01";
            const string position = "02";
            const string location = "A0102";
            const long largeWinAmount = 9999L;
            const long maxCreditMeter = 123456789L;
            const int idleTimePeriod = 747;

            properties.Setup(p => p.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(serialNumber);
            properties.Setup(p => p.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>()))
                .Returns((uint)machineId);

            properties.Setup(p => p.GetProperty(ApplicationConstants.CurrencyId, It.IsAny<object>())).Returns(currency);
            properties.Setup(p => p.GetProperty(G2S.Constants.ReportDenomId, It.IsAny<object>())).Returns(reportDenomId);

            properties.Setup(p => p.GetProperty(ApplicationConstants.Area, It.IsAny<object>())).Returns(area);
            properties.Setup(p => p.GetProperty(ApplicationConstants.Zone, It.IsAny<object>())).Returns(zone);
            properties.Setup(p => p.GetProperty(ApplicationConstants.Bank, It.IsAny<object>())).Returns(bank);
            properties.Setup(p => p.GetProperty(ApplicationConstants.Position, It.IsAny<object>())).Returns(position);
            properties.Setup(p => p.GetProperty(ApplicationConstants.Location, It.IsAny<object>())).Returns(location);
            properties.Setup(p => p.GetProperty(G2S.Constants.CabinetStyle, It.IsAny<object>()))
                .Returns(G2S.Constants.DefaultCabinetStyle);

            properties.Setup(p => p.GetProperty(AccountingConstants.LargeWinLimit, It.IsAny<object>()))
                .Returns(largeWinAmount);
            properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<object>()))
                .Returns(maxCreditMeter);

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns(idleTimePeriod);

            properties.Setup(p => p.GetProperty(ApplicationConstants.TimeZoneOffsetKey, It.IsAny<object>()))
                .Returns(TimeSpan.Zero);

            localeProvider.SetupGet(m => m.CurrentCulture).Returns(CultureInfo.CurrentCulture);

            var builder = new CabinetProfileCommandBuilder(properties.Object, localeProvider.Object);

            var device = new Mock<ICabinetDevice>();
            var profile = new cabinetProfile();

            device.SetupAllProperties();

            await builder.Build(device.Object, profile);

            Assert.AreEqual(profile.configurationId, device.Object.ConfigurationId);
            Assert.AreEqual(profile.useDefaultConfig, device.Object.UseDefaultConfig);
            Assert.AreEqual(profile.requiredForPlay, device.Object.RequiredForPlay);
            Assert.AreEqual(profile.machineNum, machineId);
            Assert.AreEqual(profile.machineId, serialNumber);
            Assert.AreEqual(profile.currencyId, currency);
            Assert.AreEqual(profile.reportDenomId, reportDenomId);
            Assert.IsNotNull(profile.localeId);
            Assert.AreEqual(profile.areaId, area);
            Assert.AreEqual(profile.zoneId, zone);
            Assert.AreEqual(profile.bankId, bank);
            Assert.AreEqual(profile.egmPosition, position);
            Assert.AreEqual(profile.machineLoc, location);
            Assert.AreEqual(profile.cabinetStyle, G2S.Constants.DefaultCabinetStyle);
            Assert.AreEqual(profile.largeWinLimit, largeWinAmount);
            Assert.AreEqual(profile.maxCreditMeter, maxCreditMeter);
            Assert.AreEqual(profile.maxHopperPayOut, 0);
            Assert.AreEqual(profile.splitPayOut, false);
            Assert.AreEqual(profile.idleTimePeriod, idleTimePeriod);
            Assert.IsNotNull(profile.timeZoneOffset, DateTime.UtcNow.GetFormattedOffset());
            Assert.AreEqual(profile.acceptNonCashAmts, t_acceptNonCashAmts.G2S_acceptAlways);
            Assert.AreEqual(profile.configDateTime, device.Object.ConfigDateTime);
            Assert.AreEqual(profile.configComplete, device.Object.ConfigComplete);
            Assert.AreEqual(profile.g2sResetSupported, false);
            Assert.AreEqual(profile.timeZoneSupported, t_g2sBoolean.G2S_true);
            Assert.AreEqual(profile.masterResetAllowed, false);
        }
    }
}