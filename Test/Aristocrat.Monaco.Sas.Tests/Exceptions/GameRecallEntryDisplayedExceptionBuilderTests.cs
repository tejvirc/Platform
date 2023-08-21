namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System.Collections.ObjectModel;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Exceptions;

    [TestClass]
    public class GameRecallEntryDisplayedExceptionBuilderTests
    {
        [TestMethod]
        public void GameRecallEntryDisplayedExceptionBuilderTest()
        {
            const int gameNumber = 1;
            const long logIndex = 100;

            var expectedResult = new Collection<byte>
            {
                (byte)GeneralExceptionCode.GameRecallEntryHasBeenDisplayed,
                Utilities.ToBcd(gameNumber, SasConstants.Bcd4Digits),
                Utilities.ToBcd(logIndex, SasConstants.Bcd4Digits)
            };

            var actual = new GameRecallEntryDisplayedExceptionBuilder(gameNumber, logIndex);
            CollectionAssert.AreEquivalent(expectedResult, actual);
            Assert.AreEqual(GeneralExceptionCode.GameRecallEntryHasBeenDisplayed, actual.ExceptionCode);
        }
    }
}