namespace Aristocrat.Monaco.Gaming.Tests
{
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrimaryGameStartedEventTest
    {
        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            const int gameId = 7;
            const long denom = 5000;
            const string wagerCategory = "1";

            var log = new Mock<IGameHistoryLog>();
            log.Setup(m => m.ShallowCopy()).Returns(log.Object);

            var target = new PrimaryGameStartedEvent(gameId, denom, wagerCategory, log.Object);

            Assert.IsNotNull(target);
            Assert.AreEqual(gameId, target.GameId);
            Assert.AreEqual(denom, target.Denomination);
            Assert.AreEqual(target.Log, log.Object);
        }
    }
}