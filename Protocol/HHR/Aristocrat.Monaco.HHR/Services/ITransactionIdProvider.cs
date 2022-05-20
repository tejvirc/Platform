namespace Aristocrat.Monaco.Hhr.Services
{
    /// <summary>
    ///     Provides the next transaction Id for HHR messages
    /// </summary>
    public interface ITransactionIdProvider
    {
        /// <summary>
        ///     Set the seed value for transaction ID
        /// </summary>
        /// <returns>Last transaction ID</returns>
        void SetLastId(long seed);

        /// <summary>
        ///     Return the next transaction ID
        /// </summary>
        /// <returns>Next transaction ID</returns>
        long GetNextTransactionId();
    }
}