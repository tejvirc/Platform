namespace Aristocrat.Monaco.Application.Media
{
    using Contracts.Media;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class MediaPlayerResizeManager : IMediaPlayerResizeManager, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private IEventBus _eventBus;
        private List<int> _mediaDisplayIds;
        private object _idLock;

        private bool _disposed;

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IMediaPlayerResizeManager) };

        /// <inheritdoc />
        public bool IsResizing
        {
            get
            {
                lock (_idLock)
                {
                    return _mediaDisplayIds != null && _mediaDisplayIds.Any();
                }
            }
        }

        /// <inheritdoc />
        public void Initialize()
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _mediaDisplayIds = new List<int>();
            _idLock = new object();

            _eventBus.Subscribe<MediaPlayerResizeStartEvent>(this, Handler);
            _eventBus.Subscribe<MediaPlayerResizeStopEvent>(this, Handler);
        }

        /// <inheritdoc />
        public bool IdIsResizing(int id)
        {
            lock (_idLock)
            {
                return _mediaDisplayIds.Contains(id);
            }
        }

        private void Handler(MediaPlayerResizeStartEvent e)
        {
            Logger.Debug($"Handle MediaPlayerResizeStartEvent for media player {e.MediaDisplayId}");
            lock (_idLock)
            {
                // Do not send another start event if there are more resizes that have not completed
                // This DOES allow the same ID to be added multiple times, so it's possible to have duplicates in the list
                if (!_mediaDisplayIds.Any())
                {
                    _eventBus.Publish(new ViewResizeEvent(true));
                }

                _mediaDisplayIds.Add(e.MediaDisplayId);
            }
        }

        private void Handler(MediaPlayerResizeStopEvent e)
        {
            Logger.Debug($"Handle MediaPlayerResizeStopEvent for media player {e.MediaDisplayId}");
            lock (_idLock)
            {
                _mediaDisplayIds.Remove(e.MediaDisplayId);

                // Do not send a stop event until all resizes have completed
                if (!_mediaDisplayIds.Any())
                {
                    _eventBus.Publish(new ViewResizeEvent(false));
                }
            }
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
            }

            _disposed = true;
        }
    }
}
