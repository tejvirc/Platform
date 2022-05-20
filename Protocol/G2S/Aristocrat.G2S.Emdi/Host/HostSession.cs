namespace Aristocrat.G2S.Emdi.Host
{
    using System.Threading;

    /// <summary>
    /// Manages host session information
    /// </summary>
    public class HostSession
    {
        private int _sessionId = 1;

        /// <summary>
        ///     Gets the session Id
        /// </summary>
        public int SessionId => _sessionId;

        /// <summary>
        ///     Gets or sets the status
        /// </summary>
        public SessionState Status { get; set; }

        /// <summary>
        ///     Gets a value indicating whether or the session is valid
        /// </summary>
        public bool IsValid => Status == SessionState.Valid;

        /// <summary>
        ///     Increments the session Id
        /// </summary>
        /// <returns></returns>
        public int Inc()
        {
            return Interlocked.Increment(ref _sessionId);
        }

        /// <summary>
        ///     Resets the session Id
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref _sessionId, 1);
            Status = SessionState.Invalid;
        }
    }
}