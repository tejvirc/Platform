namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for TransferOutStartedEventTestcs
    /// </summary>
    [TestClass]
    public class TransferOutStartedEventTestcs
    {
        [TestMethod]
        public void ConstructorTest()
        {
            Guid transactionId = Guid.NewGuid();
            long pendingCashableAmount = 1234;
            long pendingNonCashableAmount = 4567;
            long pendingPromotionalAmount = 8901;

            var target = new TransferOutStartedEvent(
                transactionId, 
                pendingCashableAmount,
                pendingPromotionalAmount,
                pendingNonCashableAmount);

            Assert.IsNotNull(target);
            Assert.AreEqual(transactionId, target.TransactionId);
            Assert.AreEqual(pendingCashableAmount, target.PendingCashableAmount);
            Assert.AreEqual(pendingNonCashableAmount, target.PendingNonCashableAmount);
            Assert.AreEqual(pendingPromotionalAmount, target.PendingPromotionalAmount);
        }
    }
}