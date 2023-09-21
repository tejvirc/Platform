namespace Aristocrat.Monaco.Gaming.Presentation.Services.IdleText;

using System;
using Application.Contracts.Localization;
using Fluxor;
using Gaming.Contracts;
using Kernel;
using Localization.Properties;
using Store;

public class IdleTextService : IIdleTextService, IDisposable
{
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private bool _disposed;
    private string? _defaultIdleText;
    private string? _cabinetIdleText;
    private string? _jurisdictionIdleText;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="eventBus"></param>
    /// <param name="properties"></param>
    public IdleTextService(IDispatcher dispatcher, IEventBus eventBus, IPropertiesManager properties)
    {
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _properties = properties;

        SubscribeToEvents();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void InitializeDefaults()
    {
        var cabinetText = _properties.GetValue<string?>(GamingConstants.IdleText, null);
        var defaultText = Localizer.For(CultureFor.Player).GetString(ResourceKeys.IdleTextDefault);
        _defaultIdleText = string.IsNullOrEmpty(defaultText) ? null : defaultText;
        _cabinetIdleText = string.IsNullOrEmpty(cabinetText) ? null : cabinetText;
    }

    /// <inheritdoc />
    public string? GetCurrentIdleText()
    {
        return _cabinetIdleText ?? _jurisdictionIdleText ?? _defaultIdleText;
    }

    /// <inheritdoc />
    public void SetCabinetIdleText(string? text)
    {
        _cabinetIdleText = string.IsNullOrEmpty(text) ? null : text;
        PublishUpdatedIdleText();
    }

    /// <inheritdoc />
    public void SetJurisdictionIdleText(string? text)
    {
        _jurisdictionIdleText = string.IsNullOrEmpty(text) ? null : text;
        ;
        PublishUpdatedIdleText();
    }

    /// <inheritdoc />
    public void SetDefaultIdleText(string? text)
    {
        _defaultIdleText = string.IsNullOrEmpty(text) ? null : text;
        ;
        PublishUpdatedIdleText();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _eventBus.UnsubscribeAll(this);
        }

        _disposed = true;
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
    }

    private void Handle(PropertyChangedEvent evt)
    {
        if (evt.PropertyName == GamingConstants.IdleText)
        {
            var text = _properties.GetValue<string?>(GamingConstants.IdleText, null);
            SetCabinetIdleText(text);
        }
    }

    private void PublishUpdatedIdleText()
    {
        _dispatcher.Dispatch(new BannerUpdateIdleTextAction(GetCurrentIdleText()));
    }
}