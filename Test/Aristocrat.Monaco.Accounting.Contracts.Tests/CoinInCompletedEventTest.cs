namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    using Accounting.Contracts.CoinAcceptor;
    using Hardware.Contracts.CoinAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CoinInCompletedEventTest
    {
        [TestMethod]
        public void PublicConstructorTest()
        {
            long amount = 100000;
            var target = new CoinInCompletedEvent(new Coin { Value = amount }, null);

            Assert.IsNotNull(target);
            Assert.AreEqual(amount, target.Coin.Value);
        }
    }
}
