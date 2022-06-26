namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using System;
    using System.Threading;
    using Vgt.Client12.Testing.Tools;

    internal class BalanceCheck : IRobotOperations, IDisposable
    {
        private IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly ILog _logger;
        private Timer _balanceCheckTimer;
        private IBank _bank;
        private bool _disposed;
        private bool _enabled;
        private static BalanceCheck instance = null;
        private static readonly object padlock = new object();
        public static BalanceCheck Instatiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new BalanceCheck(robotInfo);
                }
                return instance;
            }
        }
        private BalanceCheck(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _bank = robotInfo.ContainerService.Container.GetInstance<IBank>();
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
        }
        ~BalanceCheck() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _balanceCheckTimer = new Timer(
                                (sender) =>
                                {
                                    if (!_enabled || !IsValid()) { return; }
                                    _eventBus.Publish(new BalanceCheckEvent());
                                },
                                null,
                                1000,
                                _config.Active.IntervalBalanceCheck);
            _enabled = true;
        }
        public void Halt()
        {
            _enabled = false;
            _balanceCheckTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<BalanceCheckEvent>(this, HandleEvent);
        }
        private void HandleEvent(BalanceCheckEvent obj)
        {
            if (!IsValid())
            {
                _logger.Info($"BalanceCheck Invalidated due to Game wasn't running.");
            }
            _bank = GetBankInfo(_bank, _logger);
            CheckNegativeBalance(_bank, _logger);
            InsertCredit();
        }
        private bool IsValid()
        {
            return _sc.IsGame;
        }
        private void InsertCredit()
        {
            var enoughBlanace = _bank.QueryBalance() > _config.GetMinimumBalance() * 1000;
            var hasEdgeCase = _config?.Active?.InsertCreditsDuringGameRound == true;
            //inserting credits can lead to race conditions that make the platform not update the runtime balance
            //we now support inserting credits during game round for some jurisdictions
            if (enoughBlanace || (!_sc.IsIdle && !hasEdgeCase))
            {
                return;
            }
            _logger.Info($"Insufficient balance.");
            _eventBus.Publish(new DebugNoteEvent(_config.GetDollarsInserted()));
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _balanceCheckTimer?.Dispose();
            }
            _disposed = true;
        }
        //Todo: Move these to a static Helper Class
        public static void CheckNegativeBalance(IBank _bank, ILog Logger)
        {
            Logger.Info($"Platform balance: {_bank.QueryBalance()}");
            if (_bank.QueryBalance() < 0)
            {
                Logger.Fatal("NEGATIVE BALANCE DETECTED");
                throw new Exception($"NEGATIVE BALANCE DETECTED");
            }
        }
        public static IBank GetBankInfo(IBank _bank, ILog Logger)
        {
            if (_bank == null)
            {
                _bank = ServiceManager.GetInstance().GetService<IBank>();
            }
            if (_bank == null)
            {
                Logger.Info("_bank is null);");
            }

            return _bank;
        }
    }
}
