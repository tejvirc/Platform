namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Kernel;
using Store;

public class SystemEnabledConsumer : Consumes<SystemEnabledEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;
    private readonly ISystemDisableManager _disableManager;

    public SystemEnabledConsumer(ICommandHandlerFactory commandHandlers, ISystemDisableManager disableManager)
    {
        _commandHandlers = commandHandlers;
        _disableManager = disableManager;
    }

    public override Task ConsumeAsync(SystemEnabledEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<SystemEnabled>().Handle(new SystemEnabled(_disableManager.IsDisabled, _disableManager.DisableImmediately,
            _disableManager.CurrentDisableKeys, _disableManager.CurrentImmediateDisableKeys));

        return Task.CompletedTask;
    }
}
