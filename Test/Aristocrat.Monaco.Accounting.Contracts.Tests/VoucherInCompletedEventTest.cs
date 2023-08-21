namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for VoucherInCompletedEventTest
    /// </summary>
    [TestClass]
    public class VoucherInCompletedEventTest
    {
        [TestMethod]
        public void PublicConstructorTest()
        {
            var transaction = new VoucherInTransaction();

            var target = new VoucherRedeemedEvent(transaction);

            Assert.IsNotNull(target);
            Assert.AreEqual(transaction, target.Transaction);
        }
    }
}