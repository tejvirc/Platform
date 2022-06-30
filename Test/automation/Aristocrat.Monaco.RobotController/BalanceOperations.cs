namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts.Vouchers;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Automation;
    using log4net;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class BalanceOperations : IRobotOperations, IDisposable
    {
        private IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly ILog _logger;
        private readonly Automation _automator;
        private IBank _bank;
        private Timer _balanceCheckTimer;
        private bool _disposed;
        private static BalanceOperations instance = null;
        private static readonly object padlock = new object();
        public static BalanceOperations Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new BalanceOperations(robotInfo);
                }
                return instance;
            }
        }
        private BalanceOperations(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _bank = robotInfo.ContainerService.Container.GetInstance<IBank>();
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _automator = robotInfo.Automator;
        }
        ~BalanceOperations() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();

            if (_config.Active.IntervalBalanceCheck == 0) { return; }
            _balanceCheckTimer = new Timer(
                                (sender) =>
                                {
                                    CheckBalance();
                                },
                                null,
                                _config.Active.IntervalBalanceCheck,
                                _config.Active.IntervalBalanceCheck);
        }

        private void CheckBalance()
        {
            if (!IsValid())
            {
                _logger.Info($"BalanceCheck Invalidated due to Game wasn't running.");
            }
            _bank = GetBankInfo(_bank, _logger);
            CheckNegativeBalance(_bank, _logger);
            InsertCredit();
        }

        public void Halt()
        {
            _balanceCheckTimer?.Dispose();
        }
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<BalanceCheckEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherRejectedEvent>(this, evt =>
            {
                if (evt.Transaction.Exception.Equals((int)VoucherInExceptionCode.CreditInLimitExceeded) ||
                    evt.Transaction.Exception.Equals((int)VoucherInExceptionCode.LaundryLimitExceeded))
                {
                    _eventBus.Publish(new CashOutButtonPressedEvent());
                }
            });

            _eventBus.Subscribe<HandpayStartedEvent>(this, evt =>
            {
                if   (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info("Keying off large win");
                    Task.Delay(1000).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
                }
            });
            _eventBus.Subscribe<CashOutStartedEvent>(this, _ =>
            {
               //log
            });
            _eventBus.Subscribe<VoucherIssuedEvent>(this, evt =>
            {
                //log
            });
            _eventBus.Subscribe<TransferOutFailedEvent>(this, _ =>
            {
                //log
            });
            _eventBus.Subscribe<CashOutAbortedEvent>(this, _ =>
            {
                //log
            });
            _eventBus.Subscribe<HandpayKeyedOffEvent>(this, _ =>
            {
               //log
            });
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, evt =>
            {
                //log
            });
        }
        
        private void HandleEvent(BalanceCheckEvent obj)
        {
            CheckBalance();
        }
        private bool IsValid()
        {
            return _sc.IsGame && !_sc.IsInRecovery;
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
            _automator.InsertDollars(_config.GetDollarsInserted());
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
