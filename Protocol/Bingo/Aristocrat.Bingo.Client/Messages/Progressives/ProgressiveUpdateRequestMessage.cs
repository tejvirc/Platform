namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Message to send to request a progressive update from the server
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