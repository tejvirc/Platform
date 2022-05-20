namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class SendCashOutLimitData : LongPollData
    {
        public int GameId { get; set; }

        public long AccountingDenom { get; set; }
    }
}