namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System;
    using Handpay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Summary description for HandpayStartedEvent
    /// </summary>
    [TestClass]
    public class HandpayStartedEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            long cashable = 123;
            long promo = 456;
            long nonCash = 789;

            var target = new HandpayStartedEvent(HandpayType.CancelCredit, cashable, promo, nonCash, false);

            Assert.IsNotNull(target);
            Assert.AreEqual(cashable, target.CashableAmount);
            Assert.AreEqual(promo, target.PromoAmount);
            Assert.AreEqual(nonCash, target.NonCashAmount);
        }
    }
}