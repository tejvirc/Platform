namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using ProtoBuf;

    /// <summary>
    ///     A Game started exception builder
    /// </summary>
    [ProtoContract]
    public class GameStartedExceptionBuilder : List<byte>, ISasExceptionCollection
    {
        /// <summary>
        ///     Creates the GameStartedExceptionBuilder
        /// </summary>
        /// <param name="gameStartData">The game started data used for creating the exception</param>
        /// <param name="accountingDenom">The accounting denom to use</param>
        public GameStartedExceptionBuilder(GameStartData gameStartData, long accountingDenom)
        {
            Add((byte)ExceptionCode);
            AddRange(Utilities.ToBcd((ulong)gameStartData.CreditsWagered, SasConstants.Bcd4Digits));
            AddRange(
                Utilities.ToBcd(
                    (ulong)gameStartData.CoinInMeter.MillicentsToAccountCredits(accountingDenom),
                    SasConstants.Bcd8Digits));
            Add(gameStartData.WagerType);
            Add(gameStartData.ProgressiveGroup);
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public GameStartedExceptionBuilder()
        { }

        /// <inheritdoc />
        [ProtoMember(1)]
        public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.GameHasStarted;
    }
}