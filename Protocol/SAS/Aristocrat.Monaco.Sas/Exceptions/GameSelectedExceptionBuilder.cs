namespace Aristocrat.Monaco.Sas.Exceptions;

using System.Collections.Generic;
using Aristocrat.Sas.Client;
using ProtoBuf;

/// <summary>
///     A game selected exception builder
/// </summary>
public class GameSelectedExceptionBuilder : List<byte>, ISasExceptionCollection
{
    /// <summary>
    ///     Creates a GameSelectedExceptionBuilder
    /// </summary>
    /// <param name="gameId">The game id used for creating this exception</param>
    public GameSelectedExceptionBuilder(int gameId)
    {
        Add((byte)ExceptionCode);
        AddRange(Utilities.ToBcd((ulong)gameId, SasConstants.Bcd4Digits));
    }

    /// <summary>
    ///     Parameterless constructor used while deseriliazing
    /// </summary>
    public GameSelectedExceptionBuilder()
    {
    }

    /// <inheritdoc />
    [ProtoMember(1)]
    public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.GameSelected;
}