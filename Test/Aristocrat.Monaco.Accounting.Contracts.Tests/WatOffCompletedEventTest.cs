namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Wat;

    /// <summary>
    ///     Summary description for WatOffCompletedEventTest
    /// </summary>
    [TestClass]
    public class WatOffCompletedEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var transaction = new WatTransaction();

            var target = new WatTransferCompletedEvent(transaction);

            Assert.IsNotNull(target);
            Assert.AreEqual(transaction, target.Transaction);
        }
    }
}