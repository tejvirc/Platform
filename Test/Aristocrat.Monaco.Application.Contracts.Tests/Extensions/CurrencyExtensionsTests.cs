namespace Aristocrat.Monaco.Application.Contracts.Extensions.Tests
{
    using System;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CurrencyExtensionsTests
    {
        [DataRow("USD", "c", "en-US", 2, 1000, 0.01)]
        [DataRow("USD", "c", "en-US", 0, 1000, 1.0)]
        [DataTestMethod]
        public void MillicentsToDollarsTest(string currencyCode, string minorUnitSymbol, string cultureName, int numberOfDecimalDigits, long millicents, double expectedResult)
        {
            var culture = new CultureInfo(cultureName)
            {
                NumberFormat = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = numberOfDecimalDigits
                }
            };

            var region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency.Currency(currencyCode, region, culture, minorUnitSymbol);

            CurrencyExtensions.SetCultureInfo();
            Assert.AreEqual(Convert.ToDecimal(expectedResult), millicents.MillicentsToDollars());
        }

        [DataRow("USD", "c", "en-US", 2, 1, 0.01)]
        [DataRow("USD", "c", "en-US", 0, 1, 1.0)]
        [DataTestMethod]
        public void CentsToDollarsTest(string currencyCode, string minorUnitSymbol, string cultureName, int numberOfDecimalDigits, long millicents, double expectedResult)
        {
            var culture = new CultureInfo(cultureName)
            {
                NumberFormat = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = numberOfDecimalDigits
                }
            };

            var region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency.Currency(currencyCode, region, culture, minorUnitSymbol);

            CurrencyExtensions.SetCultureInfo();
            Assert.AreEqual(Convert.ToDecimal(expectedResult), millicents.CentsToDollars());
        }

        [DataRow("USD", "c", "en-US", 2, 1000, 1)]
        [DataRow("USD", "c", "en-US", 0, 1000, 1)]
        [DataTestMethod]
        public void MillicentsToCents(string currencyCode, string minorUnitSymbol, string cultureName, int numberOfDecimalDigits, long millicents, long expectedResult)
        {
            var culture = new CultureInfo(cultureName)
            {
                NumberFormat = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = numberOfDecimalDigits
                }
            };

            var region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency.Currency(currencyCode, region, culture, minorUnitSymbol);

            CurrencyExtensions.SetCultureInfo();
            Assert.AreEqual(expectedResult, millicents.MillicentsToCents());
        }
    }
}
