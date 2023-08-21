namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System.Collections.ObjectModel;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Exceptions;

    [TestClass]
    public class BillDataExceptionBuilderTests
    {
        [DataRow((long)1_00, BillDenominationCodes.One, GeneralExceptionCode.BillAccepted1)]
        [DataRow((long)2_00, BillDenominationCodes.Two, GeneralExceptionCode.BillAccepted2)]
        [DataRow((long)5_00, BillDenominationCodes.Five, GeneralExceptionCode.BillAccepted5)]
        [DataRow((long)10_00, BillDenominationCodes.Ten, GeneralExceptionCode.BillAccepted10)]
        [DataRow((long)20_00, BillDenominationCodes.Twenty, GeneralExceptionCode.BillAccepted20)]
        [DataRow((long)50_00, BillDenominationCodes.Fifty, GeneralExceptionCode.BillAccepted50)]
        [DataRow((long)100_00, BillDenominationCodes.OneHundred, GeneralExceptionCode.BillAccepted100)]
        [DataRow((long)200_00, BillDenominationCodes.TwoHundred, GeneralExceptionCode.BillAccepted200)]
        [DataRow((long)500_00, BillDenominationCodes.FiveHundred, GeneralExceptionCode.BillAccepted500)]
        [DataRow((long)1_000_000_00, BillDenominationCodes.OneMillion, GeneralExceptionCode.BillAccepted)]
        [DataTestMethod]
        public void GeneralBillExceptionTest(long amount, BillDenominationCodes denomCode, GeneralExceptionCode exceptionCode)
        {
            var billData = new BillData
            {
                AmountInCents = amount,
                CountryCode = BillAcceptorCountryCode.UnitedStates,
                LifetimeCount = 10
            };

            var expectedResult = new Collection<byte>
            {
                (byte)GeneralExceptionCode.BillAccepted, // RTE Mode always wants Bill Accepted
                Utilities.ToBcd((ulong)billData.CountryCode, SasConstants.Bcd2Digits),
                Utilities.ToBcd((ulong)denomCode, SasConstants.Bcd2Digits),
                Utilities.ToBcd((ulong)billData.LifetimeCount, SasConstants.Bcd8Digits)
            };

            var actual = new BillDataExceptionBuilder(billData);
            CollectionAssert.AreEquivalent(expectedResult, actual);
            Assert.AreEqual(exceptionCode, actual.ExceptionCode);
        }
    }
}