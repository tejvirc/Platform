namespace Aristocrat.Monaco.Gaming.Lobby.Lobby;

using System.Threading;
using System.Threading.Tasks;
using Kernel;
using Kernel.Contracts.Events;
using Microsoft.Extensions.Logging;
using Stateless;

public class LobbyState
{
    private readonly ILogger<LobbyState> _logger;
    private readonly IEventBus _eventBus;

    private State _state;
    private StateMachine<State, Trigger> _stateMachine;

    public LobbyState(ILogger<LobbyState> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;

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
