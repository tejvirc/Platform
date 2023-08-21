namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    public class LongPollEnabledPlayerDenominationsResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollEnabledPlayerDenominationsResponse class
        /// </summary>
        /// <param name="enabledDenominations">list of denominations codes from table c-4</param>
        public LongPollEnabledPlayerDenominationsResponse(IReadOnlyCollection<byte> enabledDenominations)
        {
            EnabledDenominations = enabledDenominations;
        }

        /// <summary>
        ///    List of denominations by code from table c-4.
        /// </summary>
        public IReadOnlyCollection<byte> EnabledDenominations { get; }
    }
}
