namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Summary description for TransferOutCompletedEventTest
    /// </summary>
    [TestClass]
    public class TransferOutCompletedEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            long cashableAmount = 1234;
            long nonCashableAmount = 4567;
            long promotionalAmount = 8901;
            var traceId = Guid.NewGuid();

            var target = new TransferOutCompletedEvent(
                cashableAmount,
                promotionalAmount,
                nonCashableAmount,
                true,
                traceId);

            Assert.IsNotNull(target);
            Assert.AreEqual(cashableAmount, target.CashableAmount);
            Assert.AreEqual(nonCashableAmount, target.NonCashableAmount);
            Assert.AreEqual(promotionalAmount, target.PromotionalAmount);
            Assert.IsTrue(target.Pending);
            Assert.AreEqual(traceId, target.TraceId);
        }
    }
}