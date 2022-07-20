namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Common;
    using Contracts.Eft;
    using Gaming.Contracts;
    using Hardware.Contracts.Door;
    using Kernel;
    using log4net;
    using Stateless;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     <para>
    ///         (From section 8.EFT of the SAS v5.02 document)  -
    ///         https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     </para>
    ///     <para>
    ///         The EftStateController as mentioned in <see cref="IEftStateController" /> controls the workflow logic of the
    ///         EFT transaction between host and client.
    ///     </para>
    ///     <para>
    ///         It uses an ISystemTimerWrapper and state machine, consisting of 3 states of Idle, FirstPhase, SecondPhase. The
    ///         initial state is Idle.
    ///     </para>
    ///     <para>
    ///         To transition into any state the incoming host message will go through validation.
    ///         It holds references to the different elements of the system to check whether the system is in any erroneous
    ///         state and if so to set the response status accordingly.
    ///     </para>
    /// </summary>
    public sealed class EftStateController : IEftStateController, IDisposable
    {
        private const int EftTimeOutMs = 800;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISystemDisableManager _disableProvider;
        private readonly IDoorService _doorService;
        private readonly IGamePlayState _gamePlayState;
        private readonly IDisableByOperatorManager _disableByOperator;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IEftTransferProvider _eftTransferProvider;
        private readonly IEftHistoryLogProvider _historyLogProvider;
        private readonly object _finalExecutionLock = new();

        //FiringMode is hardcoded as FiringMode.Queued, please DO NOT change it as _transactionStateMachine
        //relies on this to behave correctly in the racing conditions.
        private readonly StateMachine<EftState, EftStateTrigger> _transactionStateMachine = new(EftState.Idle, FiringMode.Queued);

        private ISystemTimerWrapper _timer;
        private EftTransactionResponse _currentResponse;
        private EftTransferData _currentTransferData;
        private EftTransferData _prevTransferData;
        private IEftTransferHandler _eftTransferHandler;

        private bool _disposed;

        /// <summary>
        ///     Creates and returns a new instance of EftStateController.
        /// </summary>
        /// <param name="disableProvider">Instance of <see cref="ISystemDisableManager" />.</param>
        /// <param name="doorService">Instance of <see cref="IDoorService" />.</param>
        /// <param name="gamePlayState">Instance of <see cref="IGamePlayState" />.</param>
        /// <param name="disableByOperator">Instance of <see cref="IDisableByOperatorManager" />.</param>
        /// <param name="operatorMenu">Instance of <see cref="IOperatorMenuLauncher" />.</param>
        /// <param name="eftTransferProvider">Instance of <see cref="IEftTransferProvider" />.</param>
        /// <param name="historyLogProvider">Instance of <see cref="IEftHistoryLogProvider" />.</param>
        public EftStateController(
            ISystemDisableManager disableProvider,
            IDoorService doorService,
            IGamePlayState gamePlayState,
            IDisableByOperatorManager disableByOperator,
            IOperatorMenuLauncher operatorMenu,
            IEftTransferProvider eftTransferProvider,
            IEftHistoryLogProvider historyLogProvider)
        {
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _disableByOperator = disableByOperator ?? throw new ArgumentNullException(nameof(disableByOperator));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _eftTransferProvider = eftTransferProvider ?? throw new ArgumentNullException(nameof(eftTransferProvider));
            _historyLogProvider = historyLogProvider ?? throw new ArgumentNullException(nameof(historyLogProvider));

            SetTimer();

            ConfigureStateMachine();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void RecoverIfRequired(IEftTransferHandler handler)
        {
            var lastLog = _historyLogProvider.GetLastTransaction();
            //Check if recovery is required and related to the calling LP handler
            if (lastLog != null && handler.Commands.Any(c => c == lastLog.Command) && lastLog.ToBeProcessed)
            {
                //Check with the transfer provider if transaction was processed, if not process it through handler.
                if (!_eftTransferProvider.CheckIfProcessed(lastLog.TransactionNumber.ToString(), lastLog.TransferType))
                {
                    handler.ProcessTransfer(lastLog.ReportedTransactionAmount, lastLog.TransactionNumber);
                }

                _historyLogProvider.UpdateLogEntryForRequestCompleted(
                    lastLog.Command,
                    lastLog.TransactionNumber,
                    lastLog.RequestedTransactionAmount);
            }
        }

        /// <inheritdoc />
        public EftTransactionResponse Handle(EftTransferData data, IEftTransferHandler handler)
        {
            _eftTransferHandler = handler;
            _currentTransferData = data;
            _currentResponse = data.ToEftTransactionResponse();

            FireTrigger(EftStateTrigger.CommandReceived);

            _prevTransferData = _currentTransferData;

            _historyLogProvider.AddOrUpdateEntry(
                data,
                _currentResponse);

            return _currentResponse;
        }

        /// <summary>
        ///     Sets the timerWrapper as well as includes a stub method for unit testing purposes.
        /// </summary>
        /// <param name="timerWrapper">Timer to be mocked in unit testing.</param>
        public void SetTimer(ISystemTimerWrapper timerWrapper = null)
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimedOut;
            }

            _timer = timerWrapper ?? new SystemTimerWrapper { Interval = EftTimeOutMs, AutoReset = false };
            _timer.Elapsed += TimedOut;
        }

        /// <summary>
        ///     Disposes the object
        /// </summary>
        /// <param name="disposing">Whether or not the dispose the resources</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer.Dispose();
            }

            _disposed = true;
        }

        private void TimedOut(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            _logger.Info("Timed out.");

            FireTrigger(EftStateTrigger.TimerExpired);
        }

        private void ConfigureStateMachine()
        {
            _transactionStateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    _logger.Warn("Unregistered transition attempted from state " + state + " using trigger " + trigger);
                });

            _transactionStateMachine.OnTransitioned(
                transition => _logger.Info(
                    transition.Source + " state Transitioned into " + transition.Destination + " using trigger " +
                    transition.Trigger));

            ConfigureIdleState();

            ConfigureFirstPhaseState();

            ConfigureSecondPhaseState();
        }

        private void ConfigureIdleState()
        {
            _transactionStateMachine.Configure(EftState.Idle)
                .OnEntry(
                    () =>
                    {
                        UnlockEgm();
                        ResetPrevDataAndResponse();
                    }
                )
                .PermitIf(
                    EftStateTrigger.CommandReceived,
                    EftState.FirstPhase,
                    () =>
                        CheckAndSetTransactionStatus() && CheckIfValidFirstPhaseMessage() && CheckTransferAmount())
                .OnExit(LockEgm);
        }

        private void ConfigureFirstPhaseState()
        {
            _transactionStateMachine.Configure(EftState.FirstPhase)
                .OnEntry(() => _timer.Start())
                .Permit(EftStateTrigger.TimerExpired, EftState.Idle)
                .PermitDynamic(
                    EftStateTrigger.CommandReceived,
                    () => CheckAndSetTransactionStatus() && CheckIfValidSecondPhaseMessage() && CheckTransferAmount()
                        ? EftState.SecondPhase
                        : EftState.Idle
                )
                .OnExit(() => _timer.Stop());
        }

        private void ConfigureSecondPhaseState()
        {
            _transactionStateMachine.Configure(EftState.SecondPhase)
                .OnEntry(StartTimerAndSetAckMessage)
                .PermitIf(
                    EftStateTrigger.TimerExpired,
                    EftState.Idle,
                    ExecuteCommand)
                .PermitIf(EftStateTrigger.ImpliedNackReceived, EftState.Idle, NackCommand)
                .InternalTransition(
                    EftStateTrigger.ReAckRequested,
                    () =>
                    {
                        _eftTransferProvider.RestartCashoutTimer();
                        RestartTimer();
                    })
                .PermitDynamic(
                    EftStateTrigger.CommandReceived,
                    () => CheckAndSetTransactionStatus() && CheckIfValidSecondPhaseMessage() && CheckTransferAmount()
                        ? EftState.SecondPhase
                        : EftState.Idle
                )
                .OnExit(() => _timer.Stop());
        }

        private void RestartTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void HandleImpliedAck()
        {
            _logger.Info("ImpliedAck received.");

            _timer.Stop();
            FireTrigger(EftStateTrigger.TimerExpired);
        }

        //Please use this method to trigger state change
        //and do not fire the trigger on state machine directly.
        private void FireTrigger(EftStateTrigger trigger)
        {
            lock (_transactionStateMachine)
            {
                _transactionStateMachine.Fire(trigger);
            }
        }

        private void HandleImpliedNack()
        {
            _logger.Info("ImpliedNack received.");

            _timer.Stop();
            FireTrigger(EftStateTrigger.ImpliedNackReceived);
        }

        private bool NackCommand()
        {
            _historyLogProvider.UpdateLogEntryForNackedLP(
                    _currentTransferData.Command,
                    _currentResponse.TransactionNumber,
                    _currentTransferData.TransferAmount);

            return true;
        }

        private void ReAckRequested()
        {
            //Reset the timer as re-ack is issued by the host.
            _logger.Info("Re-Ack received.");
            FireTrigger(EftStateTrigger.ReAckRequested);
        }

        private void ResetPrevDataAndResponse()
        {
            _prevTransferData = null;
        }

        private bool CheckIfValidFirstPhaseMessage()
        {
            if (_currentTransferData.Acknowledgement)
            {
                _currentResponse.Status = TransactionStatus.InvalidAck;
                return false;
            }

            //Check if already processed using IHistoryLogProv
            var lastTransaction = _historyLogProvider.GetLastTransaction();
            if (lastTransaction != null
                && _currentTransferData.TransactionNumber == lastTransaction.TransactionNumber
                && _currentTransferData.Command == lastTransaction.Command
                && _currentTransferData.TransferAmount == lastTransaction.RequestedTransactionAmount
                && lastTransaction.Acknowledgement)
            {
                _currentResponse.Status = TransactionStatus.PreviouslyCompleted;
                _currentResponse.TransferAmount = 0;
                return false;
            }

            return true;
        }

        private bool CheckIfValidSecondPhaseMessage()
        {
            if (!_currentTransferData.Acknowledgement)
            {
                _currentResponse.Status = TransactionStatus.InvalidAck;
                return false;
            }

            if (_prevTransferData?.TransactionNumber != _currentTransferData.TransactionNumber)
            {
                _currentResponse.Status = TransactionStatus.InvalidTransactionNumber;
                return false;
            }

            //Check if it is a new command coming in from host
            var lastTransaction = _historyLogProvider.GetLastTransaction();
            if (lastTransaction.TransactionNumber != _currentTransferData.TransactionNumber
                || lastTransaction.Command != _currentTransferData.Command
                || lastTransaction.RequestedTransactionAmount != _currentTransferData.TransferAmount)
            {
                _currentResponse.Status = TransactionStatus.EgmBusy;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Checks if there are any issues and sets the status to that particular error.
        /// </summary>
        /// <returns>True if the transaction should continue.</returns>
        private bool CheckAndSetTransactionStatus()
        {
            _currentResponse.Status = TransactionStatus.OperationSuccessful;
            if (IsAnyDoorsOpen())
            {
                _currentResponse.Status = TransactionStatus.EgmDoorOpen;
                return false;
            }

            if (_gamePlayState.CurrentState != PlayState.Idle || _operatorMenu.IsShowing)
            {
                _currentResponse.Status = TransactionStatus.InGamePlayMode;
                return false;
            }

            var lockupOtherThanAllowedEft =
                _disableProvider.CurrentDisableKeys?.Except(EftCommonGuids.AllowEftGuids).ToList();
            if (lockupOtherThanAllowedEft?.Count > 0)
            {
                if (lockupOtherThanAllowedEft.Except(EftCommonGuids.DisabledByHostGuids).ToList().Count > 0)
                {
                    _currentResponse.Status = TransactionStatus.EgmInTiltCondition;
                    return false;
                }

                _currentResponse.Status = TransactionStatus.EgmDisabled;
                if (_eftTransferHandler.StopTransferIfDisabledByHost())
                {
                    return false;
                }
            }

            if (_disableByOperator.DisabledByOperator)
            {
                _currentResponse.Status = TransactionStatus.EgmOutOfService;
                return false;
            }

            return true;
        }

        private bool IsAnyDoorsOpen()
        {
            return _doorService.LogicalDoors.Any(pair => _doorService.GetDoorOpen(pair.Key));
        }

        private bool CheckTransferAmount()
        {
            var (possibleTransferAmount, exceeded) =
                _eftTransferHandler.CheckTransferAmount(_currentTransferData.TransferAmount);

            if (exceeded)
            {
                _currentResponse.Status = TransactionStatus.TransferAmountExceeded;
            }

            _currentResponse.TransferAmount = possibleTransferAmount;

            return possibleTransferAmount != 0;
        }

        private void StartTimerAndSetAckMessage()
        {
            _timer.Start();
            _currentResponse.Handlers =
                new HostAcknowledgementHandler
                {
                    ImpliedAckHandler = HandleImpliedAck,
                    ImpliedNackHandler = HandleImpliedNack,
                    IntermediateNackHandler = ReAckRequested
                };
        }

        private bool ExecuteCommand()
        {
            lock (_finalExecutionLock)
            {
                _logger.Info(
                    _eftTransferHandler.ProcessTransfer(
                        _currentResponse.TransferAmount,
                        _currentResponse.TransactionNumber)
                        ? "Transaction completed."
                        : "Transaction failed.");
                _historyLogProvider.UpdateLogEntryForRequestCompleted(
                    _currentTransferData.Command,
                    _currentResponse.TransactionNumber,
                    _currentTransferData.TransferAmount);
                return true;
            }
        }

        private void LockEgm()
        {
            _logger.Info("EGM disabled.");
            _disableProvider.Disable(
                SasConstants.EftTransactionLockUpGuid,
                SystemDisablePriority.Immediate,
                _eftTransferHandler.GetDisableString,
                false);
        }

        private void UnlockEgm()
        {
            _logger.Info("EGM enabled.");
            _disableProvider.Enable(SasConstants.EftTransactionLockUpGuid);
        }
    }

    /// <summary>
    ///     EFT states used for the <see cref="StateMachine{EftState,EftStateInput}" />.
    /// </summary>
    internal enum EftState
    {
        Idle,
        FirstPhase,
        SecondPhase
    }

    /// <summary>
    ///     EFT triggers used for the <see cref="StateMachine{EftState,EftStateInput}" />.
    /// </summary>
    internal enum EftStateTrigger
    {
        CommandReceived,
        TimerExpired,
        ImpliedNackReceived,
        ReAckRequested
    }
}