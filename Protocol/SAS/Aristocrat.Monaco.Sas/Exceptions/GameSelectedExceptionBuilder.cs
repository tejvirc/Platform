namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     A game selected exception builder
    /// </summary>
    [Serializable]
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

        /// <inheritdoc />
        public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.GameSelected;
    }
}