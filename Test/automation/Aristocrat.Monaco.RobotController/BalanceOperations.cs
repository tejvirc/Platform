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
        private readonly IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly RobotLogger _logger;
        private readonly Automation _automator;
        private IBank _bank;
        private Timer _balanceCheckTimer;
        private bool _disposed;
        public BalanceOperations(IEventBus eventBus, RobotLogger logger, Automation automator, Configuration config, StateChecker sc)
        {
            _config = config;
            _sc = sc;
            _automator = automator;
            _logger = logger;
            _eventBus = eventBus;
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
                _logger.Error($"BalanceCheck Invalidated due to Game wasn't running.", GetType().Name);
            }
            _logger.Info($"BalanceCheck Requested.", GetType().Name);
            _bank = GetBankInfo(_bank, _logger);
            CheckNegativeBalance(_bank, _logger);
            InsertCredit();
        }
        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            _balanceCheckTimer?.Dispose();
            _eventBus.UnsubscribeAll(this);
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
                if (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info("Keying off large win", GetType().Name);
                    Task.Delay(1000).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
                }
            });
            _eventBus.Subscribe<CashOutStartedEvent>(this, _ =>
            {
                _logger.Info("CashOutStartedEvent Got Triggered!", GetType().Name);
            });
            _eventBus.Subscribe<VoucherIssuedEvent>(this, evt =>
            {
                _logger.Info("VoucherIssuedEvent Got Triggered!", GetType().Name);
            });
            _eventBus.Subscribe<TransferOutFailedEvent>(this, _ =>
            {
                _logger.Info("TransferOutFailedEvent Got Triggered!", GetType().Name);
            });
            _eventBus.Subscribe<CashOutAbortedEvent>(this, _ =>
            {
                _logger.Info("CashOutAbortedEvent Got Triggered!", GetType().Name);
            });
            _eventBus.Subscribe<HandpayKeyedOffEvent>(this, _ =>
            {
                _logger.Info("CashOutAbortedEvent Got Triggered!", GetType().Name);
            });
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, evt =>
            {
                _logger.Info("TransferOutCompletedEvent Got Triggered!", GetType().Name);
            });
        }
        private void HandleEvent(BalanceCheckEvent obj)
        {
            CheckBalance();
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
            _logger.Info($"Insufficient balance.", GetType().Name);
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
                _bank = null;
                _eventBus.UnsubscribeAll(this);
                _balanceCheckTimer?.Dispose();
            }
            _disposed = true;
        }
        public void CheckNegativeBalance(IBank _bank, RobotLogger Logger)
        {
            Logger.Info($"Platform balance: {_bank.QueryBalance()}", GetType().Name);
            if (_bank.QueryBalance() < 0)
            {
                Logger.Fatal("NEGATIVE BALANCE DETECTED", GetType().Name);
                throw new Exception($"NEGATIVE BALANCE DETECTED");
            }
        }
        public IBank GetBankInfo(IBank _bank, RobotLogger Logger)
        {
            if (_bank == null)
            {
                _bank = ServiceManager.GetInstance().GetService<IBank>();
            }
            if (_bank == null)
            {
                Logger.Info("_bank is null);", GetType().Name);
            }

            return _bank;
        }
    }
}
