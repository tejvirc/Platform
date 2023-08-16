namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System;
    using System.Diagnostics;
    using CefSharp.DevTools.DOM;
    using Hardware.Contracts.ButtonDeck;
    using ManagedBink;
    using Rect = ManagedBink.Rect;

    /// <summary>
    ///     Helper class for rendering the two LCD button deck images from bink video in the lobby.
    ///     Button deck is split over two displays :
    ///     1. 800x256 for bet buttons.
    ///     2. 240x320 for bash button.
    /// </summary>
    internal class LobbyButtonDeckRenderer : IDisposable
    {
        private const int BetButtonWidth = 800;
        private const int BetButtonHeight = 256;
        private const int BashButtonWidth = 240;
        private const int BashButtonHeight = 320;
        private const int RenderWidth = BetButtonWidth + BashButtonWidth;
        private const int RenderHeight = BashButtonHeight;
        private byte[] _bashButtonsDisabledImage;

        private byte[] _betButtonsDisabledImage;

        private BinkVideoDecoder _binkVideo = new BinkVideoDecoder();

        private readonly IButtonDeckDisplay _buttonDeckDisplayService;

        private bool _disposed;

        private uint _frameId;

        private bool _renderingEnable;
        private readonly Rect[] _subImageRects;

        private string _videoFilename;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyButtonDeckRenderer" /> class.
        /// </summary>
        /// <param name="buttonDeckDisplay">The button deck display service.</param>
        /// <param name="videoFilename">The filename of the video to play.</param>
        public LobbyButtonDeckRenderer(IButtonDeckDisplay buttonDeckDisplay, string videoFilename)
        {
            _buttonDeckDisplayService = buttonDeckDisplay;

            _subImageRects = new []
            {
                new Rect
                {
                    X = 0,
                    Y = RenderHeight - BetButtonHeight,
                    Width = BetButtonWidth,
                    Height = BetButtonHeight
                },
                new Rect { X = BetButtonWidth, Y = 0, Width = BashButtonWidth, Height = BashButtonHeight }
            };

            LoadDisabledImage();

            _videoFilename = videoFilename;
            _binkVideo.Open(videoFilename, _subImageRects, true);
            Debug.Assert(_binkVideo.VideoWidth == RenderWidth, $"Expected button deck video width to be {RenderWidth}");
            Debug.Assert(
                _binkVideo.VideoHeight == RenderHeight,
                $"Expected button deck video height to be {RenderHeight}");
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the system is disabled.
        /// </summary>
        public bool IsSystemDisabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether rendering is enabled.
        /// </summary>
        public bool RenderingEnabled
        {
            get => _renderingEnable;

            set
            {
                if (_renderingEnable != value)
                {
                    if (value)
                    {
                        _binkVideo?.Play();
                    }
                    else
                    {
                        _binkVideo?.Stop();
                    }

                    _renderingEnable = value;
                }

                if (IsSystemDisabled)
                {
                    DrawSystemDisabledGraphics();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the video filename to play.
        /// </summary>
        public string VideoFilename
        {
            get => _videoFilename;

            set
            {
                if (_videoFilename != value)
                {
                    _videoFilename = value;

                    var wasPlaying = _binkVideo?.Playing ?? false;

                    _binkVideo?.Stop();

                    _binkVideo?.Open(_videoFilename, _subImageRects, true);

                    if (wasPlaying)
                    {
                        _binkVideo?.Play();
                    }
                }
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Update function
        /// </summary>
        public void Update()
        {
            if (RenderingEnabled)
            {
                if (IsSystemDisabled)
                {
                    DrawSystemDisabledGraphics();
                }
                else
                {
                    DrawLobbyVideoGraphics();
                }
            }
        }

        /// <summary>
        ///     Cleanup.
        /// </summary>
        /// <param name="disposing">True if disposing; false if finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                RenderingEnabled = false;

                if (_binkVideo != null)
                {
                    _binkVideo.Dispose();
                }
            }

            _binkVideo = null;

            _disposed = true;
        }

        private void LoadDisabledImage()
        {
            _betButtonsDisabledImage = new byte[BetButtonWidth * BetButtonHeight * 2];
            _bashButtonsDisabledImage = new byte[BashButtonWidth * BashButtonHeight * 2];

            // TODO: Probably get artwork for this, but for now it is just a solid color.
            for (var i = 0; i < _betButtonsDisabledImage.Length; ++i)
            {
                _betButtonsDisabledImage[i] = 0x0;
            }

            for (var i = 0; i < _bashButtonsDisabledImage.Length; ++i)
            {
                _bashButtonsDisabledImage[i] = 0x0;
            }
        }

        private void DrawSystemDisabledGraphics()
        {
            _buttonDeckDisplayService?.Draw(0, _betButtonsDisabledImage);
            _buttonDeckDisplayService?.Draw(1, _bashButtonsDisabledImage);
        }

        private void DrawLobbyVideoGraphics()
        {
            if (_binkVideo?.Playing ?? false)
            {
                _binkVideo.Update();

                // Avoid drawing pixels if this is called faster than the bink video updates.
                if (_frameId != _binkVideo.FrameId)
                {
                    var subImage0 = _binkVideo.DecodedSubImage(0);
                    if (subImage0 != IntPtr.Zero)
                    {
                        _buttonDeckDisplayService?.Draw(0, subImage0, _binkVideo.DecodedSubImageByteCount(0));
                    }

                    var subImage1 = _binkVideo.DecodedSubImage(1);
                    if (subImage1 != IntPtr.Zero)
                    {
                        _buttonDeckDisplayService?.Draw(1, subImage1, _binkVideo.DecodedSubImageByteCount(1));
                    }

                    _frameId = _binkVideo.FrameId;
                }
            }
        }

        private ushort MakeColor_R5G6B5(uint red, uint green, uint blue)
        {
            const uint mask5 = 0x1f; // mask bottom 5 bits
            const uint mask6 = 0x3f; // mask bottom 6 bits

            var fr = red / 255.0f;
            var fg = green / 255.0f;
            var fb = blue / 255.0f;

            // Rescale to bit range.
            var r = (uint)(fr * mask5);
            var g = (uint)(fg * mask6);
            var b = (uint)(fb * mask5);

            // Repack
            var pixel565 = (r << 11) | (g << 5) | b;

            return (ushort)pixel565;
        }
    }
}
