namespace Aristocrat.Monaco.Gaming.Lobby.Services.ResponsibleGaming;

using Aristocrat.Monaco.Kernel.Contracts.Events;
using System.Reflection.Metadata;
using Contracts;
using Kernel;
using Stateless;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

public class DefaultResponsibleGamingController : ResponsibleGamingController
{
    private readonly IEventBus _eventBus;
    private readonly IResponsibleGaming _responsibleGaming;

    public DefaultResponsibleGamingController(IEventBus eventBus, IResponsibleGaming responsibleGaming)
    {
        _eventBus = eventBus;
        _responsibleGaming = responsibleGaming;

        SubscribeToEvents();

        _responsibleGaming.OnStateChange += ResponsibleGamingStateChanged;

        // Propagate to lobby property for data binding.
        _responsibleGaming.PropertyChanged += ResponsibleGamingOnPropertyChanged;
        _responsibleGaming.ForceCashOut += OnForceCashOut;
        _responsibleGaming.OnForcePendingCheck += ForcePendingResponsibleGamingCheck;
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    private async Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        _responsibleGaming.Initialize();

        return Task.CompletedTask;
    }
}
