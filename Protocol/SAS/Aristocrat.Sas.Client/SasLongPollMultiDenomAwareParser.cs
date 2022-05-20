namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public abstract class SasLongPollMultiDenomAwareParser<TResponse, TData> : SasLongPollParser<TResponse, TData>,
        IMultiDenomAwareLongPollParser
        where TResponse : LongPollMultiDenomAwareResponse
        where TData : LongPollMultiDenomAwareData, new()
    {
        /// <inheritdoc/>
        protected SasLongPollMultiDenomAwareParser(LongPoll command)
            : base(command)
        {
        }

        /// <inheritdoc/>
        public abstract IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom);

        /// <summary>
        ///     Generates a multidenom-aware error message.
        ///     Utility to simplify returning errors from multi-denom aware parsers.
        /// </summary>
        /// <param name="address">The command address, in byte form, we will return from</param>
        /// <param name="error">The desired error message to return</param>
        /// <returns>The whole return error message as a byte collection</returns>
        /// <remarks>
        ///     Generates a sequence of bytes so we can simply return this from parse functions.
        ///     We also need the address for completeness. Technically we can forgo it, since
        ///     the LPB0 parser will just discard it anyway, but because all other parser
        ///     responses will return a valid address, we return a valid address here too for
        ///     consistency's sake.
        /// </remarks>
        protected Collection<byte> GenerateMultiDenomAwareError(byte address, MultiDenomAwareErrorCode error) =>
            new Collection<byte> { address, 0, (byte)error };
    }
}