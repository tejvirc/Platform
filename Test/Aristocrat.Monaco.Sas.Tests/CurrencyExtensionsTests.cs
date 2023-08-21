namespace Aristocrat.Monaco.Sas.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CurrencyExtensionsTests
    {
        [DataRow(10000000L, 1, 10000000L)]
        [DataRow(10000000L, 10, 1000000L)]
        [DataRow(10000000L, 25, 400000L)]
        [DataRow(10000000L, 100, 100000L)]
        [DataTestMethod]
        public void CentsToAccountingCreditsTest(long cents, int accountingDenom, long expectedResults)
        {
            Assert.AreEqual(expectedResults, cents.CentsToAccountingCredits(accountingDenom));
        }
    }
}