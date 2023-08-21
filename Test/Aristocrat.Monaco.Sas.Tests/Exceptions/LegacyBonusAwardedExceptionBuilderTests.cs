namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System.Collections.ObjectModel;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Exceptions;

    [TestClass]
    public class LegacyBonusAwardedExceptionBuilderTests
    {
        [TestMethod]
        public void LegacyBonusAwardedExceptionBuilderTest()
        {
            const long amount = 1000;
            const TaxStatus taxStatus = TaxStatus.WagerMatch;

            var expected = new Collection<byte>
            {
                (byte)GeneralExceptionCode.LegacyBonusPayAwarded,
                0, 0, 0, 0, 0,
                (byte)taxStatus,
                Utilities.ToBcd((ulong)amount.MillicentsToCents(), SasConstants.Bcd8Digits)
            };

            var actual = new LegacyBonusAwardedExceptionBuilder(amount, taxStatus, 1);
            CollectionAssert.AreEquivalent(expected, actual);
            Assert.AreEqual(GeneralExceptionCode.LegacyBonusPayAwarded, actual.ExceptionCode);
        }
    }
}