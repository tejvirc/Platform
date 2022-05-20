namespace Aristocrat.Monaco.Gaming.Tests.Payment
{
    using System;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Monaco.Gaming.Payment;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BonusPaymentDeterminationHandlerTest
    {
        [DataRow(625000L)]
        [DataRow(500000L)]
        [DataRow(125000L)]
        [DataRow(50000L)]
        [TestMethod]
        public void GetBonusPayMethod_Handpay_Cashable_Test(long bonusAmountInMillicents)
        {
            var _properties = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _properties.Setup(x => x.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)).Returns(500000L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue)).Returns(500000L);

            var bank = MockBank(400000L);

            var bonusTransaction = CreateCommittedBonusTransaction(PayMethod.Handpay, It.IsAny<long>());

            var bonusPaymentDeterminationHandler = new BonusPaymentDeterminationHandler(_properties.Object, bank.Object);

            var payMethod = bonusPaymentDeterminationHandler.GetBonusPayMethod(bonusTransaction, bonusAmountInMillicents);

            Assert.AreEqual(PayMethod.Handpay, payMethod);
        }

        private Mock<IBank> MockBank(long balance)
        {
            var bank = new Mock<IBank>();
            bank.Setup(x => x.QueryBalance()).Returns(balance);
            return bank;
        }

        private BonusTransaction CreateCommittedBonusTransaction(PayMethod method = PayMethod.Any, long cashAmount = 0, long nonCashAmount = 0, long promoAmount = 0)
        {
            var bonus = new BonusTransaction(1,
                DateTime.UtcNow,
                "BonusId",
                cashAmount,
                nonCashAmount,
                promoAmount,
                1,
                1,
                method)
            {
                PaidCashableAmount = cashAmount,
                PaidNonCashAmount = nonCashAmount,
                PaidPromoAmount = promoAmount
            };

            return bonus;
        }
    }
}
