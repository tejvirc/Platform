namespace Aristocrat.Monaco.Sas.Exceptions;

using System.Collections.Generic;
using Aristocrat.Sas.Client;
using Aristocrat.Sas.Client.LongPollDataClasses;

/// <summary>
///     Builds the legacy bonus award exception
/// </summary>
public class LegacyBonusAwardedExceptionBuilder : List<byte>, ISasExceptionCollection
{
    /// <summary>
    ///     Creates a LegacyBonusAwardedExceptionBuilder
    /// </summary>
    /// <param name="amount">The amount for awarded for this legacy bonus</param>
    /// <param name="taxStatus">The tax status for the awarded legacy bonus</param>
    /// <param name="accountingDenom"></param>
    public LegacyBonusAwardedExceptionBuilder(long amount, TaxStatus taxStatus, long accountingDenom)
    {
        Add((byte)ExceptionCode);
        Add(0x00);
        AddRange(Utilities.ToBcd(0x00, SasConstants.Bcd8Digits));
        Add((byte)taxStatus);
        AddRange(Utilities.ToBcd((ulong)amount.MillicentsToAccountCredits(accountingDenom), SasConstants.Bcd8Digits));
    }

    /// <summary>
    ///     Parameterless constructor used while deseriliazing
    /// </summary>
    public LegacyBonusAwardedExceptionBuilder()
    {
    }

    /// <inheritdoc />
    public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.LegacyBonusPayAwarded;
}