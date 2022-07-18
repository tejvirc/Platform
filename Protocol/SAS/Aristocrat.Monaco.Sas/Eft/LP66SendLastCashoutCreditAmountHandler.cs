namespace Aristocrat.Monaco.Sas.Eft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Sas.Client.EFT.Response;
    using log4net;
    using Stateless;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     (From section 8.9 EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     Handles LP66, send last cashout credit amount to host. Most of the workflow logic is handled in
    /// </summary>
    public class LP66SendLastCashoutCreditAmountHandler : ISasLongPollHandler<EftSendLastCashoutResponse, EftSendLastCashoutData>
    {
        private const int EftCommandTimeoutMs = 800;

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISystemDisableManager _systemDisableProvider;
        private readonly IGamePlayState _gamePlayState;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IDisableByOperatorManager _disableByOperator;
        private readonly IDoorService _doorService;

        private readonly IPlayerInitiatedCashoutProvider _playerCashoutProvider;

        private readonly EftSendLastCashoutData _requestCommandData = new();

        private readonly EftSendLastCashoutResponse _lastCashoutResponse = new();

        //FiringMode is hardcoded as FiringMode.Queued, please DO NOT change it as _stateMachine
        //relies on this to behave correctly in the racing conditions.
        private readonly StateMachine<LP66HandlerState, LP66HandlerStateTrigger> _stateMachine = new(LP66HandlerState.Idle, FiringMode.Queued);
        private ISystemTimerWrapper _timer;

        /// <summary>
        ///     Constructor of LP66SendLastCashoutCreditAmountHandler
        /// </summary>
        /// <param name="doorService"></param>  
        /// <param name="playerCashoutProvider"></param>        
        /// <param name="systemDisableManager"></param>
        /// <param name="gamePlayState"></param>
        /// <param name="operatorMenu"></param>
        /// <param name="disableByOperator"></param>
        public LP66SendLastCashoutCreditAmountHandler(
            IDoorService doorService,
            IPlayerInitiatedCashoutProvider playerCashoutProvider,
            ISystemDisableManager systemDisableManager,
            IGamePlayState gamePlayState,
            IOperatorMenuLauncher operatorMenu,
            IDisableByOperatorManager disableByOperator)
        {
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));
            _playerCashoutProvider = playerCashoutProvider ?? throw new ArgumentNullException(nameof(playerCashoutProvider));
            _systemDisableProvider = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _disableByOperator = disableByOperator ?? throw new ArgumentNullException(nameof(disableByOperator));

            SetupTimer();
            ConfigureStateMachine();
        }

        /// <inheritdoc cref="ISasLongPollHandler" />
        public List<LongPoll> Commands { get; } = new() { LongPoll.EftSendLastCashOutCreditAmount };

        /// <inheritdoc />
        public EftSendLastCashoutResponse Handle(EftSendLastCashoutData data)
        {
            Clear();
            _requestCommandData.Acknowledgement = data.Acknowledgement;
            _stateMachine.Fire(LP66HandlerStateTrigger.CommandReceived); //will change status, amount and handler
            var theFinalResponse = GetResponse();
            return theFinalResponse;
        }

        /// <summary>
        ///     Copy the response to a new object
        /// </summary>
        /// <returns>EftSendLastCashoutResponse</returns>
        private EftSendLastCashoutResponse GetResponse()
        {
            return new EftSendLastCashoutResponse
            {
                Acknowledgement = _requestCommandData.Acknowledgement,
                LastCashoutAmount = _lastCashoutResponse.LastCashoutAmount,
                Handlers = _lastCashoutResponse.Handlers,
                Status = _lastCashoutResponse.Status
            };
        }

        /// <summary>
        ///     Clear the data after processing one command
        /// </summary>
        private void Clear()
        {
            _lastCashoutResponse.Acknowledgement = false;
            _lastCashoutResponse.LastCashoutAmount = 0; //reset to 0
            _lastCashoutResponse.Handlers = null;
            _lastCashoutResponse.Status = TransactionStatus.OperationSuccessful; //default value
            _requestCommandData.Acknowledgement = false;
        }

        /// <summary>
        ///     Sets the timerWrapper as well as includes a stub method for unit testing purposes.
        ///     Please refer to EftStateController that has same appraoch
        /// </summary>
        /// <param name="timerWrapper">Timer to be mocked in unit testing.</param>
        public void SetupTimer(ISystemTimerWrapper timerWrapper = null)
        {
            if (_timer != null)
            {
                _timer.Elapsed -= (_, _) => _stateMachine.Fire(LP66HandlerStateTrigger.TimerExpired);
            }

            _timer = timerWrapper ?? new SystemTimerWrapper { Interval = EftCommandTimeoutMs, AutoReset = false };
            _timer.Elapsed += (_, _) => _stateMachine.Fire(LP66HandlerStateTrigger.TimerExpired);
        }

        /// <summary>
        ///     Configure the state machine
        /// </summary>
        private void ConfigureStateMachine()
        {
            _stateMachine.OnUnhandledTrigger(
                (state, trigger) => _logger.Warn($"Unregistered transition attempted from state {state} using trigger {trigger}"));

            _stateMachine.OnTransitioned(
                transition => _logger.Info($"{transition.Source} state Transitioned into {transition.Destination} using trigger {transition.Trigger}"));

            _stateMachine.Configure(LP66HandlerState.Idle)
                .PermitDynamic(LP66HandlerStateTrigger.CommandReceived, ProcessCommandWhenIdle);

            _stateMachine.Configure(LP66HandlerState.WaitingForHostAck)
                .OnEntry(() => { _timer.Start(); })     //only reason to use anonymous method is for unit test (SetupTimer method)
                .Permit(LP66HandlerStateTrigger.TimerExpired, LP66HandlerState.Idle)
                .PermitDynamic(LP66HandlerStateTrigger.CommandReceived, ProcessCommandWhenWaitingForHostAck)
                .OnExit(() => { _timer.Stop(); });       //only reason to use anonymous method is for unit test (SetupTimer method)

            _stateMachine.Configure(LP66HandlerState.WaitingForImpliedAck)
                .OnEntry(() => { _timer.Start(); })      //only reason to use anonymous method is for unit test (SetupTimer method)
                .Permit(LP66HandlerStateTrigger.ImpliedNackInvoked, LP66HandlerState.Idle)
                .InternalTransition(LP66HandlerStateTrigger.ReAckRequested, () => { _timer.Stop(); _timer.Start(); })
                .PermitDynamic(LP66HandlerStateTrigger.CommandReceived, ProcessCommandWhenWaitingForImpliedAck)
                .PermitDynamic(LP66HandlerStateTrigger.ImpliedAckInvoked, ClearLastCashoutAmount)
                .PermitDynamic(LP66HandlerStateTrigger.TimerExpired, ClearLastCashoutAmount)
                .OnExit(() => { _timer.Stop(); });       //only reason to use anonymous method is for unit test (SetupTimer method)
        }

        /// <summary>
        ///     Process the command while Idle state
        /// </summary>
        /// <returns>LP66HandlerState</returns>
        private LP66HandlerState ProcessCommandWhenIdle()
        {
            //set the amount regardless
            _lastCashoutResponse.LastCashoutAmount = _playerCashoutProvider.GetCashoutAmount();

            if (_requestCommandData.Acknowledgement)       //verify Ack
            {
                _lastCashoutResponse.Status = TransactionStatus.InvalidAck;
                return LP66HandlerState.Idle;   //Expect Ack = 0, however received Ack = 1, so return InvalidAck status, and back to Idle status
            }

            _lastCashoutResponse.Status = GetStatus();  //set the status
            return LP66HandlerState.WaitingForHostAck;
        }

        /// <summary>
        ///     Process the command when waiting for host ack
        /// </summary>
        /// <returns>LP66HandlerState</returns>
        private LP66HandlerState ProcessCommandWhenWaitingForHostAck()
        {
            //set the amount regardless
            _lastCashoutResponse.LastCashoutAmount = _playerCashoutProvider.GetCashoutAmount();

            if (!_requestCommandData.Acknowledgement)   //verify Ack
            {
                _lastCashoutResponse.Status = TransactionStatus.InvalidAck;
                return LP66HandlerState.Idle;  //Expect Ack = 1, however received Ack = 0, so InvalidAck status, and back to Idle status
            }

            _lastCashoutResponse.Status = GetStatus();  //set the status
            _lastCashoutResponse.Handlers = new HostAcknowledgementHandler
            {
                ImpliedAckHandler = () => _stateMachine.Fire(LP66HandlerStateTrigger.ImpliedAckInvoked),
                ImpliedNackHandler = () => _stateMachine.Fire(LP66HandlerStateTrigger.ImpliedNackInvoked),
                IntermediateNackHandler = () => _stateMachine.Fire(LP66HandlerStateTrigger.ReAckRequested)
            };

            return LP66HandlerState.WaitingForImpliedAck;   //go to next state
        }

        /// <summary>
        ///     Process the command when waiting for implied ack
        /// </summary>
        /// <returns>LP66HandlerState</returns>
        private LP66HandlerState ProcessCommandWhenWaitingForImpliedAck()
        {
            //set the amount regardless
            _lastCashoutResponse.LastCashoutAmount = _playerCashoutProvider.GetCashoutAmount();

            if (!_requestCommandData.Acknowledgement)   //verify Ack
            {
                _lastCashoutResponse.Status = TransactionStatus.InvalidAck;
                return LP66HandlerState.Idle;           //Expect Ack = 1, however received Ack = 0, so InvalidAck status, and back to Idle status
            }

            //resend the status and amount
            _lastCashoutResponse.Status = GetStatus();  //set the status
            _lastCashoutResponse.Handlers = new HostAcknowledgementHandler
            {
                ImpliedAckHandler = () => _stateMachine.Fire(LP66HandlerStateTrigger.ImpliedAckInvoked),
                ImpliedNackHandler = () => _stateMachine.Fire(LP66HandlerStateTrigger.ImpliedNackInvoked),
                IntermediateNackHandler = () => _stateMachine.Fire(LP66HandlerStateTrigger.ReAckRequested)
            };

            return LP66HandlerState.WaitingForImpliedAck;   //go to next state
        }

        /// <summary>
        /// this method will be invoked when the implied ack action occurs
        /// OR the timer is elapsed when the state is LP66HandlerState.WaitingForImpliedAck
        /// </summary>
        /// <returns></returns>
        private LP66HandlerState ClearLastCashoutAmount()
        {
            //Clear the cashout amount, there is lock to prevent concurrent issue in PlayerCashoutProvider
            _logger.Info("Clear Player Last Cashout Amount from storage.");
            _playerCashoutProvider.ClearCashoutAmount();
            return LP66HandlerState.Idle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private TransactionStatus GetStatus()
        {
            if (_doorService.LogicalDoors.Any(pair => _doorService.GetDoorOpen(pair.Key)))
            {
                return TransactionStatus.OperationSuccessful;
            }

            var lockupOtherThanAllowedEft = _systemDisableProvider.CurrentDisableKeys?.Except(EftCommonGuids.AllowEftGuids).ToList();
            if (lockupOtherThanAllowedEft?.Count > 0)
            {
                if (lockupOtherThanAllowedEft.Except(EftCommonGuids.DisabledByHostGuids).ToList().Count > 0)
                {
                    return TransactionStatus.EgmInTiltCondition;
                }

                return TransactionStatus.EgmDisabled;
            }

            if (_gamePlayState.CurrentState != PlayState.Idle || _operatorMenu.IsShowing)
            {
                return TransactionStatus.InGamePlayMode;
            }

            if (_disableByOperator.DisabledByOperator)
            {
                return TransactionStatus.EgmOutOfService;
            }

            return TransactionStatus.OperationSuccessful;
        }

        /// <summary>
        ///     LP66 handle state
        /// </summary>
        private enum LP66HandlerState
        {
            Idle,
            WaitingForHostAck,
            WaitingForImpliedAck
        }

        /// <summary>
        ///     Eft LP66Handler triggers used for the <see cref="StateMachine{LP66HandlerState,LP66HandlerStateTrigger}" />.
        /// </summary>
        private enum LP66HandlerStateTrigger
        {
            CommandReceived,        //Respond the command, and waiting for the host to ack
            ReAckRequested,
            TimerExpired,
            ImpliedAckInvoked,
            ImpliedNackInvoked,
        }
    }
}