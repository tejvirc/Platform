namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.Models
{
    using System;
    using Common.GAT.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetLogStatusResultTest
    {
        [TestMethod]
        public void WhenConstructWithArgsExpectValidPropertiesSet()
        {
            var lastSequence = 1;
            var totalEntries = 1;

            var result = new GetLogStatusResult(lastSequence, totalEntries);

            Assert.AreEqual(lastSequence, result.LastSequence);
            Assert.AreEqual(totalEntries, result.TotalEntries);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenConstructWithInvalidLastSequenceExpectException()
        {
            var lastSequence = -1;
            var totalEntries = 1;

            var result = new GetLogStatusResult(lastSequence, totalEntries);

            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenConstructWithInvalidTotalEntriesExpectException()
        {
            var lastSequence = 1;
            var totalEntries = -1;

            var result = new GetLogStatusResult(lastSequence, totalEntries);

            Assert.IsNull(result);
        }
    }
}