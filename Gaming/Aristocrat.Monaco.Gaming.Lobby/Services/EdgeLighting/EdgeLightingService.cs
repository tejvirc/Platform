namespace Aristocrat.Monaco.Gaming.Lobby.Services.EdgeLighting;

using System;
using Application.Contracts;
using Application.Contracts.EdgeLight;
using Application.Contracts.Localization;
using Fluxor;
using Hardware.Contracts.EdgeLighting;
using Kernel;
using Localization.Properties;
using Store;

public sealed class EdgeLightingService : IEdgeLightingService, IDisposable
{
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly ILocalizerFactory _localizer;
    private readonly IEdgeLightingStateManager _edgeLightingStateManager;

    private IEdgeLightToken? _edgeLightStateToken;

    public EdgeLightingService(
        IDispatcher dispatcher,
        IEventBus eventBus,
        IPropertiesManager properties,
        ILocalizerFactory localizer,
        IEdgeLightingStateManager edgeLightingStateManager)
    {
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _properties = properties;
        _localizer = localizer;
        _edgeLightingStateManager = edgeLightingStateManager;

        SubscribeToEvents();
    }

    public void SetEdgeLighting(EdgeLightState? newState)
    {
        _edgeLightingStateManager.ClearState(_edgeLightStateToken);

        if (newState.HasValue)
        {
            _edgeLightStateToken = _edgeLightingStateManager.SetState(newState.Value);
        }
    }

    public void Dispose()
    {
        _eventBus.UnsubscribeAll(this);
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
    }

    private void Handle(PropertyChangedEvent evt)
    {
        switch (evt.PropertyName)
        {
            case ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey:
                SetOverrideEdgeLight();
                break;
        }
    }

    private void SetOverrideEdgeLight()
    {
        var transparentColorName = _localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent);

        var edgeLightingAttractModeOverrideSelection = _properties.GetValue(
            ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
            transparentColorName);

        if (edgeLightingAttractModeOverrideSelection != transparentColorName)
        {
            _dispatcher.Dispatch(new UpdateOverrideEdgeLight { CanOverrideEdgeLight  = true });
        }
        else
        {
            _dispatcher.Dispatch(new UpdateOverrideEdgeLight { CanOverrideEdgeLight = false });
        }
    }
}
