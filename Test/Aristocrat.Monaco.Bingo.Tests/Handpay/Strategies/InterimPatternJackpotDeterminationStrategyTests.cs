namespace Aristocrat.Monaco.Bingo.Handpay.Strategies.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class InterimPatternJackpotDeterminationStrategyTests
    {
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private InterimPatternJackpotDeterminationStrategy _target;

        [TestInitialize]
        public void MyTestInitalize()
        {
            _target = new InterimPatternJackpotDeterminationStrategy(_properties.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest()
        {
            _target = new InterimPatternJackpotDeterminationStrategy(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransactionTest()
        {
            _target.GetPaymentResults(0, null);
        }

        [DataRow(500, 1000, 1000, 1000, 500)]
        [DataRow(500, 500, 1000, 0, 1000)]
        [DataRow(1000, 1000, 1000, 2000, 0)]
        [DataTestMethod]
        public void GetPaymentResultsTest(long firstPatternWin, long secondPatternWin, long jackpotLimit, long expectedHandpayAmount, long expectedCreditAmount)
        {
            _properties.Setup(x => x.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit))
                .Returns(jackpotLimit);

            var transaction = new CentralTransaction
            {
                Outcomes = new List<Outcome>
                {
                    new Outcome(1, 1, 1, OutcomeReference.Direct, OutcomeType.Standard, firstPatternWin, 1, string.Empty),
                    new Outcome(2, 1, 1, OutcomeReference.Direct, OutcomeType.Standard, secondPatternWin, 2, string.Empty)
                }
            };

            var results = _target.GetPaymentResults(firstPatternWin + secondPatternWin, transaction).ToList();
            Assert.AreEqual(1, results.Count);
            var paymentResult = results.First();
            Assert.AreEqual(expectedCreditAmount, paymentResult.MillicentsToPayToCreditMeter);
            Assert.AreEqual(expectedHandpayAmount, paymentResult.MillicentsToPayUsingLargeWinStrategy);
        }
    }
}