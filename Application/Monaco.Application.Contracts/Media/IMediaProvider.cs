namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to retrieve and interact with the available media displays
    /// </summary>
    public interface IMediaProvider : IService
    {
        /// <summary>
        ///     Gets a media player by its Id
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <returns>An <see cref="IMediaPlayer" /> if found, else null</returns>
        IMediaPlayer GetMediaPlayer(int id);

        /// <summary>
        ///     Gets a collection of media players
        /// </summary>
        /// <returns>A collection of media players</returns>
        IEnumerable<IMediaPlayer> GetMediaPlayers();

        /// <summary>
        ///  Gets a collection of placeholder media players
        /// </summary>
        /// <returns>A collection of placeholder media players</returns>
        IEnumerable<IMediaPlayer> GetPlaceholders();

        /// <summary>
        ///     Get a media content object by its Ids
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="contentId">The media content Id</param>
        /// <param name="transactionId">The media transactionId</param>
        /// <returns>An <see cref="IMedia"/> if found, else null</returns>
        IMedia GetMedia(int id, long contentId, long transactionId);

        /// <summary>
        ///     Enable a media player with the provided status
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="status">The reason the media player is being disabled.</param>
        void Enable(int id, MediaPlayerStatus status);

        /// <summary>
        ///     Disables a media player with the provided status
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="status">The reason the media player is being disabled.</param>
        void Disable(int id, MediaPlayerStatus status);

        /// <summary>
        ///     Tell a media player to pre-load content
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="contentUri">The media address</param>
        /// <param name="contentId">The content id</param>
        /// <param name="nativeRes">Indicates whether the content should be displayed at its native resolution</param>
        /// <param name="accessToken">The access token</param>
        /// <param name="authorizedEvents">List of authorized EDMI events</param>
        /// <param name="authorizedCommands">List of authorized EDMI commands</param>
        /// <param name="mdContentToken">Non-zero token for EMDI content-to-content comms, if allowed</param>
        /// <param name="emdiReconnectTimer">EMDI reconnect timer (milliseconds) - 0 implies EMDI connection not required</param>
        /// <returns>transaction id</returns>
        long Preload(
            int id,
            string contentUri,
            long contentId,
            bool nativeRes,
            long accessToken,
            IEnumerable<string> authorizedEvents,
            IEnumerable<string> authorizedCommands,
            long mdContentToken,
            int emdiReconnectTimer
            );

        /// <summary>
        ///     Tell a media player to load content
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="contentId">The content id</param>
        /// <param name="transactionId">The transaction id</param>
        void Load(int id, long contentId, long transactionId);

        /// <summary>
        ///     Tell a media player to unload content
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="contentId">The content id</param>
        /// <param name="transactionId">The transaction id</param>
        void Unload(int id, long contentId, long transactionId);

        /// <summary>
        ///     Tell a media player to activate content
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="contentId">The content id</param>
        /// <param name="transactionId">The transaction id</param>
        void ActivateContent(int id, long contentId, long transactionId);

        /// <summary>
        ///     Update a player's visibility and persist property values locally
        /// </summary>
        /// <param name="player"></param>
        /// <param name="visible"></param>
        void UpdatePlayer(IMediaPlayer player, bool visible);

        /// <summary>
        ///     Show a media player
        /// </summary>
        /// <param name="id">The media player id</param>
        void Show(int id);

        /// <summary>
        ///     Hide a media player
        /// </summary>
        /// <param name="id">The media player id</param>
        void Hide(int id);

        /// <summary>
        ///     Raise a media player window to the front
        /// </summary>
        /// <param name="id">The media player id</param>
        void Raise(int id);

        /// <summary>
        /// ShowHidePlaceholders shows or hides all placeholder media players linked to
        /// the media player id passed in.
        /// </summary>
        /// <param name="linkId"></param>
        /// <param name="show"></param>
        void ShowHidePlaceholders(int linkId, bool show);

        /// <summary>
        ///     Gets a collection of screens capable of displaying a media player
        /// </summary>
        /// <returns>A collection of screens</returns>
        IEnumerable<IMediaScreen> GetScreens();

        /// <summary>
        ///     Send EMDI instruction from host to content.
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="base64Message">The base64-encoded message.</param>
        void SendEmdiFromHostToContent(int id, string base64Message);

        /// <summary>
        ///     Send EMDI instruction from content to host.
        /// </summary>
        /// <param name="id">The media player id</param>
        /// <param name="base64Message">The base64-encoded message.</param>
        void SendEmdiFromContentToHost(int id, string base64Message);

        /// <summary>
        ///     Tell the provider whether an EMDI connection has opened or closed.
        /// </summary>
        /// <param name="port">Which port</param>
        /// <param name="connected">True if connection is open, false if closed</param>
        void SetEmdiConnected(int port, bool connected);

        /// <summary>
        ///     Log the hosted content
        /// </summary>
        /// <param name="mediaPlayerId">The media player id</param>
        /// <param name="contentName">Name of content to log</param>
        /// <param name="eventName">Name of event to log</param>
        /// <param name="eventDescription">Description of event to log</param>
        void LogContentEvent(int mediaPlayerId, string contentName, string eventName, string eventDescription);

        /// <summary>
        ///     Get the media log
        /// </summary>
        IEnumerable<IMedia> MediaLog { get; }

        /// <summary>
        ///     Get the maximum number of media contents that can be stored
        /// </summary>
        int MaxMediaStorage { get; }

        /// <summary>
        ///     Get whether the <see cref="MaxMediaStorage"/> limit has been reached
        /// </summary>
        bool MaxMediaStorageReached { get; }


        /// <summary>
        ///     Get the minimum media log size
        /// </summary>
        int MinimumMediaLogSize { get; }

        /// <summary>
        ///     Returns true if any primary overly media display is visible
        /// </summary>
        bool IsPrimaryOverlayVisible { get; }
    }
}
