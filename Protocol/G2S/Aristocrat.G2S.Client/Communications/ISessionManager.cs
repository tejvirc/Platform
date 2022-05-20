namespace Aristocrat.G2S.Client.Communications
{
    using System;

    /// <summary>
    ///     Provides a mechanism to manage sessions.
    /// </summary>
    public interface ISessionManager : IDisposable
    {
        /// <summary>
        ///     Creates a new Session.
        /// </summary>
        /// <param name="sink">An instance of session observer.</param>
        /// <param name="sessionId">Unique session identifier.</param>
        /// <param name="command">ClassCommand instance to be handled by the session.</param>
        /// <param name="callback">SessionCallback instance to be invoked with the response.</param>
        /// <param name="retryCount">Overrides the retry count for the endpoint.</param>
        /// <param name="sessionTimeout">Overrides the session timeout for the endpoint.</param>
        /// <returns>Session</returns>
        Session Create(
            ISessionSink sink,
            long sessionId,
            ClassCommand command,
            SessionCallback callback,
            int retryCount,
            TimeSpan sessionTimeout);

        /// <summary>
        ///     Gets a Session by it's identifier.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>Returns a Session if found or null.</returns>
        Session GetById(long sessionId);

        /// <summary>
        ///     Completes a Session.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        void Complete(long sessionId);

        /// <summary>
        ///     Completes a Session with the specified status.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="status">SessionStatus used to complete the session.</param>
        void Complete(long sessionId, SessionStatus status);

        /// <summary>
        ///     Completes a Session with the specified error.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="completeError">Error used to complete the session</param>
        void Complete(long sessionId, Error completeError);

        /// <summary>
        ///     Completes all Sessions with the specified status.
        /// </summary>
        /// <param name="hostId">The host identifier.</param>
        /// <param name="status">SessionStatus used to complete the session.</param>
        void CompleteAll(int hostId, SessionStatus status);
    }
}