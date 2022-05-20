namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Linq;

    /// <summary>
    ///     Manages G2S Sessions
    /// </summary>
    internal class SessionManager : ISessionManager
    {
        private bool _disposed;
        private SessionMap _sessions = new SessionMap();

        /// <inheritdoc />
        public Session Create(
            ISessionSink sink,
            long sessionId,
            ClassCommand command,
            SessionCallback callback,
            int retryCount,
            TimeSpan sessionTimeout)
        {
            var session = new Session(
                sink,
                sessionId,
                command,
                callback,
                retryCount,
                (int)sessionTimeout.TotalMilliseconds);

            _sessions.Add(command.SessionId, session);

            return session;
        }

        /// <inheritdoc />
        public Session GetById(long sessionId)
        {
            return _sessions[sessionId];
        }

        /// <inheritdoc />
        public void Complete(long sessionId)
        {
            _sessions.Remove(sessionId);
        }

        /// <inheritdoc />
        public void Complete(long sessionId, SessionStatus status)
        {
            _sessions[sessionId]?.Complete(status);
        }

        /// <inheritdoc />
        public void Complete(long sessionId, Error completeError)
        {
            var session = _sessions[sessionId];
            if (session == null)
            {
                return;
            }

            // **session.Request.GenerateError(_messageBuilder.DefaultNamespace);
            session.Request.GenerateError(Constants.DefaultSchema);
            session.Complete(SessionStatus.RequestError);
        }

        /// <inheritdoc />
        public void CompleteAll(int hostId, SessionStatus status)
        {
            var sessions = _sessions.Values.Where(s => s.Request.HostId == hostId).ToArray();

            foreach (var session in sessions)
            {
                session.Complete(status);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _sessions.Dispose();
            }

            _sessions = null;

            _disposed = true;
        }
    }
}