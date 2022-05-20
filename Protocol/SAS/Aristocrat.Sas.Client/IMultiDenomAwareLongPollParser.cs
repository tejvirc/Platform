namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;
    public interface IMultiDenomAwareLongPollParser : ILongPollParser
    {
        /// <summary>
        ///     Parses a long poll and gets a response, accounting for requested denomination
        /// </summary>
        /// <param name="command">The long poll message</param>
        /// <param name="denom">Denomination requested for the long poll, expressed in cents</param>
        /// <returns>The response for the long poll, or null if there is no response</returns>
        IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command, long denom);
    }
}