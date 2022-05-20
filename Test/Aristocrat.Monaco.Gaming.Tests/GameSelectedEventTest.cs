namespace Aristocrat.Monaco.Gaming.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for GameSelectedEventTest and is intended
    ///     to contain all GameSelectedEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class GameSelectedEventTest
    {
        /// <summary>
        ///     Test title id for unit tests.
        /// </summary>
        private const int TitleId = 123;

        /// <summary>
        ///     Test theme id for unit tests.
        /// </summary>
        private const int ThemeId = 456;

        /// <summary>
        ///     Test paytable id for unit tests.
        /// </summary>
        private const int PayTableId = 789;

        /// <summary>
        ///     A test for the constructor
        /// </summary>
        [TestMethod]
        public void GameSelectedEventConstructorTest()
        {
            //var target = new GameSelectedEvent(TitleId, ThemeId, PayTableId);
            //Assert.IsTrue(target.TitleId.Equals(TitleId));
            //Assert.IsTrue(target.ThemeId.Equals(ThemeId));
            //Assert.IsTrue(target.PayTableId.Equals(PayTableId));
        }

        /// <summary>
        ///     A test for serialization
        /// </summary>
        [TestMethod]
        public void GameSelectedEventSerializeTest()
        {
            //var originalEvent = new GameSelectedEvent(TitleId, ThemeId, PayTableId);
            //var expectedGuid = originalEvent.GloballyUniqueId;

            //var stream = new FileStream("GameSelectedEvent.dat", FileMode.Create);
            //var formatter = new SoapFormatter(
            //    null,
            //    new StreamingContext(StreamingContextStates.File));

            //formatter.Serialize(stream, originalEvent);

            //stream.Position = 0;

            //var target = (GameSelectedEvent) formatter.Deserialize(stream);

            //Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
            //Assert.IsTrue(target.TitleId.Equals(TitleId));
            //Assert.IsTrue(target.ThemeId.Equals(ThemeId));
            //Assert.IsTrue(target.PayTableId.Equals(PayTableId));
        }
    }
}