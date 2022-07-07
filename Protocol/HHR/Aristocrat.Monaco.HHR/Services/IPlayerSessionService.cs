namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Threading.Tasks;
    using Client.Messages;

    /// <summary>
    ///     Maintains player session and knows when to seek a new player ID from server by listening for credit
    ///     and balance events on the bus.
    /// </summary>
    public interface IPlayerSessionService
    {
        /// <summary>
        ///     Get the currently active player ID that should be used for communication with the server.
        /// </summary>
        Task<string> GetCurrentPlayerId(int timeoutMs = HhrConstants.PlayerIdFetchTimeoutMilliseconds);
    }
}