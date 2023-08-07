namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Base;
    using Contracts.Client;
    using log4net;
    using Stateless;

    /// <summary>
    ///     Handles the host validation
    /// </summary>
    public class HostValidationProvider : IHostValidationProvider, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly ISasHost _sasHost;
        private const double ValidationPendingExceptionTime = 800.0;
        private const int ValidationTimeOut = 10;

        private readonly SasExceptionTimer _sasExceptionTimer;
        private readonly StateMachine<ValidationState, ValidationTrigger> _validationStateMachine;
        private HostValidationData _hostValidationData;
        private TaskCompletionSource<HostValidationResults> _validationTask;
        private bool _disposed;

        private enum ValidationTrigger
        {
            CashoutInformationAvailable,
            CashoutInformationRead,
            ValidationTimedOut,
            ValidationHostOffline,
            ValidationNumberReceived
        }

        /// <summary>
        ///     Creates a HostValidationProvider Instance
        /// </summary>
        public HostValidationProvider(ISasExceptionHandler exceptionHandler, ISasHost sasHost)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _sasExceptionTimer = new SasExceptionTimer(
                _exceptionHandler,
                ValidationException,
                PendingValidationRequest,
                ValidationPendingExceptionTime);

            _validationStateMachine = CreateStateMachine();
        }

        /// <inheritdoc />
        public ValidationState CurrentState => _validationStateMachine.State;

        /// <inheritdoc />
        public Task<HostValidationResults> GetValidationResults(ulong amount, TicketType ticketType)//Called by Handler
        {
            if (!_validationStateMachine.CanFire(ValidationTrigger.CashoutInformationAvailable))
            {
                return Task.FromResult((HostValidationResults)null);
            }

            _hostValidationData = new HostValidationData(amount, ticketType);
            _validationStateMachine.Fire(ValidationTrigger.CashoutInformationAvailable);
            return WaitForValidationInformation();
        }

        /// <inheritdoc />
        public bool SetHostValidationResult(HostValidationResults validationResults)//Called by Handler LP 58
        {
            if (!_validationStateMachine.CanFire(ValidationTrigger.ValidationNumberReceived))
            {
                return false;
            }

            var result = _validationTask?.TrySetResult(validationResults) ?? false;
            _validationStateMachine.Fire(ValidationTrigger.ValidationNumberReceived);
            return result;
        }

        /// <inheritdoc />
        public HostValidationData GetPendingValidationData()//Called by Handler for LP 57
        {
            if (!_validationStateMachine.CanFire(ValidationTrigger.CashoutInformationRead))
            {
                return null;
            }

            _validationStateMachine.Fire(ValidationTrigger.CashoutInformationRead);
            return _hostValidationData;
        }

        /// <inheritdoc />
        public void OnSystemDisabled()
        {
            if (_validationStateMachine.CanFire(ValidationTrigger.ValidationHostOffline) &&
                !_sasHost.IsHostOnline(SasGroup.Validation))
            {
                // Fail the transfer as the host is offline and we can't get validation data
                _validationTask?.TrySetResult(null);
                _validationStateMachine.Fire(ValidationTrigger.ValidationHostOffline);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     The dispose handing
        /// </summary>
        /// <param name="disposing">Whether or not to dispose it resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _validationTask?.TrySetCanceled();
                _sasExceptionTimer.Dispose();
            }

            _disposed = true;
        }

        private async Task<HostValidationResults> WaitForValidationInformation()
        {
            using (var cancellation = new CancellationTokenSource())
            {
                if (_validationTask.Task == await Task.WhenAny(
                        _validationTask.Task,
                        Task.Delay(TimeSpan.FromSeconds(ValidationTimeOut), cancellation.Token)))
                {
                    cancellation.Cancel();
                    return await _validationTask.Task;
                }

                _validationTask.TrySetCanceled();
                await _validationStateMachine.FireAsync(ValidationTrigger.ValidationTimedOut);
                return null;
            }
        }

        private StateMachine<ValidationState, ValidationTrigger> CreateStateMachine()
        {
            var validationStateMachine =
                new StateMachine<ValidationState, ValidationTrigger>(ValidationState.NoValidationPending);
            validationStateMachine.Configure(ValidationState.NoValidationPending)
                .PermitIf(
                    ValidationTrigger.CashoutInformationAvailable,
                    ValidationState.CashoutInformationPending,
                    () => !PendingValidationRequest());
            validationStateMachine.Configure(ValidationState.CashoutInformationPending)
                .Permit(ValidationTrigger.ValidationTimedOut, ValidationState.NoValidationPending)
                .Permit(ValidationTrigger.ValidationHostOffline, ValidationState.NoValidationPending)
                .Permit(ValidationTrigger.CashoutInformationRead, ValidationState.ValidationNumberPending)
                .Permit(ValidationTrigger.ValidationNumberReceived, ValidationState.NoValidationPending)
                .OnEntry(
                    () =>
                    {
                        _validationTask = new TaskCompletionSource<HostValidationResults>(TaskCreationOptions.RunContinuationsAsynchronously);
                        _sasExceptionTimer.StartTimer();
                    })
                .OnExit(() =>
                {
                    _sasExceptionTimer.StopTimer();
                    _exceptionHandler.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.SystemValidationRequest));
                });
            validationStateMachine.Configure(ValidationState.ValidationNumberPending)
                .PermitReentry(ValidationTrigger.CashoutInformationRead)
                .Permit(ValidationTrigger.ValidationNumberReceived, ValidationState.NoValidationPending)
                .Permit(ValidationTrigger.ValidationHostOffline, ValidationState.NoValidationPending)
                .Permit(ValidationTrigger.ValidationTimedOut, ValidationState.NoValidationPending);

            validationStateMachine.OnTransitioned(
                transition => Logger.Debug(
                    $"Transitioned From: {transition.Source} To: {transition.Destination} Trigger: {transition.Trigger}"));
            validationStateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                    Logger.Error($"Invalid Host Validation Transition. State: {state} Trigger: {trigger}"));
            return validationStateMachine;
        }

        private GeneralExceptionCode? ValidationException() => PendingValidationRequest()
            ? GeneralExceptionCode.SystemValidationRequest
            : (GeneralExceptionCode?)null;

        private bool PendingValidationRequest() => !(_validationTask?.Task?.IsCompleted ?? true);
    }
}