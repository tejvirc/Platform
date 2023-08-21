namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System.Collections.ObjectModel;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Exceptions;

    [TestClass]
    public class GameEndedExceptionBuilderTests
    {
        [TestMethod]
        public void GameEndedExceptionBuilderTest()
        {
            const int accountingDenom = 1;
            const long winAmount = 1000;

            var expectedResult = new Collection<byte>
            {
                (byte)GeneralExceptionCode.GameHasEnded,
                Utilities.ToBcd((ulong)winAmount.CentsToAccountingCredits(accountingDenom), SasConstants.Bcd8Digits)
            };

            var actual = new GameEndedExceptionBuilder(winAmount, accountingDenom);
            CollectionAssert.AreEquivalent(expectedResult, actual);
            Assert.AreEqual(GeneralExceptionCode.GameHasEnded, actual.ExceptionCode);
        }
    }
}