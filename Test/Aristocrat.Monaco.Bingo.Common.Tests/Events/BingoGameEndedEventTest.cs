namespace Aristocrat.Monaco.Bingo.Common.Events.Tests
{
    using Aristocrat.Monaco.Bingo.Common.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Summary description for BingoGameEndedEvent
    /// </summary>
    [TestClass]
    public class BingoGameEndedEventTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            long gameSerialNumber = 123456;

            var target = new BingoGameEndedEvent(gameSerialNumber);

            Assert.IsNotNull(target);
            Assert.AreEqual(gameSerialNumber, target.GameSerialNumber);
        }
    }
}