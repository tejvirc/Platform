namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using System;
    using System.Globalization;
    using Aristocrat.G2S.Client.Devices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CabinetExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGetLocaleIdCabinetDeviceIsNullExpectException()
        {
            var cabinetDevice = (ICabinetDevice)null;
            cabinetDevice.LocaleId(CultureInfo.CurrentCulture);
        }

        [TestMethod]
        public void WhenGetAllCultureInfoExpectSuccess()
        {
            var cabinetDeviceMock = new Mock<ICabinetDevice>();
            var cabinetDevice = cabinetDeviceMock.Object;

            var cultureInfo = CultureInfo.CurrentCulture;
            var regionInfo = new RegionInfo(cultureInfo.LCID);

            var localeId = cabinetDevice.LocaleId(cultureInfo);
            Assert.AreEqual($"{cultureInfo.TwoLetterISOLanguageName}_{regionInfo.TwoLetterISORegionName}", localeId);
        }
    }
}