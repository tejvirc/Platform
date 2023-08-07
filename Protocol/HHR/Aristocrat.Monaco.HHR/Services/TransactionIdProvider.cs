namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Reflection;
    using System.Threading;
    using log4net;

    /// <inheritdoc />
    public class TransactionIdProvider : ITransactionIdProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private long _lastId;

        /// <summary>
        ///     Set the current value for transaction ID (the previous value given as an ID)
        /// </summary>
        /// <returns>Last transaction ID</returns>
        public void SetLastId(long seed)
        {
            Logger.Debug($"Transaction ID seeded with value: {seed}");
            _lastId = seed;
        }

        public long GetNextTransactionId()
        {
            if (_lastId == uint.MaxValue)
            {
                throw new ArgumentException("Couldn't get transaction ID - outside bounds");
            }

            return Interlocked.Increment(ref _lastId);
        }
    }
}