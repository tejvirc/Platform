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

        [DataRow(0, 0, DisplayName = "Sequence number zero")]
        [DataRow(-1, 0, DisplayName = "Sequence number negative")]
        [DataRow(1, -1, DisplayName = "Progressive level Id negative")]
        [ExpectedException(typeof(ArgumentException))]
        [DataTestMethod]
        public void AddProgressiveLevelInfoInvalidValuesTest(long levelId, int sequenceNumber)
        {
            _target.AddProgressiveLevelInfo(levelId, sequenceNumber);
        }

        [DataRow(-1, -1L,   DisplayName = "Progressive level Id invalid negative")]
        [DataRow(0, 10001L, DisplayName = "Progressive level Id 0")]
        [DataRow(1, 10002L, DisplayName = "Progressive level Id 1")]
        [DataRow(2, 10003L, DisplayName = "Progressive level Id 2")]
        [DataRow(3, -1,     DisplayName = "Progressive level Id invalid not existing")]
        [DataTestMethod]
        public void GetServerProgressiveLevelIdTest(int levelId, long expectedResponse)
        {
            // First add some values
            _target.AddProgressiveLevelInfo(10001L, 1);
            _target.AddProgressiveLevelInfo(10002L, 2);
            _target.AddProgressiveLevelInfo(10003L, 3);

            // Retrieve the values
            Assert.AreEqual(_target.GetServerProgressiveLevelId(levelId), expectedResponse);
        }
    }
}