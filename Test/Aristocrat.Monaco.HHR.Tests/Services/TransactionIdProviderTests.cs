namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using Aristocrat.Monaco.Hhr.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionIdProviderTests
    {

        [TestMethod]
        public void TestGetTransactionId_WhenSeedIsSet_ExpectIncrementedValue()
        {
            var target = new TransactionIdProvider();
            target.SetLastId(1);
            Assert.AreEqual(2, target.GetNextTransactionId());
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void TestInvalidTransactionId_WhenValueIsMax_ExpectException()
        {
            var target = new TransactionIdProvider();
            target.SetLastId(uint.MaxValue - 1);
            Assert.AreEqual(uint.MaxValue, target.GetNextTransactionId());
            target.GetNextTransactionId();
        }
    }
}
