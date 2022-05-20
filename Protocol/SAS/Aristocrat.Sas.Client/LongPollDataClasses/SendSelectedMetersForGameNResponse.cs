namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <inheritdoc />
    public class SendSelectedMetersForGameNResponse : LongPollMultiDenomAwareResponse
    {
        /// <summary>
        ///     Gets the list of selected meters for game n response
        /// </summary>
        public IReadOnlyCollection<SelectedMeterForGameNResponse> SelectedMeters { get; }

        /// <summary>
        ///     Create the SendSelectedMetersForGameNResponse
        /// </summary>
        /// <param name="selectedMeters">The list of selected meters</param>
        public SendSelectedMetersForGameNResponse(IReadOnlyCollection<SelectedMeterForGameNResponse> selectedMeters)
        {
            SelectedMeters = selectedMeters;
        }
    }
}
