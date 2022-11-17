namespace Aristocrat.Monaco.Bingo.Commands
{
    public class ProgressiveInfoRequestCommand
    {
        public ProgressiveInfoRequestCommand(
            string machineSerial,
            int gameTitleId)
        {
            MachineSerial = machineSerial;
            GameTitleId = gameTitleId;
        }

        public string MachineSerial { get; }

        public int GameTitleId { get; }
    }
}