namespace Aristocrat.Monaco.Gaming.Presentation.Services.InfoBar;

using System;
using System.Linq;
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

    public async void ClearMessage(Guid ownerId, InfoBarRegion region)
    {
        if (RegionHasTransientMessage(ownerId, region))
        {
            await CancelTransientMessage(ownerId, region);
            ClearTransientMessageTimeout(ownerId, region);
        }

        //ClearMessage
        _infoBarState.Value.MessageDataSet.RemoveAll(m => m.OwnerId == ownerId && m.Region == region);
        if (_infoBarState.Value.MessageDataSet.IsEmpty)
        {
            _logger.Log(LogLevel.Debug, $"Removed message owner={ownerId} region={region}; total={_infoBarState.Value.MessageDataSet.Count}");
            UpdateDisplay(region);
        }
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

    private void ClearTransientMessageTimeout(Guid ownerId, InfoBarRegion region)
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
