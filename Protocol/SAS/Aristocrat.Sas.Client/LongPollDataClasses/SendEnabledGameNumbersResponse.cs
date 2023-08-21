namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <inheritdoc />
    public class SendEnabledGameNumbersResponse : LongPollMultiDenomAwareResponse
    {
        /// <summary>
        ///     The Send Enabled Game Numbers Response message
        /// </summary>
        /// <param name="enabledGameIds">The enabled games ids</param>
        public SendEnabledGameNumbersResponse(IReadOnlyCollection<long> enabledGameIds)
        {
            EnabledGameIds = enabledGameIds;
        }

        /// <summary>
        ///     Gets the enabled games ids
        /// </summary>
        public IReadOnlyCollection<long> EnabledGameIds { get; }
    }
}