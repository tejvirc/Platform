namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    #endregion

    /// <summary>
    ///     Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class BankBalanceChangedEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            long oldBalance = 1000;
            long newBalance = 1234;
            var target = new BankBalanceChangedEvent(oldBalance, newBalance, Guid.Empty);

            Assert.IsNotNull(target);
            Assert.AreEqual(oldBalance, target.OldBalance);
            Assert.AreEqual(newBalance, target.NewBalance);
        }
    }
}