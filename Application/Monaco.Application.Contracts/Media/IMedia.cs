namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a piece of media
    /// </summary>
    public interface IMedia : ILogSequence
    {
        /// <summary>
        ///     Gets the transaction ID, unique to EGM.
        /// </summary>
        long TransactionId { get; }

        /// <summary>
        ///     Gets the media identifier
        /// </summary>
        long Id { get; }

        /// <summary>
        ///     Gets the address
        /// </summary>
        Uri Address { get; }

        /// <summary>
        ///     Get the access token
        /// </summary>
        long AccessToken { get; }

        /// <summary>
        ///     Gets the current media state
        /// </summary>
        MediaState State { get; set; }

        /// <summary>
        ///     Gets the <see cref="MediaContentError"/> exception code when the MediaState is <see cref="MediaState.Error"/>
        /// </summary>
        MediaContentError ExceptionCode { get; set; }

        /// <summary>
        ///     Gets the list of authorized events
        /// </summary>
        IEnumerable<string> AuthorizedEvents { get; }

        /// <summary>
        ///     Gets the list of authorized commands
        /// </summary>
        IEnumerable<string> AuthorizedCommands { get; }

        /// <summary>
        ///     Get the EMDI token for content-to-content comms
        /// </summary>
        long MdContentToken { get; }

        /// <summary>
        ///     Get whether EMDI connection is required for showing content
        /// </summary>
        bool EmdiConnectionRequired { get; }

        /// <summary>
        ///     When <see cref="EmdiConnectionRequired"/> is true, this value specifies
        ///     how long (in milliseconds) active content will wait for a reconnection
        ///     once a EMDI connection has closed
        /// </summary>
        int EmdiReconnectTimer { get; }

        /// <summary>
        ///     Get the timestamp for loadContent.
        /// </summary>
        DateTime? LoadTime { get; }

        /// <summary>
        ///     Get the timestamp for releaseContent.
        /// </summary>
        DateTime? ReleaseTime { get; }

        /// <summary>
        ///     Get whether media should display in native resolution (true)
        ///     or be scaled by the window (false).
        /// </summary>
        bool NativeResolution { get; }

        /// <summary>
        ///     Get the media player ID that this media content is associated to.
        /// </summary>
        int PlayerId { get; }

        /// <summary>
        /// Return whether it's in a finalized state.
        /// </summary>
        bool IsFinalized { get; }
    }
}
