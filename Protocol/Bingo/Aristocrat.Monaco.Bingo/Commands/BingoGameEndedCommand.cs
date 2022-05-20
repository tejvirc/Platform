namespace Aristocrat.Monaco.Bingo.Commands
{
    using Gaming.Contracts;
    using Gaming.Contracts.Central;

    public class BingoGameEndedCommand
    {
        public BingoGameEndedCommand(string machineSerial, CentralTransaction transaction, IGameHistoryLog log)
        {
            MachineSerial = machineSerial;
            Transaction = transaction;
            Log = log;
        }

        public string MachineSerial { get; }

        public CentralTransaction Transaction { get; }

        public IGameHistoryLog Log { get; }
    }
}