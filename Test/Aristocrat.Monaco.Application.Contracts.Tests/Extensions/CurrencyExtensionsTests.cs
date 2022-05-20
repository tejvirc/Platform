namespace Aristocrat.Monaco.Application.Contracts.Extensions.Tests
{
    using System;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CurrencyExtensionsTests
    {
        [DataRow("en-US", 2, 1000, 0.01)]
        [DataRow("en-US", 0, 1000, 1.0)]
        [DataTestMethod]
        public void MillicentsToDollarsTest(string cultureName, int numberOfDecimalDigits, long millicents, double expectedResult)
        {
            var culture = new CultureInfo(cultureName)
            {
                NumberFormat = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = numberOfDecimalDigits
                }
            };

            CurrencyExtensions.SetCultureInfo(culture);
            Assert.AreEqual(Convert.ToDecimal(expectedResult), millicents.MillicentsToDollars());
        }

        [DataRow("en-US", 2, 1, 0.01)]
        [DataRow("en-US", 0, 1, 1.0)]
        [DataTestMethod]
        public void CentsToDollarsTest(string cultureName, int numberOfDecimalDigits, long millicents, double expectedResult)
        {
            var culture = new CultureInfo(cultureName)
            {
                NumberFormat = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = numberOfDecimalDigits
                }
            };

            CurrencyExtensions.SetCultureInfo(culture);
            Assert.AreEqual(Convert.ToDecimal(expectedResult), millicents.CentsToDollars());
        }

        [DataRow("en-US", 2, 1000, 1)]
        [DataRow("en-US", 0, 1000, 1)]
        [DataTestMethod]
        public void MillicentsToCents(string cultureName, int numberOfDecimalDigits, long millicents, long expectedResult)
        {
            var culture = new CultureInfo(cultureName)
            {
                NumberFormat = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = numberOfDecimalDigits
                }
            };

            CurrencyExtensions.SetCultureInfo(culture);
            Assert.AreEqual(expectedResult, millicents.MillicentsToCents());
        }
    }
}
