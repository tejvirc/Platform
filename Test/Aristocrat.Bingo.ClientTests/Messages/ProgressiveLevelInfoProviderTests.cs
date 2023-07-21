namespace Aristocrat.Bingo.Client.Messages.Tests
{
    using System;
    using Messages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProgressiveLevelInfoProviderTests
    {
        private ProgressiveLevelInfoProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new();
        }

        [DataRow(0, 0, 1, 1, DisplayName = "Sequence number zero")]
        [DataRow(-1, 0, 1, 1, DisplayName = "Sequence number negative")]
        [DataRow(1, -1, 1, 1, DisplayName = "Progressive level Id negative")]
        [DataRow(0, 0, -1, 1, DisplayName = "gameTitleId negative")]
        [DataRow(0, 0, 1, 0, DisplayName = "denomination zero")]
        [DataRow(0, 0, 1, -1, DisplayName = "denomination negative")]
        [ExpectedException(typeof(ArgumentException))]
        [DataTestMethod]
        public void AddProgressiveLevelInfoInvalidValuesTest(long levelId, int sequenceNumber, int gameTitleId, long denomination)
        {
            _target.AddProgressiveLevelInfo(levelId, sequenceNumber, gameTitleId, denomination);
        }

        [DataRow(-1, -1L,   DisplayName = "Progressive level Id invalid negative")]
        [DataRow(0, 10001L, DisplayName = "Progressive level Id 0")]
        [DataRow(1, 10002L, DisplayName = "Progressive level Id 1")]
        [DataRow(2, 10003L, DisplayName = "Progressive level Id 2")]
        [DataRow(3, -1,     DisplayName = "Progressive level Id invalid not existing")]
        [DataTestMethod]
        public void GetServerProgressiveLevelIdTest(int levelId, long expectedResponse)
        {
            var gameTitleId = 100;
            var denomination = 25L;

            // First add some values
            _target.AddProgressiveLevelInfo(10001L, 1, gameTitleId, denomination);
            _target.AddProgressiveLevelInfo(10002L, 2, gameTitleId, denomination);
            _target.AddProgressiveLevelInfo(10003L, 3, gameTitleId, denomination);

            // Retrieve the values
            Assert.AreEqual(_target.GetServerProgressiveLevelId(levelId), expectedResponse);
        }
    }
}