namespace Aristocrat.Monaco.Gaming.Lobby.Services.IdleText;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Contracts;
using Contracts.Lobby;
using Fluxor;
using Kernel;
using Monaco.UI.Common;
using Store;

public class IdleTextService
{
    private const double IdleTextTimerIntervalSeconds = 30.0;
    private const string IdleTextFamilyName = "Segoe UI";
    private const double MaximumBlinkingIdleTextWidth = 1000;

    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;

    private readonly ITimer _idleTextTimer;

    public IdleTextService(IDispatcher dispatcher, IEventBus eventBus, IPropertiesManager properties)
    {
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _properties = properties;

        _idleTextTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(IdleTextTimerIntervalSeconds) };
        _idleTextTimer.Tick += IdleTextTimerTick;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
    }

    private void Handle(PropertyChangedEvent evt)
    {
        if (evt.PropertyName == GamingConstants.IdleText)
        {
            UpdateIdleText();
        }
    }

    private void UpdateIdleText()
    {
        var text = _properties.GetValue<string?>(GamingConstants.IdleText, null);
        _dispatcher.Dispatch(new UpdateIdleTextAction { IdleText = text });

        var bannerDisplayMode = MeasureIdleText(text!).Width <= MaximumBlinkingIdleTextWidth
            ? BannerDisplayMode.Blinking
            : BannerDisplayMode.Scrolling;

        _dispatcher.Dispatch(new UpdateBannerDisplayModeAction { Mode = bannerDisplayMode });
    }

    private void IdleTextTimerTick(object? sender, EventArgs e)
    {
        _idleTextTimer.Stop();
    }

    private static Size MeasureIdleText(string idleText)
    {
        var formattedText = new FormattedText(
            idleText,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily(IdleTextFamilyName), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            32,
            Brushes.Black,
            new NumberSubstitution(),
            1);

        return new Size(formattedText.Width, formattedText.Height);
    }
}
