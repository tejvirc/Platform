namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Cabinet.Contracts;
    using Contracts.InfoBar;
    using log4net;
    using Kernel;

    public class InfoBarViewModel : BaseObservableObject, IDisposable
    {
        private class InfoBarMessageData
        {
            public Guid OwnerId;
            public InfoBarRegion Region;
            public string Text;
            public InfoBarColor TextColor;
            public InfoBarColor BackgroundColor;
            public CancellationTokenSource Cts;
            public Task TransientMessageTask;
            public double Duration;
        }

        private const double FontSizeMinimum = 16;
        private const double MarginSide = 4;
        private const double MarginTop = 6;

        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isOpen;
        private bool _disposed;
        private IEventBus _eventBus;

        private Thickness _margin;
        private double _barHeight;
        private double _fontSize = FontSizeMinimum;
        private string _leftRegionText;
        private string _centerRegionText;
        private string _rightRegionText;
        private double _leftRegionDuration;
        private double _centerRegionDuration;
        private double _rightRegionDuration;
        private InfoBarColor _backgroundColor;
        private InfoBarColor _leftRegionTextColor;
        private InfoBarColor _centerRegionTextColor;
        private InfoBarColor _rightRegionTextColor;
        private List<InfoBarMessageData> _messageDataSet = new List<InfoBarMessageData>();

        public const double BarHeightFraction = 0.035;
        public const double BarHeightMinimum = 30;

        public bool IsInfoBarEmpty => new[] { _leftRegionText, _centerRegionText, _rightRegionText }.All(string.IsNullOrEmpty);

        public DisplayRole DisplayTarget { get; set; }

        public InfoBarViewModel()
            : this(
                ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public InfoBarViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<InfoBarDisplayStaticMessageEvent>(this, Handler, e => e.DisplayTarget == DisplayTarget);
            _eventBus.Subscribe<InfoBarDisplayTransientMessageEvent>(this, Handler, e => e.DisplayTarget == DisplayTarget);
            _eventBus.Subscribe<InfoBarCloseEvent>(this, Handler, e => e.DisplayTarget == DisplayTarget);
            _eventBus.Subscribe<InfoBarClearMessageEvent>(this, Handler, e => e.DisplayTarget == DisplayTarget);
            _eventBus.Subscribe<InfoBarSetHeightEvent>(this, Handler, e => e.DisplayTarget == DisplayTarget);
        }

        public double BarHeight
        {
            get => _barHeight;
            set => SetProperty(ref _barHeight, value);
        }

        public Thickness RegionMargin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        public string LeftRegionText
        {
            get => _leftRegionText;
            set => SetProperty(ref _leftRegionText, value);
        }

        public string CenterRegionText
        {
            get => _centerRegionText;
            set => SetProperty(ref _centerRegionText, value);
        }

        public string RightRegionText
        {
            get => _rightRegionText;
            set => SetProperty(ref _rightRegionText, value);
        }

        public double LeftRegionDuration
        {
            get => _leftRegionDuration;
            set => SetProperty(ref _leftRegionDuration, value);
        }

        public double CenterRegionDuration
        {
            get => _centerRegionDuration;
            set => SetProperty(ref _centerRegionDuration, value);
        }

        public double RightRegionDuration
        {
            get => _rightRegionDuration;
            set => SetProperty(ref _rightRegionDuration, value);
        }

        public InfoBarColor BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public InfoBarColor LeftRegionTextColor
        {
            get => _leftRegionTextColor;
            set => SetProperty(ref _leftRegionTextColor, value);
        }

        public InfoBarColor CenterRegionTextColor
        {
            get => _centerRegionTextColor;
            set => SetProperty(ref _centerRegionTextColor, value);
        }

        public InfoBarColor RightRegionTextColor
        {
            get => _rightRegionTextColor;
            set => SetProperty(ref _rightRegionTextColor, value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

                foreach (var msg in _messageDataSet)
                {
                    msg.Cts?.Dispose();
                }
            }

            _eventBus = null;

            foreach (var msg in _messageDataSet)
            {
                msg.Cts = null;
                msg.TransientMessageTask = null;
            }

            _disposed = true;
        }

        private InfoBarMessageData DisplayMessage(
            Guid ownerId,
            string message,
            InfoBarRegion region,
            InfoBarColor textColor,
            InfoBarColor backgroundColor,
            double durationSec = 0)
        {
            ClearMessage(ownerId, region);

            var messageData = new InfoBarMessageData
            {
                OwnerId = ownerId,
                Region = region,
                TextColor = textColor,
                BackgroundColor = backgroundColor,
                Text = message,
                Duration = durationSec
            };

            _messageDataSet.Add(messageData);

            UpdateDisplay(region);
            Logger.Debug($"Added message owner={ownerId} region={region} message={message}; total={_messageDataSet.Count}");

            IsOpen = true;

            return messageData;
        }

        private async Task DisplayTransientMessage(
            Guid ownerId,
            string message,
            InfoBarRegion region,
            InfoBarColor textColor,
            InfoBarColor backgroundColor,
            TimeSpan duration,
            CancellationToken token = default)
        {
            var messageData = DisplayMessage(ownerId, message, region, textColor, backgroundColor, duration.TotalSeconds);

            Logger.Debug($"Start transient task, duration={duration}");
            var timeoutCts = new CancellationTokenSource();

            // Create a composite token source that auto cancels if either of the 2 sources are canceled
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token))
            {
                var timeoutTask = Task.Delay(duration, linkedCts.Token);

                SetTransientMessageTimeout(messageData, timeoutTask, linkedCts);

                // Asynchronously wait for timer timeout
                await timeoutTask;
                Logger.Debug("Transient task ended naturally");

                // Cleanup message and timeout state after timeout completes
                if (RegionHasTransientMessage(ownerId, region))
                {
                    ClearTransientMessageTimeout(ownerId, region);
                    ClearMessage(ownerId, region);
                }
            }
        }

        private async Task CancelTransientMessageTimeout(Guid ownerId, InfoBarRegion region)
        {
            Logger.Debug($"Try cancel message owner={ownerId} region={region}");
            var foundMessage = GetOwnedRegionTransientMessage(ownerId, region);
            if (foundMessage == null)
            {
                Logger.Debug($"Transient message owner={ownerId} region={region} not found");
                return;
            }

            try
            {
                foundMessage.Cts.Cancel();
                await foundMessage.TransientMessageTask;
            }
            catch (OperationCanceledException)
            {
                Logger.Debug($"Transient message owner={ownerId} region={region}, was canceled");
            }
        }

        private void SetTransientMessageTimeout(
            InfoBarMessageData messageData,
            Task timeoutTask,
            CancellationTokenSource timeoutCts)
        {
            messageData.TransientMessageTask = timeoutTask;
            messageData.Cts = timeoutCts;
        }

        private void ClearTransientMessageTimeout(Guid ownerId, InfoBarRegion region)
        {
            var foundMessage = GetOwnedRegionTransientMessage(ownerId, region);
            if (foundMessage == null)
            {
                return;
            }

            foundMessage.Cts.Dispose();
            foundMessage.Cts = null;
            foundMessage.TransientMessageTask = null;
            Logger.Debug($"Cleared transient message owner={ownerId} region={region}");
        }

        private void ClearMessage(Guid ownerId, InfoBarRegion region)
        {
            if (_messageDataSet.RemoveAll(m => m.OwnerId == ownerId && m.Region == region) > 0)
            {
                Logger.Debug($"Removed message owner={ownerId} region={region}; total={_messageDataSet.Count}");

                UpdateDisplay(region);
            }
        }

        private bool RegionHasTransientMessage(Guid ownerId, InfoBarRegion region)
        {
            return _messageDataSet.Any(m => m.OwnerId == ownerId && m.Region == region && m.TransientMessageTask != null);
        }

        private InfoBarMessageData GetOwnedRegionTransientMessage(Guid ownerId, InfoBarRegion region)
        {
            if (RegionHasTransientMessage(ownerId, region))
            {
                return _messageDataSet.First(m => m.OwnerId == ownerId && m.Region == region && m.TransientMessageTask != null);
            }

            Logger.Debug($"Looking for transient message owner={ownerId} region={region}, not found");
            return null;
        }

        private void UpdateDisplay(InfoBarRegion region)
        {
            switch (region)
            {
                // For each region, concatenate its text messages, and use the last one's text color and duration.
                case InfoBarRegion.Left:
                    var leftMessages = _messageDataSet.Where(m => m.Region == InfoBarRegion.Left).ToList();
                    LeftRegionText = string.Join(" | ", leftMessages.Select(m => m.Text));
                    LeftRegionTextColor = leftMessages.Any() ? leftMessages.Last().TextColor : InfoBarColor.Black;
                    LeftRegionDuration = leftMessages.Any() ? leftMessages.Last().Duration : 0;
                    Logger.Debug($"left text={LeftRegionText} color={LeftRegionTextColor} dur={LeftRegionDuration}");
                    break;

                case InfoBarRegion.Center:
                    var centerMessages = _messageDataSet.Where(m => m.Region == InfoBarRegion.Center).ToList();
                    CenterRegionText = string.Join(" | ", centerMessages.Select(m => m.Text));
                    CenterRegionTextColor = centerMessages.Any() ? centerMessages.Last().TextColor : InfoBarColor.Black;
                    CenterRegionDuration = centerMessages.Any() ? centerMessages.Last().Duration : 0;
                    Logger.Debug($"center text={CenterRegionText} color={CenterRegionTextColor} dur={CenterRegionDuration}");
                    break;

                case InfoBarRegion.Right:
                    var rightMessages = _messageDataSet.Where(m => m.Region == InfoBarRegion.Right).ToList();
                    RightRegionText = string.Join(" | ", rightMessages.Select(m => m.Text));
                    RightRegionTextColor = rightMessages.Any() ? rightMessages.Last().TextColor : InfoBarColor.Black;
                    RightRegionDuration = rightMessages.Any() ? rightMessages.Last().Duration : 0;
                    Logger.Debug($"right text={RightRegionText} color={RightRegionTextColor} dur={RightRegionDuration}");
                    break;
            }

            // Set background to the most recent definition.
            BackgroundColor = _messageDataSet.Count > 0 ? _messageDataSet.Last().BackgroundColor : InfoBarColor.Black;
            Logger.Debug($"background color={BackgroundColor}");
        }

        private async Task Handler(InfoBarDisplayStaticMessageEvent e, CancellationToken token)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                async () =>
                {
                    try
                    {
                        if (RegionHasTransientMessage(e.OwnerId, e.Region))
                        {
                            await CancelTransientMessageTimeout(e.OwnerId, e.Region);
                            ClearTransientMessageTimeout(e.OwnerId, e.Region);
                        }

                        DisplayMessage(e.OwnerId, e.Message, e.Region, e.TextColor, e.BackgroundColor);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error displaying static message owner={e.OwnerId} region={e.Region}.", ex);
                    }
                });
        }

        private async Task Handler(InfoBarDisplayTransientMessageEvent e, CancellationToken token)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                async () =>
                {
                    try
                    {
                        if (RegionHasTransientMessage(e.OwnerId, e.Region))
                        {
                            await CancelTransientMessageTimeout(e.OwnerId, e.Region);
                            ClearTransientMessageTimeout(e.OwnerId, e.Region);
                        }

                        await DisplayTransientMessage(
                            e.OwnerId,
                            e.Message,
                            e.Region,
                            e.TextColor,
                            e.BackgroundColor,
                            e.Duration,
                            token);

                        if (IsInfoBarEmpty)
                        {
                            IsOpen = false;
                            _eventBus.Publish(new InfoBarCloseEvent(e.DisplayTarget));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Debug("Transient message was canceled early");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error displaying transient message owner={e.OwnerId} region={e.Region}.", ex);
                    }
                });
        }

        private async Task Handler(InfoBarClearMessageEvent e, CancellationToken token)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                async () =>
                {
                    foreach (var region in e.Regions)
                    {
                        try
                        {
                            if (RegionHasTransientMessage(e.OwnerId, region))
                            {
                                await CancelTransientMessageTimeout(e.OwnerId, region);
                                ClearTransientMessageTimeout(e.OwnerId, region);
                            }

                            ClearMessage(e.OwnerId, region);

                            if (IsInfoBarEmpty)
                            {
                                IsOpen = false;
                                _eventBus.Publish(new InfoBarCloseEvent(e.DisplayTarget));
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error clearing message owner={e.OwnerId} region={e.Regions}.", ex);
                        }
                    }
                });
        }

        private void Handler(InfoBarCloseEvent e) => Execute.OnUIThread(() => IsOpen = false);

        private void Handler(InfoBarSetHeightEvent e)
        {
            Execute.OnUIThread(() =>
            {
                var scaleUp = e.Height / BarHeightMinimum;

                BarHeight = e.Height;
                FontSize = FontSizeMinimum * scaleUp;
                RegionMargin = new Thickness(
                    MarginSide * scaleUp,
                    MarginTop * scaleUp,
                    MarginSide * scaleUp,
                    0);
                Logger.Debug($"BarHeight = {BarHeight}, FontSize = {FontSize}");
            });
        }
    }
}
