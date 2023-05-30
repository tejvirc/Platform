namespace Aristocrat.Monaco.Gaming.Class3
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Session;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Contracts;
    using Kernel;

    //Another option: Decorator implementation based on inheritance, 
    public class PlayerBankCOAM : PlayerBank
    {
        private readonly IPlayerBank _decorated;
        public PlayerBankCOAM(
            IPlayerBank decorated,
            IBank bank,
            ITransactionCoordinator transactionCoordinator,
            ITransferOutHandler transferOut,
            IPersistentStorageManager persistentStorage,
            IMeterManager meters,
            IPlayerService players,
            IEventBus bus,
            IGameHistory history) : base(bank, transactionCoordinator, transferOut, persistentStorage, meters, players, bus, history)
        {
            _decorated = decorated;
        }

        // this implies to have virtual CashOut method in base PlayerBank class

        //public override bool CashOut()
        //{
        //    return base.CashOut();
        //}
    }
}
