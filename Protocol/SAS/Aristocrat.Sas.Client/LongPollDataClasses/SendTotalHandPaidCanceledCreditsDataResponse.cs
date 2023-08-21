namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class SendTotalHandPaidCanceledCreditsDataResponse : LongPollResponse
    {
        public bool Succeeded { get; set; }
        
        public ulong MeterValue { get; set; }
    }

    public class SendTotalHandPaidCanceledCreditsData : LongPollData
    {
        public int GameId { get; set; }

        public long AccountingDenom { get; set; }
    }
}