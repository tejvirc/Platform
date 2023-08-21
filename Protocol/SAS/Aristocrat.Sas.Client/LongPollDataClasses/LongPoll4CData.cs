namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    /// Data holder class for the SetValidationIdNumber long poll
    /// </summary>
    public class LongPoll4CData : LongPollData
    {
        public uint MachineValidationId { get; set; }
        public uint SequenceNumber { get; set; }
    }

    /// <summary>
    /// Data holder class for the SetValidationIdNumber long poll
    /// </summary>
    public class LongPoll4CResponse : LongPollResponse
    {
        public bool UsingSecureEnhancedValidation { get; set; }
        public uint MachineValidationId { get; set; }
        public uint SequenceNumber { get; set; }
    }
}
