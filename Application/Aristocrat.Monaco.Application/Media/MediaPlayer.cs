namespace Aristocrat.Monaco.Application.Media
{
    using Contracts.Media;
    using System;

    public partial class MediaProvider
    {
        /// <summary>
        ///     Local implementation of <see cref="IMediaPlayer" />
        /// </summary>
        private class MediaPlayer : IMediaPlayer
        {
            public event EventHandler HideRequested;

            public event EventHandler ShowRequested;

            /// <inheritdoc />
            public int Id { get; set; }

            /// <inheritdoc />
            public int Priority { get; set; }

            /// <inheritdoc />
            public ScreenType ScreenType { get; set; }

            /// <inheritdoc />
            public string ScreenDescription { get; set; }

            /// <inheritdoc />
            public DisplayType DisplayType { get; set; }

            /// <inheritdoc />
            public DisplayPosition DisplayPosition { get; set; }

            /// <inheritdoc />
            public string Description { get; set; }

            /// <inheritdoc />
            public int XPosition { get; set; }

            /// <inheritdoc />
            public int YPosition { get; set; }

            /// <inheritdoc />
            public int Height { get; set; }

            /// <inheritdoc />
            public int Width { get; set; }

            /// <inheritdoc />
            public int DisplayHeight { get; set; }

            /// <inheritdoc />
            public int DisplayWidth { get; set; }

            /// <inheritdoc />
            public bool TouchCapable { get; set; }

            /// <inheritdoc />
            public bool AudioCapable { get; set; }

            /// <inheritdoc />
            public int Port { get; set; }

            /// <inheritdoc />
            public bool EmdiConnected { get; set; }

            /// <inheritdoc />
            public IMedia ActiveMedia { get; private set; }

            /// <inheritdoc />
            public bool Visible { get; set; }
            
            /// <inheritdoc />
            public bool Enabled => Status == MediaPlayerStatus.None;

            /// <inheritdoc />
            public MediaPlayerStatus Status { get; set; }

            /// <inheritdoc />
            public bool GameSuspended { get; set; }

            /// <inheritdoc />
            public bool TopmostWindow { get; set; }

            /// <inheritdoc />
            public bool IsModal { get; set; }

            public int? LinkId { get; set; }

            public bool IsPlaceholder => LinkId.HasValue;

            internal void Start(Media media)
            {
                ActiveMedia = media;
            }

            internal void Unload(IMedia media)
            {
                if (ActiveMedia == media)
                {
                    ActiveMedia = null;
                    Visible = false;
                }
            }
            
            internal void Show()
            {
                Visible = true;
            }
            
            internal void Hide()
            {
                Visible = false;
            }

            /// <summary>
            ///     Request to Show this media player when ready
            /// </summary>
            internal void RequestShow()
            {
                ShowRequested?.Invoke(this, null);
            }

            /// <summary>
            ///     Request to Hide this media player when ready
            /// </summary>
            internal void RequestHide()
            {
                HideRequested?.Invoke(this, null);
            }

            internal void Raise()
            {
                // TODO ExtRMD
            }
        }
    }
}
