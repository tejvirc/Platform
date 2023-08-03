
namespace Aristocrat.Monaco.Common.Tests.Currency
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using log4net;

    using Aristocrat.Monaco.Common.Currency;
    

    [TestClass]
    public class CurrencyLoaderTest
    {
        //CultureInfo.GetCultures gives 154 cultures(CultureInfo NETCore)
        private const int _numberOfCurrencies = 154;

        [TestMethod]
        public void CurrencyLoad_Success()
        {
            var mockLogger = new Mock<ILog>();
            mockLogger.Setup(l => l.Error(It.IsAny<string>()));

            var currencies = CurrencyLoader.GetCurrenciesFromWindows(mockLogger.Object);

            Assert.IsNotNull(currencies);
            Assert.AreEqual(_numberOfCurrencies, currencies.Count);

            // make sure all currencies have associated culture
            Assert.IsTrue(currencies.Values.All(c => c != null));
        }
    }
}
