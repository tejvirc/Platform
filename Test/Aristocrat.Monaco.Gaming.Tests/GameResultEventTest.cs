namespace Aristocrat.Monaco.Gaming.Tests
{
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameResultEventTest
    {
        [TestMethod]
        public void TwoParameterConstructorTest()
        {
            const int gameId = 1;
            const long denomination = 5000;
            const string wagerCategory = "1";

            var log = new Mock<IGameHistoryLog>();
            log.Setup(m => m.ShallowCopy()).Returns(log.Object);

            var target = new GameResultEvent(gameId, denomination, wagerCategory, log.Object);
            Assert.IsNotNull(target);
            Assert.AreEqual(gameId, target.GameId);
            Assert.AreEqual(denomination, target.Denomination);
            Assert.AreEqual(target.Log, log.Object);
        }
    }
}