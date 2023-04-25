namespace Aristocrat.Monaco.Bingo.Handpay.Strategies.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Central;
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPaymentResultsNullTransactionTest()
        {
            _target.GetPaymentResults(1000, null).ToList();
        }

        [DataRow(1000, 1001, 0, 1000, 500)]
        [DataRow(1000, 1000, 1000, 0, 500)]
        [DataRow(1000, 999, 1000, 0, 500)]
        [DataRow(1000, 1001, 0, 1000, 0)]
        [DataRow(1000, 1000, 1000, 0, 0)]
        [DataRow(1000, 999, 1000, 0, 0)]
        [DataRow(1000, 1201, 0, 1200, 600)]
        [DataRow(1000, 1200, 1200, 0, 600)]
        [DataRow(1000, 1199, 1200, 0, 600)]
        [DataTestMethod]
        public void GetPaymentResultsTest(long winAmount, long jackpotLimit, long expectedHandpayAmount, long expectedCreditAmount, long outcomeValue)
        {
            var transaction = new CentralTransaction
            {
                Outcomes = new List<Outcome>
                    {
                        new(1, 2, 3, OutcomeReference.Direct, OutcomeType.Standard, outcomeValue, 2, string.Empty),
                        new(2, 2, 3, OutcomeReference.Direct, OutcomeType.Standard, outcomeValue, 2, string.Empty)
                    }
            };

            _properties.Setup(x => x.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit))
                .Returns(jackpotLimit);
            var results = _target.GetPaymentResults(winAmount, transaction).ToList();
            Assert.AreEqual(1, results.Count);
            var paymentResult = results.First();
            Assert.AreEqual(expectedCreditAmount, paymentResult.MillicentsToPayToCreditMeter);
            Assert.AreEqual(expectedHandpayAmount, paymentResult.MillicentsToPayUsingLargeWinStrategy);
        }
    }
}