namespace Aristocrat.Monaco.Bingo.Commands
{
    public class ProgressiveUpdateRequestCommand
    {
        public ProgressiveUpdateRequestCommand(string machineSerial)
        {
            MachineSerial = machineSerial;
        }

        public string MachineSerial { get; }
    }
}