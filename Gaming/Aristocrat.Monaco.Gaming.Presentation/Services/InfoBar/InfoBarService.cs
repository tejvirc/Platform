namespace Aristocrat.Monaco.Gaming.Presentation.Services.InfoBar;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Cabinet.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;
using Aristocrat.Monaco.Gaming.Presentation.Contracts.InfoBar;
using Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Store;

public class InfoBarService : IInfoBarService
{
    private readonly ILogger<InfoBarService> _logger;
    private readonly IState<InfoBarState> _infoBarState;
    private readonly IPropertiesManager _properties;
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    public DisplayRole DisplayTarget { get; set; }

    public InfoBarService(
        IEventBus eventBus,
        IDispatcher dispatcher,
        ILogger<InfoBarService> logger,
        IState<InfoBarState> infoBarState,
        IPropertiesManager properties)
    {
        _eventBus = eventBus;
        _dispatcher = dispatcher;
        _logger = logger;
        _infoBarState = infoBarState;
        _properties = properties;
    }

    public async Task ClearMessage(Guid ownerId, InfoBarRegion region)
    {
        await CancelAndClearTransientMessage(ownerId, region);

        //ClearMessage
        _infoBarState.Value.MessageDataSet.RemoveAll(m => m.OwnerId == ownerId && m.Region == region);
        if (_infoBarState.Value.MessageDataSet.IsEmpty)
        {
            _logger.Log(LogLevel.Debug, $"Removed message owner={ownerId} region={region}; total={_infoBarState.Value.MessageDataSet.Count}");
            UpdateDisplay(region);
        }
    }

    public async Task DisplayTransientMessage(
        Guid ownerId,
        string message,
        InfoBarRegion region,
        InfoBarColor textColor,
        InfoBarColor backgroundColor,
        TimeSpan duration,
        CancellationToken token = default)
    {
        await CancelAndClearTransientMessage(ownerId, region);

        var messageData = DisplayMessage(ownerId, message, region, textColor, backgroundColor, duration.TotalSeconds, token);

        if (duration.Seconds > 0) //Do this for Transient Message. Skip this for static message.
        {
            _logger.Log(LogLevel.Debug, $"Start transient task, duration={duration}");
            var cts = new CancellationTokenSource();

            // Create a composite token source that auto cancels if either of the 2 sources are canceled
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token))
            {
                var timeoutTask = Task.Delay(duration, linkedCts.Token);

                messageData.TransientMessageTask = timeoutTask;
                messageData.Cts = cts;

                // Asynchronously wait for timer timeout
                await timeoutTask;
                _logger.Log(LogLevel.Debug, "Transient task ended naturally");

                // Cleanup message and timeout state after timeout completes
                await CancelAndClearTransientMessage(ownerId, region);
            }
        }
    }

    private async Task CancelAndClearTransientMessage(Guid ownerId, InfoBarRegion region)
    {
        if (RegionHasTransientMessage(ownerId, region))
        {
            await CancelTransientMessage(ownerId, region);
            ClearTransientMessage(ownerId, region);
        }
    }

    private InfoBarMessageData DisplayMessage(Guid ownerId, string message, InfoBarRegion region, InfoBarColor textColor, InfoBarColor backgroundColor, double duration, CancellationToken token)
    {
        var messageData = new InfoBarMessageData
        {
            OwnerId = ownerId,
            Region = region,
            TextColor = textColor,
            BackgroundColor = backgroundColor,
            Text = message,
            Duration = duration
        };

        _infoBarState.Value.MessageDataSet.Add(messageData);

        UpdateDisplay(region);
        //Logger.Debug($"Added message owner={ownerId} region={region} message={message}; total={_infoBarState.Value.MessageDataSet.Count}");

        return messageData;
    }

    private bool RegionHasTransientMessage(Guid ownerId, InfoBarRegion region)
    {
        return _infoBarState.Value.MessageDataSet.Any(
            m => m.OwnerId == ownerId && m.Region == region && m.TransientMessageTask != null);

    }

    private async Task CancelTransientMessage(Guid ownerId, InfoBarRegion region)
    {
        _logger.Log(LogLevel.Debug, $"Try cancel message owner={ownerId} region={region}");
        var foundMessage = GetOwnedRegionTransientMessage(ownerId, region);
        if (foundMessage == null)
        {
            _logger.Log(LogLevel.Debug, $"Transient message owner={ownerId} region={region} not found");
            return;
        }

        try
        {
            foundMessage.Cts.Cancel();
            await foundMessage.TransientMessageTask;
        }
        catch (OperationCanceledException)
        {
            _logger.Log(LogLevel.Debug, $"Transient message owner={ownerId} region={region}, was canceled");
        }
    }

    private InfoBarMessageData GetOwnedRegionTransientMessage(Guid ownerId, InfoBarRegion region)
    {
        if (RegionHasTransientMessage(ownerId, region))
        {
            return _infoBarState.Value.MessageDataSet.First(m => m.OwnerId == ownerId && m.Region == region && m.TransientMessageTask != null);
        }

        _logger.Log(LogLevel.Debug, $"Looking for transient message owner={ownerId} region={region}, not found");
        return null;
    }

    private void ClearTransientMessage(Guid ownerId, InfoBarRegion region)
    {
        var firstTransientMessage = _infoBarState.Value.MessageDataSet.First(m => m.OwnerId == ownerId && m.Region == region && m.TransientMessageTask != null);
        if (firstTransientMessage == null)
        {
            return;
        }

        firstTransientMessage.Cts.Dispose();
        firstTransientMessage.Cts = null;
        firstTransientMessage.TransientMessageTask = null;
        _logger.Log(LogLevel.Debug, $"Cleared transient message owner={ownerId} region={region}");
    }

    private void UpdateDisplay(InfoBarRegion region)
    {
        string? text;
        InfoBarColor color;
        double duration;

        switch (region)
        {
            // For each region, concatenate its text messages, and use the last one's text color and duration.
            case InfoBarRegion.Left:
                var leftMessages = _infoBarState.Value.MessageDataSet.Where(m => m.Region == InfoBarRegion.Left).ToList();
                text = string.Join(" | ", leftMessages.Select(m => m.Text));
                color = leftMessages.Any() ? leftMessages.Last().TextColor : InfoBarColor.Black;
                duration = leftMessages.Any() ? leftMessages.Last().Duration : 0;
                _dispatcher.Dispatch(new InfoBarLeftRegionUpdateAction(text, duration, color));
                _logger.Log(LogLevel.Debug, $"left text={text} color={color} dur={color}");
                break;

            case InfoBarRegion.Center:
                var centerMessages = _infoBarState.Value.MessageDataSet.Where(m => m.Region == InfoBarRegion.Center).ToList();
                text = string.Join(" | ", centerMessages.Select(m => m.Text));
                color = centerMessages.Any() ? centerMessages.Last().TextColor : InfoBarColor.Black;
                duration = centerMessages.Any() ? centerMessages.Last().Duration : 0;
                _dispatcher.Dispatch(new InfoBarCenterRegionUpdateAction(text, duration, color));
                _logger.Log(LogLevel.Debug, $"center text={text} color={color} dur={duration}");
                break;

            case InfoBarRegion.Right:
                var rightMessages = _infoBarState.Value.MessageDataSet.Where(m => m.Region == InfoBarRegion.Right).ToList();
                text = string.Join(" | ", rightMessages.Select(m => m.Text));
                color = rightMessages.Any() ? rightMessages.Last().TextColor : InfoBarColor.Black;
                duration = rightMessages.Any() ? rightMessages.Last().Duration : 0;
                _dispatcher.Dispatch(new InfoBarRightRegionUpdateAction(text, duration, color));
                _logger.Log(LogLevel.Debug, $"right text={text} color={color} dur={duration}");
                break;
        }

        // Set background to the most recent definition.
        InfoBarColor BackgroundColor = _infoBarState.Value.MessageDataSet.Count > 0 ? _infoBarState.Value.MessageDataSet.Last().BackgroundColor : InfoBarColor.Black;
        _logger.Log(LogLevel.Debug, $"background color={BackgroundColor}");
    }
}
