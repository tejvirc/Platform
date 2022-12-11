namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using ProtoBuf;

    /// <summary>
    ///     A bill data exception builder
    /// </summary>
    [ProtoContract]
    public class BillDataExceptionBuilder : List<byte>, ISasExceptionCollection
    {
        /// <summary>
        ///     Creates a BillDataExceptionBuilder
        /// </summary>
        /// <param name="billData">The bill data used for creating the exception</param>
        public BillDataExceptionBuilder(BillData billData)
        {
            ExceptionCode = SasBillException.GetBillException(billData.AmountInCents);
            var denomCode = Utilities.ConvertBillValueToDenominationCode(billData.AmountInCents);
            Add((byte)GeneralExceptionCode.BillAccepted);
            AddRange(Utilities.ToBcd((ulong)billData.CountryCode, SasConstants.Bcd2Digits));
            AddRange(Utilities.ToBcd(denomCode < 0 ? 0 : (ulong)denomCode, SasConstants.Bcd2Digits));
            AddRange(Utilities.ToBcd((ulong)billData.LifetimeCount, SasConstants.Bcd8Digits));
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public BillDataExceptionBuilder()
        { }

        /// <inheritdoc />
        [ProtoMember(1)]
        public GeneralExceptionCode ExceptionCode { get; }
    }
}