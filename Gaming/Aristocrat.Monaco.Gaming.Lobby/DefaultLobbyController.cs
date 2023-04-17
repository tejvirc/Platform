namespace Aristocrat.Monaco.Gaming.Lobby;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Kernel.Contracts.Events;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Stateless;
using Store;

public class DefaultLobbyController : LobbyController
{
    private readonly ILogger<DefaultLobbyController> _logger;
    private readonly IEventBus _eventBus;
    private readonly IDispatcher _dispatcher;

    private State _state;
    private StateMachine<State, Trigger> _stateMachine;

    public DefaultLobbyController(ILogger<DefaultLobbyController> logger, IEventBus eventBus, IDispatcher dispatcher)
    {
        _logger = logger;
        _eventBus = eventBus;
        _dispatcher = dispatcher;

        CreateStateMachine(State.Startup);

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    private void CreateStateMachine(State initialState)
    {
        _state = initialState;

        _stateMachine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

        _stateMachine.Configure(State.Startup)
            .OnEntry(() => { });

        _stateMachine.Configure(State.Initialize)
            .OnEntryAsync(async () => await OnInitialize())
            .Permit(Trigger.Started, State.Startup);

        _stateMachine.OnUnhandledTrigger(
            (state, trigger) =>
            {
                _logger.LogError(
                    "Invalid State Transition. State : {State} Trigger : {Trigger}",
                    state,
                    trigger);
            });

        _stateMachine.OnTransitioned(
            transition =>
            {
                _logger.LogDebug(
                    "Transitioned From : {Source} To : {Destination} Trigger : {Trigger}",
                    transition.Source,
                    transition.Destination,
                    transition.Trigger);
            });
    }

    private Task OnInitialize()
    {
        _dispatcher.Dispatch(new InitializeAction());

        return Task.CompletedTask;
    }

    private async Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        await _stateMachine.FireAsync(Trigger.Started);
    }

    private enum State
    {
        Startup,

        Initialize
    }

    private enum Trigger
    {
        Started
    }
}
