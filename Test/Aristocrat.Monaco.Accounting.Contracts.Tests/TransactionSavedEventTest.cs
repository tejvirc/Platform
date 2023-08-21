namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Summary description for TransactionSavedEventTest
    /// </summary>
    [TestClass]
    public class TransactionSavedEventTest
    {
        [TestMethod]
        public void Constructor2Test()
        {
            ITransaction transaction = new BillTransaction();
            var target = new TransactionSavedEvent(transaction);

            Assert.IsNotNull(target);
            Assert.AreEqual(transaction, target.Transaction);
        }
    }
}