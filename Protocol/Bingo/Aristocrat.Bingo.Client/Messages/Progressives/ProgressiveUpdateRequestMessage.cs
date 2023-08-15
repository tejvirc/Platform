namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Message to send to request progressive updates from the server for this EGM.
    /// </summary>
    public class ProgressiveUpdateRequestMessage : IMessage
    {
        public ProgressiveUpdateRequestMessage(string machineSerial)
        {
            MachineSerial = machineSerial;
        }

        public string MachineSerial { get; }
    }
}