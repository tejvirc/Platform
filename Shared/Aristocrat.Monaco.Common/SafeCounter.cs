namespace Aristocrat.Monaco.Common
{
    /// <summary>
    ///     Implements thread-safe logic for incrementing counter and resetting on overflow.
    /// </summary>
    public sealed class SafeCounter
    {
        private readonly object _syncLock = new object();
        private int _counter;

        /// <summary>
        ///     Increments the counter and returns the next value
        /// </summary>
        /// <returns>Returns the next value in the sequence.</returns>
        public int Next()
        {
            lock (_syncLock)
            {
                _counter++;

                if (_counter < 1)
                {
                    _counter = 1;
                }

                return _counter;
            }
        }

        /// <summary>
        ///     Resets the counter to zero.
        /// </summary>
        public void Reset()
        {
            lock (_syncLock)
            {
                _counter = 0;
            }
        }
    }
}
