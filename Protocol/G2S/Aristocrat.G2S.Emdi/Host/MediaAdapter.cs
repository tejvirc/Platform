namespace Aristocrat.G2S.Emdi.Host
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Monaco.Application.Contracts.Media;

    /// <summary>
    ///     Helper class for <see cref="IMediaProvider" /> services
    /// </summary>
    public class MediaAdapter : IMediaAdapter, IDisposable
    {
        private readonly ConcurrentDictionary<int, DisplayAction> _displayActions =
            new ConcurrentDictionary<int, DisplayAction>();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaAdapter" /> class.
        /// </summary>
        /// <param name="media">An instance of <see cref="IMediaProvider" /> interface</param>
        public MediaAdapter(IMediaProvider media)
        {
            Provider = media;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IMediaProvider Provider { get; }

        /// <inheritdoc />
        public async Task<bool> SetDeviceVisibleAsync(int port, bool status)
        {
            var player = Provider.GetMediaPlayers().First(x => x.Port == port);

            _displayActions.AddOrUpdate(
                player.Id,
                new DisplayAction
                {
                    Status = status,
                    Timer = new Timer(
                        OnDisplayActionTimerElapsed,
                        player.Id,
                        TimeSpan.FromMilliseconds(300),
                        Timeout.InfiniteTimeSpan)
                },
                (key, action) =>
                {
                    action.Status = status;
                    return action;
                });

            return await Task.FromResult(status);

            // The commented code below properly handles this command based on the EMDI protocol spec. However, the IGT PSM
            // content does not properly implement the EMDI protocol for this command so a workaround has been implemented.
            //if (status)
            //{
            //    _media.Show(player.Id);
            //}
            //else
            //{
            //    _media.Hide(player.Id);
            //}

            //return Success(
            //        new deviceVisibleStatus
            //        {
            //            deviceVisibleState = player.Visible
            //        });
        }

        private void OnDisplayActionTimerElapsed(object state)
        {
            var id = (int)state;

            if (!_displayActions.TryRemove(id, out var action))
            {
                return;
            }

            action.Timer.Dispose();

            if (action.Status)
            {
                Provider.Show(id);
            }
            else
            {
                Provider.Hide(id);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var action in _displayActions.Values)
                {
                    action.Timer.Dispose();
                }

                _displayActions.Clear();
            }

            _disposed = true;
        }

        /// <inheritdoc />
        ~MediaAdapter()
        {
            Dispose(false);
        }

        private class DisplayAction
        {
            public bool Status { get; set; }

            public Timer Timer { get; set; }
        }
    }
}