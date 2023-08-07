namespace Aristocrat.Monaco.Application.Media
{
    using Contracts;
    using Contracts.Media;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    ///     Implement the <see cref="IMediaProvider"/> interface
    /// </summary>
    public partial class MediaProvider : IMediaProvider, IPropertyProvider, IDisposable
    {
        private const PersistenceLevel PersistenceLevel = Hardware.Contracts.Persistence.PersistenceLevel.Critical;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const string DataBlock = @"Data";
        private const string IdBlock = @"Id";
        private const string LogBlock = @"Log";

        private const string MediaPlayerId = @"MediaPlayer.Id";
        private const string MediaPlayerDescription = @"MediaPlayer.Description";
        private const string MediaPlayerDisplayPosition = @"MediaPlayer.DisplayPosition";
        private const string MediaPlayerPriority = @"MediaPlayer.Priority";
        private const string MediaPlayerCurrentStatus = @"MediaPlayer.Status";
        private const string MediaPlayerLogCount = @"MediaPlayer.LogCount";

        private const string MediaPlayerMediaId = @"Media.Id";
        private const string MediaPlayerMediaAddress = @"Media.Address";
        private const string MediaPlayerMediaAccessToken = @"Media.AccessToken";
        private const string MediaPlayerMediaState = @"Media.State";
        private const string MediaPlayerMediaExceptionCode = @"Media.ExceptionCode";
        private const string MediaPlayerMediaTransactionId = @"Media.TransactionId";
        private const string MediaPlayerMediaSequenceNumber = @"Media.SequenceNumber";
        private const string MediaPlayerMediaAuthorizedEvents = @"Media.AuthorizedEvents";
        private const string MediaPlayerMediaAuthorizedCommands = @"Media.AuthorizedCommands";
        private const string MediaPlayerMediaMdContentToken = @"Media.MdContentToken";
        private const string MediaPlayerMediaEmdiConnectionRequired = @"Media.EmdiConnectionRequired";
        private const string MediaPlayerMediaEmdiReconnectTimer = @"Media.EmdiReconnectTimer";
        private const string MediaPlayerMediaLoadTime = @"Media.LoadTime";
        private const string MediaPlayerMediaReleaseTime = @"Media.ReleaseTime";
        private const string MediaPlayerMediaNativeResolution = @"Media.NativeResolution";
        private const string MediaPlayerMediaPlayerId = @"Media.PlayerId";

        private const int DefaultPort = 1023;
        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageAccessor _dataBlock;
        private readonly IPersistentStorageAccessor _idBlock;
        private readonly IPersistentStorageAccessor _logBlock;
        private const int PlaceHolderIdStart = 1000;

        private readonly List<MediaPlayer> _mediaPlayers = new ();

        private readonly List<MediaPlayer> _placeholders = new ();

        private readonly IPersistentStorageManager _storageManager;

        private readonly List<Media> _medias = new ();

        private bool _disposed;

        /// <summary>
        ///     Constructor for <see cref="MediaProvider"/>
        /// </summary>
        public MediaProvider()
        {
            _storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            _idBlock = _storageManager.GetAccessor(PersistenceLevel, GetBlockName(IdBlock));
            _dataBlock = _storageManager.GetAccessor(PersistenceLevel, GetBlockName(DataBlock));
            _logBlock = _storageManager.GetAccessor(PersistenceLevel, GetBlockName(LogBlock));

            // These events allow us to keep our model current; they do not cause protocol activity.
            _eventBus.Subscribe<MediaPlayerContentReadyEvent>(this, HandleEvent);
            _eventBus.Subscribe<MediaPlayerModalChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<MediaPlayerTopmostChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<MediaPlayerGamePlaySuspenseChangedEvent>(this, HandleEvent);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IMediaPlayer GetMediaPlayer(int id)
        {
            return _mediaPlayers.FirstOrDefault(m => m.Id == id);
        }

        /// <inheritdoc />
        public IEnumerable<IMediaPlayer> GetMediaPlayers()
        {
            return _mediaPlayers;
        }

        public IEnumerable<IMediaPlayer> GetPlaceholders()
        {
            return _placeholders;    
        }

        /// <inheritdoc />
        public IMedia GetMedia(int id, long contentId, long transactionId)
        {
            return FindMedia(id, contentId, transactionId);
        }

        /// <inheritdoc />
        public void Enable(int id, MediaPlayerStatus status)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);

            if ((player.Status & status) == 0)
            {
                return;
            }

            player.Status &= ~status;
            UpdatePlayer(player);

            if (player.Enabled)
            {
                SetState(id, true);

                if (!player.IsPlaceholder)
                {
                    _eventBus.Publish(new MediaPlayerEnabledEvent(player, status));
                }
            }
        }

        /// <inheritdoc />
        public void Disable(int id, MediaPlayerStatus status)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);

            if ((player.Status & status) == 0)
            {
                player.Status |= status;
                UpdatePlayer(player);

                SetState(id, false);

                if (!player.IsPlaceholder)
                {
                    _eventBus.Publish(new MediaPlayerDisabledEvent(player, status));
                }
            }
        }

        /// <inheritdoc />
        public long Preload(
            int id,
            string contentUri,
            long contentId,
            bool nativeRes,
            long accessToken,
            IEnumerable<string> authorizedEvents, 
            IEnumerable<string> authorizedCommands,
            long mdContentToken,
            int emdiReconnectTimer)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);

            if (!player.Enabled)
            {
                return -1;
            }

            var idProvider = ServiceManager.GetInstance().GetService<IIdProvider>();

            var media = new Media
            {
                TransactionId = idProvider.GetNextTransactionId(),
                Id = contentId,
                Address = new Uri(contentUri),
                NativeResolution = nativeRes,
                AccessToken = accessToken,
                AuthorizedEvents = authorizedEvents,
                AuthorizedCommands = authorizedCommands,
                MdContentToken = mdContentToken,
                EmdiConnectionRequired = (0 < emdiReconnectTimer),
                EmdiReconnectTimer = emdiReconnectTimer,
                PlayerId = id,
                State = MediaState.Pending,
                LoadTime = null,
                ReleaseTime = null,
                LogSequence = idProvider.GetNextLogSequence<IMediaProvider>()
            };

            AddMedia(media);

            UpdateMediaLog();

            return media.TransactionId;
        }

        /// <inheritdoc />
        public void Load(int id, long contentId, long transactionId)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);

            if (player.Enabled)
            {
                var media = FindMedia(id, contentId, transactionId);

                player.Unload(media);

                media.State = MediaState.Loaded;
                media.LoadTime = DateTime.Now;

                UpdatePlayer(player);

                _eventBus.Publish(new PrepareContentMediaPlayerEvent(player, media));
            }
        }

        /// <inheritdoc />
        public void Unload(int id, long contentId, long transactionId)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);

            if (player.Enabled)
            {
                var media = FindMedia(id, contentId, transactionId);

                player.Unload(media);

                media.State = MediaState.Released;
                media.ReleaseTime = DateTime.Now;

                UpdatePlayer(player);

                _eventBus.Publish(new ReleaseContentMediaPlayerEvent(player, media));
            }
        }

        /// <inheritdoc />
        public void ActivateContent(int id, long contentId, long transactionId)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);
            Debug.Assert(player != null && !player.IsPlaceholder);

            if (player.Enabled)
            {
                var media = FindMedia(id, contentId, transactionId);

                player.Start(media);
                UpdatePlayer(player);

                _eventBus.Publish(new SetContentMediaPlayerEvent(player, media));
            }
        }

        /// <inheritdoc />
        public void UpdatePlayer(IMediaPlayer player, bool visible)
        {
            Logger.Debug($"UpdatePlayer MediaPlayer {player.Id}: Visible={visible}");
            ((MediaPlayer)player).Visible = visible;

            UpdatePlayer(player);
        }

        /// <inheritdoc />
        public void Show(int id)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);
            Logger.Debug($"RequestShow MediaPlayer {id}");
            player.RequestShow();
        }

        /// <inheritdoc />
        public void Hide(int id)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);
            Logger.Debug($"RequestHide MediaPlayer {id}");
            player.RequestHide();
        }

        /// <inheritdoc />
        public void Raise(int id)
        {
            var player = _mediaPlayers.Single(m => m.Id == id);

            if (player.Enabled)
            {
                player.Raise();
                UpdatePlayer(player);

                _eventBus.Publish(new RaiseMediaPlayerEvent(player));
            }
        }

        public void ShowHidePlaceholders(int linkId, bool show)
        {
            var placeholders = GetPlaceholders(linkId);
            if (placeholders.Any())
            {
                if (show)
                {
                    placeholders.ForEach(o => o.RequestShow());
                }
                else
                {
                    placeholders.ForEach(o => o.RequestHide());
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<IMediaScreen> GetScreens()
        {
            var screens = new List<IMediaScreen>();

            var primary = Screen.AllScreens.FirstOrDefault(s => s.Primary);
            if (primary != null)
            {
                screens.Add(
                    new MediaScreen
                    {
                        Type = ScreenType.Primary,
                        Description = @"Game Screen",
                        Height = primary.Bounds.Height,
                        Width = primary.Bounds.Width
                    });
            }

            var secondary = Screen.AllScreens.FirstOrDefault(s => !s.Primary && s.Bounds.Y < 0);
            if (secondary != null)
            {
                screens.Add(
                    new MediaScreen
                    {
                        Type = ScreenType.Glass,
                        Description = @"Top Glass",
                        Height = secondary.Bounds.Height,
                        Width = secondary.Bounds.Width
                    });
            }

            return screens;
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>(ApplicationConstants.MediaPlayers, GetMediaPlayers())
        };

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case ApplicationConstants.MediaPlayers:
                    return GetMediaPlayers();
                default:
                    var errorMessage = "Unknown media property: " + propertyName;
                    throw new UnknownPropertyException(errorMessage);
            }
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            // No external sets for this provider...
        }

        /// <inheritdoc />
        public string Name => typeof(IMediaProvider).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMediaProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
            var enabled = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(ApplicationConstants.MediaDisplayEnabled, false);

            if (!enabled) return;

            GetCurrentPlayers();

            Configure();

            _storageManager.ResizeBlock(GetBlockName(LogBlock), MinimumMediaLogSize);

            ServiceManager.GetInstance().GetService<IPropertiesManager>().AddPropertyProvider(this);
        }

        /// <inheritdoc />
        public void SendEmdiFromHostToContent(int id, string base64Message)
        {
            var player = GetMediaPlayer(id);

            if (player.Enabled)
            {
                _eventBus.Publish(new MediaPlayerEmdiMessageFromHostEvent(player, base64Message));
            }
        }

        /// <inheritdoc />
        public void SendEmdiFromContentToHost(int id, string base64Message)
        {
            var player = GetMediaPlayer(id);

            if (player.Enabled)
            {
                _eventBus.Publish(new MediaPlayerEmdiMessageToHostEvent(player, base64Message));
            }
        }

        /// <inheritdoc />
        public void SetEmdiConnected(int port, bool connected)
        {
            var player = _mediaPlayers.First(m => m.Port == port);
 
            var previouslyConnected = player.EmdiConnected;
            player.EmdiConnected = connected;

            // If a media content requires EMDI connection...
            if (null != player.ActiveMedia && player.ActiveMedia.EmdiConnectionRequired)
            {
                // If the connection has been lost and media is still active
                // that requires it, start a reconnect timer.
                if (previouslyConnected && !connected)
                {
                    Task.Delay(player.ActiveMedia.EmdiReconnectTimer).ContinueWith(_ =>
                    {
                        // better check again after the delay
                        if (null != player.ActiveMedia && player.ActiveMedia.EmdiConnectionRequired && !player.EmdiConnected)
                        {
                            _eventBus.Publish(new MediaPlayerContentReadyEvent(player.Id, player.ActiveMedia, MediaContentError.EmdiConnectionError));
                        }
                    });
                }
            }
        }

        /// <inheritdoc />
        public void LogContentEvent(int mediaPlayerId, string contentName, string eventName, string eventDescription)
        {
            Logger.InfoFormat(CultureInfo.CurrentCulture, "EMDI: LogContentEvent Device: {0}, ContentName: {1}, Event Name: {2}, Event Description: {3}",
                mediaPlayerId, contentName, eventName, eventDescription);
        }

        /// <inheritdoc />
        public IEnumerable<IMedia> MediaLog => _medias;

        /// <inheritdoc />
        public int MaxMediaStorage => 10;

        /// <inheritdoc />
        public bool MaxMediaStorageReached => _medias.Count(m => m.State == MediaState.Loaded || m.State == MediaState.Executing) >= MaxMediaStorage;

        /// <inheritdoc />
        public int MinimumMediaLogSize => 35;

        public bool IsPrimaryOverlayVisible => _mediaPlayers.Any(m => m.IsPrimaryOverlay() && m.Visible);

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
        
        private void AddMedia(Media media)
        {
            if (MinimumMediaLogSize <= _medias.Count)
            {
                var oldestFinalizedMedia = _medias.First(m => m.IsFinalized);
                _medias.Remove(oldestFinalizedMedia);
            }

            _medias.Add(media);
        }

        private Media FindMedia(int playerId, long contentId, long transactionId)
        {
            return _medias.SingleOrDefault(
                m => m.PlayerId == playerId && m.Id == contentId && m.TransactionId == transactionId);
        }

        private static Template GetTemplate(IMediaScreen screen)
        {
            var aspectRatio = GetAspectRatio(screen);

            switch (aspectRatio.ToString())
            {
                case "16:9":
                case "8:5": // 8:5 = 16:10
                case "683:384": // Described as 16:9 
                    return Template.WideLandscape;
                case "4:3":
                case "5:4":
                    return Template.Landscape;
                case "9:16":
                case "5:8": // 5:8 = 10:16
                case "384:683": // Described as 9:16
                    return Template.Portrait;
                default:
                    return Template.Unknown;
            }
        }

        private static AspectRatio GetAspectRatio(IMediaScreen screen)
        {
            var divisor = GetGreatestCommonDivisor(screen.Width, screen.Height);

            return new AspectRatio(screen.Width / divisor, screen.Height / divisor);
        }

        private static int GetGreatestCommonDivisor(int a, int b)
        {
            while (true)
            {
                if (b == 0)
                {
                    return a;
                }

                var a1 = a;
                a = b;
                b = a1 % b;
            }
        }

        private void GetCurrentPlayers()
        {
            var mediaPlayerId = (int)_idBlock[MediaPlayerId];

            var mediaPlayers = _dataBlock.GetAll();

            for (var index = 0; index < mediaPlayerId; index++)
            {
                if (mediaPlayers.TryGetValue(index, out var data))
                {
                    _mediaPlayers.Add(new MediaPlayer
                    {
                        Id = (int)data[MediaPlayerId],
                        Description = (string)data[MediaPlayerDescription],
                        DisplayPosition = (DisplayPosition)data[MediaPlayerDisplayPosition],
                        Priority = (int)data[MediaPlayerPriority],
                        Status = (MediaPlayerStatus)data[MediaPlayerCurrentStatus],
                        EmdiConnected = false
                    });
                }
            }

            RestoreMediaLog();
        }

        private void Configure()
        {
            var screens = GetScreens().ToList();

            var primary = screens.FirstOrDefault(s => s.Type == ScreenType.Primary);
            if (primary != null)
            {
                switch (GetTemplate(primary))
                {
                    case Template.Landscape:
                        break;
                    case Template.WideLandscape:
                        var serviceWindow = GetOrCreatePlayer(
                            primary,
                            @"Wide Service Window",
                            DisplayPosition.Right,
                            2,
                            DisplayType.Scale,
                            288,
                            900,
                            true);

                        serviceWindow.DisplayWidth = new AspectRatio(8, 25).GetCalculatedWidth(primary.Height);

                        var serviceWindowPlaceholders = GetPlaceholders(serviceWindow.Id);
                        serviceWindowPlaceholders.ForEach(o => o.DisplayWidth = serviceWindow.DisplayWidth);

                        var banner = GetOrCreatePlayer(
                            primary,
                            @"Wide Primary Banner",
                            DisplayPosition.Bottom,
                            3,
                            DisplayType.Scale,
                            1440,
                            144,
                            true);

                        banner.DisplayHeight = new AspectRatio(10, 1).GetCalculatedHeight(primary.Width);
                        banner.YPosition = primary.Height - 144;

                        var bannerPlaceholders = GetPlaceholders(banner.Id);
                        bannerPlaceholders.ForEach(o => o.DisplayHeight = banner.DisplayHeight);
                        bannerPlaceholders.ForEach(o => o.YPosition = banner.YPosition);

                        GetOrCreatePlayer(
                            primary,
                            @"Wide Primary Overlay",
                            DisplayPosition.FullScreen,
                            1,
                            DisplayType.Overlay,
                            720,
                            450);
                        break;
                    case Template.Portrait:
                        break;
                    case Template.Unknown:
                        break;
                }
            }

            var secondary = screens.FirstOrDefault(s => s.Type == ScreenType.Glass);
            if (secondary != null)
            {
                switch (GetTemplate(secondary))
                {
                    case Template.Unknown:
                        break;
                    case Template.Landscape:
                        break;
                    case Template.WideLandscape:
                        var overlay = GetOrCreatePlayer(
                            secondary,
                            @"Wide Secondary Overlay",
                            DisplayPosition.FullScreen,
                            1,
                            DisplayType.Overlay,
                            720,
                            450);

                        overlay.TouchCapable = false;
                        break;
                    case Template.Portrait:
                        break;
                }
            }
        }

        private MediaPlayer GetOrCreatePlayer(
            IMediaScreen screen,
            string description,
            DisplayPosition position,
            int priority,
            DisplayType displayType,
            int width,
            int height,
            bool createPlaceholder = false)
        {
            var player = FindMediaPlayer(description, position) ?? new MediaPlayer();

            player.TouchCapable = true;
            player.AudioCapable = true;
            player.Visible = false;
            player.DisplayHeight = screen.Height;
            player.DisplayWidth = screen.Width;
            player.ScreenType = screen.Type;
            player.DisplayType = displayType;
            player.ScreenDescription = screen.Description;
            player.Port = DefaultPort + player.Id;
            player.XPosition = 0;
            player.YPosition = 0;
            player.Width = width;
            player.Height = height;

            if (player.Id == 0)
            {
                var mediaPlayerId = (int)_idBlock[MediaPlayerId] + 1;

                _idBlock[MediaPlayerId] = mediaPlayerId;

                _storageManager.ResizeBlock(GetBlockName(DataBlock), mediaPlayerId);

                var blockIndex = mediaPlayerId - 1;

                using (var transaction = _dataBlock.StartTransaction())
                {
                    transaction[blockIndex, MediaPlayerId] = mediaPlayerId;
                    transaction[blockIndex, MediaPlayerDescription] = description;
                    transaction[blockIndex, MediaPlayerDisplayPosition] = position;
                    transaction[blockIndex, MediaPlayerCurrentStatus] = MediaPlayerStatus.None;
                    transaction[blockIndex, MediaPlayerPriority] = priority;

                    transaction.Commit();
                }

                player.Id = mediaPlayerId;
                player.Description = description;
                player.DisplayPosition = position;
                player.Priority = priority;
                player.Status = MediaPlayerStatus.None;
                player.Port = DefaultPort + mediaPlayerId;

                _mediaPlayers.Add(player);
            }

            if (createPlaceholder)
            {
                CreatePlaceholder(
                    player.Id,
                    screen,
                    description,
                    position,
                    priority,
                    displayType,
                    width,
                    height);
            }

            return player;
        }

        private void CreatePlaceholder(
                    int linkId,
                    IMediaScreen screen,
                    string description,
                    DisplayPosition position,
                    int priority,
                    DisplayType displayType,
                    int width,
                    int height)
        {
            var player = new MediaPlayer
            {
                TouchCapable = true,
                AudioCapable = true,
                Visible = false,
                DisplayHeight = screen.Height,
                DisplayWidth = screen.Width,
                ScreenType = screen.Type,
                DisplayType = displayType,
                ScreenDescription = screen.Description,
                Port = 0,
                XPosition = 0,
                YPosition = 0,
                Width = width,
                Height = height,
                Description = description + " Placeholder",
                DisplayPosition = position,
                Priority = priority,
                Status = MediaPlayerStatus.None,
                LinkId = linkId
            };

            // placeholder media viewer.
            var mediaPlayerId = PlaceHolderIdStart;
            var placeholderPlayers = _mediaPlayers.Where(o => o.LinkId.HasValue).ToList();
            if (placeholderPlayers.Any())
            {
                var lastPlaceholderId = placeholderPlayers.Max(o => o.Id);
                mediaPlayerId = lastPlaceholderId + 1;
            }

            player.Id = mediaPlayerId;
            _placeholders.Add(player);
        }

        private MediaPlayer FindMediaPlayer(string description, DisplayPosition position)
        {
            return _mediaPlayers.SingleOrDefault(m => m.Description == description && m.DisplayPosition == position);
        }

        private void UpdatePlayer(IMediaPlayer player)
        {
            if (player.IsPlaceholder)
                return;

            var blockIndex = player.Id - 1;
            using (var transaction = _dataBlock.StartTransaction())
            {
                transaction[blockIndex, MediaPlayerDisplayPosition] = player.DisplayPosition;
                transaction[blockIndex, MediaPlayerPriority] = player.Priority;
                transaction[blockIndex, MediaPlayerCurrentStatus] = player.Status;
                transaction.Commit();
            }
        }

        private void RestoreMediaLog()
        {
            var logCount = (int)_idBlock[MediaPlayerLogCount];

            var logs = _logBlock.GetAll();

            for (var logIndex = 0; logIndex < logCount; logIndex++)
            {
                if (!logs.TryGetValue(logIndex, out var log))
                {
                    continue;
                }

                var media =
                    new Media
                    {
                        Id = (long)log[MediaPlayerMediaId],
                        Address = new Uri((string)log[MediaPlayerMediaAddress]),
                        AccessToken = (long)log[MediaPlayerMediaAccessToken],
                        State = (MediaState)log[MediaPlayerMediaState],
                        ExceptionCode = (MediaContentError)log[MediaPlayerMediaExceptionCode],
                        TransactionId = (long)log[MediaPlayerMediaTransactionId],
                        LogSequence = (long)log[MediaPlayerMediaSequenceNumber],
                        AuthorizedEvents = new List<string>(((string)log[MediaPlayerMediaAuthorizedEvents]).Split(',')),
                        AuthorizedCommands = new List<string>(((string)log[MediaPlayerMediaAuthorizedCommands]).Split(',')),
                        MdContentToken = (long)log[MediaPlayerMediaMdContentToken],
                        EmdiConnectionRequired = (bool)log[MediaPlayerMediaEmdiConnectionRequired],
                        EmdiReconnectTimer = (int)log[MediaPlayerMediaEmdiReconnectTimer],
                        LoadTime = new DateTime((long)log[MediaPlayerMediaLoadTime]),
                        ReleaseTime = new DateTime((long)log[MediaPlayerMediaReleaseTime]),
                        NativeResolution = (bool)log[MediaPlayerMediaNativeResolution],
                        PlayerId = (int)log[MediaPlayerMediaPlayerId]
                    };

                // Upon reboot, all content isn't actually loaded
                if (media.State != MediaState.Error)
                {
                    media.State = MediaState.Released;
                }

                AddMedia(media);
            }
        }

        private void UpdateMediaLog()
        {
            for (var logIndex = 0; logIndex < MediaLog.Count(); logIndex++)
            {
                var media = MediaLog.ElementAt(logIndex);

                using (var transaction = _logBlock.StartTransaction())
                {
                    transaction[logIndex, MediaPlayerMediaId] = media.Id;
                    transaction[logIndex, MediaPlayerMediaAddress] = media.Address.ToString();
                    transaction[logIndex, MediaPlayerMediaAccessToken] = media.AccessToken;
                    transaction[logIndex, MediaPlayerMediaState] = media.State;
                    transaction[logIndex, MediaPlayerMediaExceptionCode] = media.ExceptionCode;
                    transaction[logIndex, MediaPlayerMediaTransactionId] = media.TransactionId;
                    transaction[logIndex, MediaPlayerMediaSequenceNumber] = media.LogSequence;
                    transaction[logIndex, MediaPlayerMediaAuthorizedEvents] = String.Join(",", media.AuthorizedEvents.ToArray());
                    transaction[logIndex, MediaPlayerMediaAuthorizedCommands] = String.Join(",", media.AuthorizedCommands.ToArray());
                    transaction[logIndex, MediaPlayerMediaMdContentToken] = media.MdContentToken;
                    transaction[logIndex, MediaPlayerMediaEmdiConnectionRequired] = media.EmdiConnectionRequired;
                    transaction[logIndex, MediaPlayerMediaEmdiReconnectTimer] = media.EmdiReconnectTimer;
                    transaction[logIndex, MediaPlayerMediaLoadTime] = media.LoadTime?.Ticks ?? 0;
                    transaction[logIndex, MediaPlayerMediaReleaseTime] = media.ReleaseTime?.Ticks ?? 0;
                    transaction[logIndex, MediaPlayerMediaNativeResolution] = media.NativeResolution;
                    transaction[logIndex, MediaPlayerMediaPlayerId] = media.PlayerId;

                    transaction.Commit();
                }
            }

            _idBlock[MediaPlayerLogCount] = MediaLog.Count();
        }

        private string GetBlockName(string name = null)
        {
            var baseName = GetType().ToString();
            return string.IsNullOrEmpty(name) ? baseName : $"{baseName}.{name}";
        }

        private void SetState(int id, bool enable)
        {
            if (!enable)
            {
                var player = _mediaPlayers.Single(m => m.Id == id);

                if (null != player.ActiveMedia)
                {
                    Unload(id, player.ActiveMedia.Id, player.ActiveMedia.TransactionId);
                }

                Hide(id);
            }
        }

        private List<MediaPlayer> GetPlaceholders(int linkId)
        {
            return _placeholders.Where(o => o.LinkId.HasValue && o.LinkId == linkId).ToList();
        }

        private void HandleEvent(MediaPlayerContentReadyEvent evt)
        {
            if (evt.Media == null) return;

            evt.Media.ExceptionCode = evt.ContentError;
            evt.Media.State = (evt.ContentError == MediaContentError.None) ? MediaState.Executing : MediaState.Error;

            _eventBus.Publish(new MediaPlayerContentResultEvent(evt.MediaPlayerId, evt.Media));

            if (evt.Media.State == MediaState.Error)
            {
                Logger.Debug($"Automatically hiding media player {evt.MediaPlayerId} due to error state");
                Hide(evt.MediaPlayerId);
            }
        }

        private void HandleEvent(MediaPlayerModalChangedEvent evt)
        {
            var player = _mediaPlayers.Single(m => m.Id == evt.MediaPlayer.Id);

            if (player.Enabled)
            {
                player.IsModal = evt.On;
            }
        }

        private void HandleEvent(MediaPlayerTopmostChangedEvent evt)
        {
            var player = _mediaPlayers.Single(m => m.Id == evt.MediaPlayer.Id);

            if (player.Enabled)
            {
                player.TopmostWindow = evt.On;
            }
        }

        private void HandleEvent(MediaPlayerGamePlaySuspenseChangedEvent evt)
        {
            var player = _mediaPlayers.Single(m => m.Id == evt.MediaPlayer.Id);

            if (player.Enabled)
            {
                player.GameSuspended = evt.On;
            }
        }

        private enum Template
        {
            Unknown,
            Landscape,
            WideLandscape,
            Portrait
        }
    }
}
