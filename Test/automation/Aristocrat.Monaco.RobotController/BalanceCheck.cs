namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Vgt.Client12.Testing.Tools;

    internal class BalanceCheck : IRobotService, IDisposable
    {
        private Timer _balanceCheckTimer;
        private readonly Configuration _config;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IGamePlayState _gamePlayState;
        private IBank _bank;
        private readonly ILog _logger;
        private IEventBus _eventBus;
        private bool _disposed;

        public BalanceCheck(Configuration config, ILobbyStateManager lobbyStateManager, IGamePlayState gamePlayState, IBank bank, ILog logger, IEventBus eventBus)
        {
            _config = config;
            _lobbyStateManager = lobbyStateManager;
            _bank = bank;
            _logger = logger;
            _eventBus = eventBus;
            _gamePlayState = gamePlayState;
        }

        ~BalanceCheck()
        {
            Dispose(false);
        }

        public string Name => typeof(BalanceCheck).FullName;

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<BalanceCheckEvent>(this, HandleEvent);
        }

        private void HandleEvent(BalanceCheckEvent obj)
        {
            _logger.Info($"state : [{_lobbyStateManager.CurrentState}] - BalanceCheck Begins.");
            if (_lobbyStateManager.CurrentState is LobbyState.Game)
            {
                _bank = Helper.GetBankInfo(_bank, _logger);
                Helper.CheckNegativeBalance(_lobbyStateManager, _bank, _logger);
                long minBalance = _config.GetMinimumBalance();
                long balance = _bank.QueryBalance();
                if (balance <= minBalance * 1000)
                {
                    _logger.Info($"state : [{_lobbyStateManager.CurrentState}] - Insufficient balance.  Balance: {balance}, Minimum Balance: {minBalance * 1000}");
                    InsertCredit();
                }
            }
            else
            {
                _logger.Info($"state : [{_lobbyStateManager.CurrentState}] - BalanceCheck Invalidated.");
            }
        }


        private void InsertCredit()
        {
            //inserting credits can lead to race conditions that make the platform not update the runtime balance
            //we now support inserting credits during game round for some jurisdictions
            if (_gamePlayState.CurrentState != PlayState.Idle || _config?.Active?.InsertCreditsDuringGameRound == false) { return; }
            _eventBus.Publish(new DebugNoteEvent(_config.GetDollarsInserted()));

        }

        public ICollection<Type> ServiceTypes => throw new NotImplementedException();

        public void Execute()
        {
            _balanceCheckTimer = new Timer(
                                (sender) =>
                                {
                                    _eventBus.Publish(new BalanceCheckEvent());
                                },
                                null,
                                _config.Active.IntervalBalanceCheck,
                                Timeout.Infinite);
        }

        public void Halt()
        {
            _balanceCheckTimer.Dispose();
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
                _balanceCheckTimer.Dispose();
            }

            _disposed = true;
        }

        public void Initialize()
        {
            SubscribeToEvents();
        }
    }

    internal static class Helper
    {
        public static void CheckNegativeBalance(ILobbyStateManager lobbyStateManager, IBank _bank, ILog Logger)
        {
            Logger.Info($"state : [{lobbyStateManager.CurrentState}] - Platform balance: {_bank.QueryBalance()}");
            if (_bank.QueryBalance() < 0)
            {
                Logger.Fatal("NEGATIVE BALANCE DETECTED");
                throw new Exception($"state : [{lobbyStateManager.CurrentState}] NEGATIVE BALANCE DETECTED");
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
                Logger.Info("BalanceCheck. _bank is null);");
            }

            return _bank;
        }
    }
}
