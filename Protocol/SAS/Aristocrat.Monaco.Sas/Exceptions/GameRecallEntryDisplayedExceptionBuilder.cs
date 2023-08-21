namespace Aristocrat.Monaco.Sas.Exceptions;

using System.Collections.Generic;
using Aristocrat.Sas.Client;

/// <summary>
///     A game recall entry displayed exception builder
/// </summary>
public class GameRecallEntryDisplayedExceptionBuilder : List<byte>, ISasExceptionCollection
{
    /// <summary>
    ///     Creates a GameRecallEntryDisplayedExceptionBuilder
    /// </summary>
    /// <param name="gameNumber">The game number used for this exception</param>
    /// <param name="recallEntry">The recall entry number used for this exception</param>
    public GameRecallEntryDisplayedExceptionBuilder(long gameNumber, long recallEntry)
    {
        Add((byte)ExceptionCode);
        AddRange(Utilities.ToBcd((ulong)gameNumber, SasConstants.Bcd4Digits));
        AddRange(Utilities.ToBcd((ulong)recallEntry, SasConstants.Bcd4Digits));
    }

    /// <summary>
    ///     Parameterless constructor used while deseriliazing
    /// </summary>
    public GameRecallEntryDisplayedExceptionBuilder()
    {
    }

    /// <inheritdoc />
    public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.GameRecallEntryHasBeenDisplayed;
}