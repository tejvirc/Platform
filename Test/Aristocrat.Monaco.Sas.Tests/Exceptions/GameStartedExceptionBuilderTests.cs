namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System.Collections.ObjectModel;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Exceptions;

    [TestClass]
    public class GameStartedExceptionBuilderTests
    {
        [TestMethod]
        public void GameStartedExceptionBuilderTest()
        {
            var gameStartData = new GameStartData { CoinInMeter = 1000000, CreditsWagered = 30, ProgressiveGroup = 6, WagerType = 54 };
            const int accountingDenom = 1;

            var expectedResult = new Collection<byte>
            {
                (byte)GeneralExceptionCode.GameHasStarted,
                Utilities.ToBcd((ulong)gameStartData.CreditsWagered, SasConstants.Bcd4Digits),
                Utilities.ToBcd((ulong)gameStartData.CoinInMeter.MillicentsToAccountCredits(accountingDenom), SasConstants.Bcd8Digits),
                gameStartData.WagerType,
                gameStartData.ProgressiveGroup
            };

            var actual = new GameStartedExceptionBuilder(gameStartData, accountingDenom);
            CollectionAssert.AreEquivalent(expectedResult, actual);
            Assert.AreEqual(GeneralExceptionCode.GameHasStarted, actual.ExceptionCode);
        }
    }
}