namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Summary description for WatOnCompletedEventTest
    /// </summary>
    [TestClass]
    public class WatOnCompletedEventTest
    {
        [TestMethod]
        public void Constructor2Test()
        {
            var transaction = new WatOnTransaction();
            var target = new WatOnCompleteEvent(transaction);

            Assert.IsNotNull(target);
            Assert.AreEqual(transaction, target.Transaction);
        }
    }
}