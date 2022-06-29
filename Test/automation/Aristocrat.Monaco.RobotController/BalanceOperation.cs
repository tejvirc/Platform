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

    internal class BalanceOperation : IRobotOperations, IDisposable
    {
        private IEventBus _eventBus;
        private readonly Configuration _config;
        private readonly StateChecker _sc;
        private readonly ILog _logger;
        private readonly Automation _automator;
        private Timer _balanceCheckTimer;
        private IBank _bank;
        private bool _disposed;
        private static BalanceOperation instance = null;
        private static readonly object padlock = new object();
        public static BalanceOperation Instantiate(RobotInfo robotInfo)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new BalanceOperation(robotInfo);
                }
                return instance;
            }
        }
        private BalanceOperation(RobotInfo robotInfo)
        {
            _config = robotInfo.Config;
            _sc = robotInfo.StateChecker;
            _bank = robotInfo.ContainerService.Container.GetInstance<IBank>();
            _logger = robotInfo.Logger;
            _eventBus = robotInfo.EventBus;
            _automator = robotInfo.Automator;
        }
        ~BalanceOperation() => Dispose(false);
        public void Execute()
        {
            SubscribeToEvents();
            _balanceCheckTimer = new Timer(
                                (sender) =>
                                {
                                    if (!IsValid()) { return; }
                                    _eventBus.Publish(new BalanceCheckEvent());
                                },
                                null,
                                _config.Active.IntervalBalanceCheck,
                                _config.Active.IntervalBalanceCheck);
        }
        public void Halt()
        {
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
                if   (evt.Handpay == HandpayType.GameWin ||
                     evt.Handpay == HandpayType.CancelCredit)
                {
                    _logger.Info("Keying off large win");
                    HandlerWaitCashOut();
                    Task.Delay(1000).ContinueWith(_ => _automator.JackpotKeyoff()).ContinueWith(_ => _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30)));
                }
            });
            _eventBus.Subscribe<CashOutStartedEvent>(this, _ =>
            {
               //log
            });
            _eventBus.Subscribe<VoucherIssuedEvent>(this, evt =>
            {
                //_lastVoucherIssued = evt.Transaction;
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
                //if (_previousVoucherBalance.Amount == evt.CashableAmount &&
                //    evt.Timestamp.Subtract(_previousVoucherBalance.TimeStamp).TotalSeconds < Constants.DuplicateVoucherWindow)
                //{
                //    LogFatal("Possible identical vouchers detected. Disabling.");
                //    Enabled = false;
                //}

                //_previousVoucherBalance.TimeStamp = evt.Timestamp;
                //_previousVoucherBalance.Amount = evt.CashableAmount;
            });
        }
        private void HandlerWaitCashOut()
        {
            //_waitDuration += _config.Active.IntervalResolution;

            //if (TimeTo(_counter, 500))
            //{
            //    _automator.DismissTimeLimitDialog(_isTimeLimitDialogVisible);
            //}

            //if (_waitDuration > Constants.CashOutTimeout)
            //{
            //    _waitDuration = 0;
            //    _automator.EnableCashOut(false);
            //    _automator.EnableExitToLobby(false);
            //    ControllerState = (RobotControllerState.Running);
            //}
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
        private void HandlerInsertVoucher()
        {
            //if (IsPlatformIdle())
            //{
            //    _waitDuration += _config.Active.IntervalResolution;

            //    if (_lastVoucherIssued != null)
            //    {
            //        _automator.InsertVoucher(_lastVoucherIssued.Barcode);

            //        _eventBus.Subscribe<VoucherRedeemedEvent>(this, _ =>
            //        {
            //            _voucherRedeemed = true;
            //        });

            //        ControllerState = (RobotControllerState.InsertVoucherComplete);
            //    }
            //    else
            //    {
            //        ControllerState = (PreviousControllerState);
            //    }
            //}
            //else
            //{
            //    _waitDuration = 0;

            //    DriveToIdle();
            //}
            //private void HandlerInsertVoucherComplete()
            //{
            //    if (_voucherRedeemed)
            //    {
            //        _timeoutDuration = 0;

            //        _eventBus.Unsubscribe<VoucherRedeemedEvent>(this);

            //        _lastVoucherIssued = null;

            //        ControllerState = (PreviousControllerState);
            //    }
            //    else
            //    {
            //        _timeoutDuration += _config.Active.IntervalResolution;
            //    }
            //}
        }
    }
}
