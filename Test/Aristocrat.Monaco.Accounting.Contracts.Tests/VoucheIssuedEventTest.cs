namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using Hardware.Contracts.Ticket;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for VoucheIssuedEventTest
    /// </summary>
    [TestClass]
    public class VoucheIssuedEventTest
    {
        [TestMethod]
        public void Constructor1Test()
        {
            var target = new VoucherIssuedEvent(null, null);

            Assert.IsNotNull(target);
            Assert.AreEqual(null, target.Transaction);
            Assert.AreEqual(null, target.PrintedTicket);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            var transaction =
                new VoucherOutTransaction(1, DateTime.Now, 1, AccountType.Cashable, "123", 5000, "12345")
                {
                    TransactionId = 123
                };
            var target = new VoucherIssuedEvent(transaction, null);

            Assert.IsNotNull(target);
            Assert.AreEqual(123, target.Transaction.TransactionId);
        }
    }
}