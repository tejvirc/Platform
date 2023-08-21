namespace Aristocrat.Monaco.Application.Contracts
{
    /// <summary>
    ///     Definition of the INoteAcceptorMonitor interface.
    /// </summary>
    public interface INoteAcceptorMonitor
    {
        /// <summary>
        ///     Set the current account balance in credits.
        /// </summary>
        /// <param name="credits">Current credits</param>
        void SetCurrentCredits(long credits);
    }
}
