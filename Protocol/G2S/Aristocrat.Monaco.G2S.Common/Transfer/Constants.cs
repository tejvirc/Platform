namespace Aristocrat.Monaco.G2S.Common.Transfer
{
    using System;

    /// <summary>
    ///     Transfer Constants
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        ///     The default retry count
        /// </summary>
        public const int DefaultRetryCount = 3;

        /// <summary>
        ///     Initial delay for retries
        /// </summary>
        public static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(3);

        /// <summary>
        ///     Initial delay for retries
        /// </summary>
        public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);
    }
}