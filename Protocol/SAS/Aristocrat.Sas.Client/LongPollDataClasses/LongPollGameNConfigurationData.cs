namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class LongPollGameNConfigurationData : LongPollData
    {
        public ulong GameNumber { get; set; }

        public long AccountingDenom { get; set; }
    }
}
