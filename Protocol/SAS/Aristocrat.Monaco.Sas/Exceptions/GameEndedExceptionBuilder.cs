namespace Aristocrat.Monaco.Sas.Exceptions;

using System.Collections.Generic;
using Aristocrat.Sas.Client;

/// <summary>
///     A game ended exception builder
/// </summary>
public class GameEndedExceptionBuilder : List<byte>, ISasExceptionCollection
{
    /// <summary>
    ///     Creates a GameEndedExceptionBuilder
    /// </summary>
    /// <param name="winAmount">The game end win amount used to create the exception</param>
    /// <param name="accountingDenom">The accounting denom to use</param>
    public GameEndedExceptionBuilder(long winAmount, long accountingDenom)
    {
        Add((byte)ExceptionCode);
        AddRange(Utilities.ToBcd((ulong)winAmount.CentsToAccountingCredits(accountingDenom), SasConstants.Bcd8Digits));
    }

    /// <summary>
    ///     Parameterless constructor used while deseriliazing
    /// </summary>
    public GameEndedExceptionBuilder()
    {
    }

    /// <inheritdoc />
    public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.GameHasEnded;
}