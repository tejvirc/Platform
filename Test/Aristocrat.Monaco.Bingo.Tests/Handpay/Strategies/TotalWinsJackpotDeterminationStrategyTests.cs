namespace Aristocrat.Monaco.Bingo.Handpay.Strategies.Tests
{
    using System;
    using System.Linq;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TotalWinsJackpotDeterminationStrategyTests
    {
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private TotalWinsJackpotDeterminationStrategy _target;

        [TestInitialize]
        public void MyTestInitalize()
        {
            _target = new TotalWinsJackpotDeterminationStrategy(_properties.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest()
        {
            _target = new TotalWinsJackpotDeterminationStrategy(null);
        }

        [DataRow(1000, 1001, 0, 1000)]
        [DataRow(1000, 1000, 1000, 0)]
        [DataRow(1000, 999, 1000, 0)]
        [DataTestMethod]
        public void GetPaymentResultsTest(long winAmount, long jackpotLimit, long expectedHandpayAmount, long expectedCreditAmount)
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit))
                .Returns(jackpotLimit);

            var results = _target.GetPaymentResults(winAmount, null).ToList();
            Assert.AreEqual(1, results.Count);
            var paymentResult = results.First();
            Assert.AreEqual(expectedCreditAmount, paymentResult.MillicentsToPayToCreditMeter);
            Assert.AreEqual(expectedHandpayAmount, paymentResult.MillicentsToPayUsingLargeWinStrategy);
        }
    }
}