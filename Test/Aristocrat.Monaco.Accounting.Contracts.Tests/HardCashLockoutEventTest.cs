namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for HardCashLockoutEventTest
    /// </summary>
    [TestClass]
    public class HardCashLockoutEventTest
    {
        [TestMethod]
        public void Constructor1Test()
        {
            var target = new HardCashLockoutEvent();

            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            long cashableAmount = 100;
            long nonCashableAmount = 123;
            long promotionalAmount = 456;
            var target = new HardCashLockoutEvent(cashableAmount, nonCashableAmount, promotionalAmount);

            Assert.IsNotNull(target);
            Assert.AreEqual(cashableAmount, target.CashableAmount);
            Assert.AreEqual(nonCashableAmount, target.NonCashableAmount);
            Assert.AreEqual(promotionalAmount, target.PromotionalAmount);
        }
    }
}