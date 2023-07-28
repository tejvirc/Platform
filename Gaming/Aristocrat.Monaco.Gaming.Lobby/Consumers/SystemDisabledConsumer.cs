namespace Aristocrat.Monaco.Gaming.Lobby.Consumers;

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using CommandHandlers;
using Kernel;
using Store;

public class SystemDisabledConsumer : Consumes<SystemDisabledEvent>
{
    private readonly ICommandHandlerFactory _commandHandlers;
    private readonly ISystemDisableManager _disableManager;

    public SystemDisabledConsumer(ICommandHandlerFactory commandHandlers, ISystemDisableManager disableManager)
    {
        _commandHandlers = commandHandlers;
        _disableManager = disableManager;
    }

    public override Task ConsumeAsync(SystemDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        _commandHandlers.Create<SystemDisabled>().Handle(new SystemDisabled(theEvent.Priority, _disableManager.IsDisabled,
            _disableManager.DisableImmediately, _disableManager.CurrentDisableKeys,
            _disableManager.CurrentImmediateDisableKeys));

        return Task.CompletedTask;
    }
}
