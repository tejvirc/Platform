namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using System;

    /// <summary>
    ///     Provides a mechanism to interact with and control a Media Player
    /// </summary>
    public interface IMediaPlayer
    {
        /// <summary>
        ///     Hide requested for this player
        /// </summary>
        event EventHandler HideRequested;

        /// <summary>
        ///     Show requestedfor this player
        /// </summary>
        event EventHandler ShowRequested;

        /// <summary>
        ///     Gets the unique identifier of the media display
        /// </summary>
        int Id { get; }

        /// <summary>
        ///     Gets the media display priority
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Gets the screen type
        /// </summary>
        ScreenType ScreenType { get; }

        /// <summary>
        ///     Gets the screen description
        /// </summary>
        string ScreenDescription { get; }

        /// <summary>
        ///     Gets the behavior of the device in relation to what is already being displayed on the screen
        /// </summary>
        DisplayType DisplayType { get; }

        /// <summary>
        ///     Gets the position of the media display on the screen
        /// </summary>
        DisplayPosition DisplayPosition { get; }

        /// <summary>
        ///     Gets the media display description
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets the distance from the left edge of the screen where the media display window is located
        /// </summary>
        int XPosition { get; }

        /// <summary>
        ///     Gets the distance from the top edge of the screen where the media display window is located
        /// </summary>
        int YPosition { get; }

        /// <summary>
        ///     Gets the recommended height to which the content SHOULD be authored
        /// </summary>
        int Height { get; }

        /// <summary>
        ///     Gets the recommended width to which the content SHOULD be authored
        /// </summary>
        int Width { get; }

        /// <summary>
        ///     Gets the height of the media display on the screen
        /// </summary>
        int DisplayHeight { get; }

        /// <summary>
        ///     Gets the width of the media display on the screen
        /// </summary>
        int DisplayWidth { get; }

        /// <summary>
        ///     Gets a value indicating whether the device can receive touchscreen input
        /// </summary>
        bool TouchCapable { get; }

        /// <summary>
        ///     Gets a value indicating whether the device will play audio
        /// </summary>
        bool AudioCapable { get; }

        /// <summary>
        ///     Gets the port number that is available to make local socket connection. If the value is set to 1023 then the local
        ///     connection is disabled
        /// </summary>
        int Port { get; }

        /// <summary>
        ///     Gets whether an EMDI connection exists currently
        /// </summary>
        bool EmdiConnected { get; }

        /// <summary>
        ///     Gets the active media
        /// </summary>
        IMedia ActiveMedia { get; }

        /// <summary>
        ///     Gets a value indicating whether the media player is visible
        /// </summary>
        bool Visible { get; }

        /// <summary>
        ///     Gets a value indicating whether the media player is enabled
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        ///     Gets the media player status
        /// </summary>
        MediaPlayerStatus Status { get; }

        /// <summary>
        ///     Get whether the game play currently is suspended.
        /// </summary>
        bool GameSuspended { get; }

        /// <summary>
        ///     Get whether this window is topmost.
        /// </summary>
        bool TopmostWindow { get; }

        /// <summary>
        ///     Get whether this window actually is modal.
        /// </summary>
        bool IsModal { get; }

        /// <summary>
        ///     Get whether this window is a placeholder linked to another media player
        /// </summary>
        bool IsPlaceholder { get; }
    }
}
