namespace Aristocrat.Monaco.Gaming.Presentation.Services.Replay;

using System;
using System.Threading.Tasks;
using Gaming.Contracts;
using Gaming.Commands;
using Hardware.Contracts.Button;
using Kernel;
using Microsoft.Extensions.Logging;

public sealed class ReplayService : IReplayService, IDisposable
{
    private readonly ILogger<ReplayService> _logger;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly ICommandHandlerFactory _commandHandlerFactory;
    private readonly IGameDiagnostics _gameDiagnostics;

    public ReplayService(
        ILogger<ReplayService> logger,
        IEventBus eventBus,
        IPropertiesManager properties,
        ICommandHandlerFactory commandHandlerFactory,
        IGameDiagnostics gameDiagnostics)
    {
        _logger = logger;
        _eventBus = eventBus;
        _properties = properties;
        _commandHandlerFactory = commandHandlerFactory;
        _gameDiagnostics = gameDiagnostics;
    }

    public void Dispose()
    {
        _eventBus.UnsubscribeAll(this);
    }

    public Task ContinueAsync()
    {
        _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Play));

        return Task.CompletedTask;
    }

    public Task<bool> GetReplayPauseActiveAsync() =>
        Task.FromResult(_properties.GetValue(GamingConstants.ReplayPauseActive, true));

    public Task NotifyCompletedAsync(long endCredits)
    {
        _commandHandlerFactory.Create<ReplayGameEnded>().Handle(new ReplayGameEnded(endCredits));

        return Task.CompletedTask;
    }

    public Task EndReplayAsync()
    {
        _gameDiagnostics.End();

        return Task.CompletedTask;
    }
}
