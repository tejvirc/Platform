namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System.Collections.ObjectModel;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Exceptions;

    [TestClass]
    public class GameSelectedExceptionBuilderTests
    {
        [TestMethod]
        public void GameSelectedExceptionBuilderTest()
        {
            const int gameId = 3;
            var expectedResults = new Collection<byte>
            {
                (byte)GeneralExceptionCode.GameSelected,
                Utilities.ToBcd(gameId, SasConstants.Bcd4Digits)
            };

            var actual = new GameSelectedExceptionBuilder(gameId);
            CollectionAssert.AreEquivalent(expectedResults, actual);
            Assert.AreEqual(GeneralExceptionCode.GameSelected, actual.ExceptionCode);
        }
    }
}