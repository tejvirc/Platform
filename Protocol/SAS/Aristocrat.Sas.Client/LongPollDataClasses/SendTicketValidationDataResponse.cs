namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Response class to hold the response of LP 70 - Send Ticket Validation Data
    /// </summary>
    public class SendTicketValidationDataResponse : LongPollResponse
    {
        public ParsingCode ParsingCode { set; get; }

        public string Barcode { set; get; }
    }
}
