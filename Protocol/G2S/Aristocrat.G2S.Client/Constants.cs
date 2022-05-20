namespace Aristocrat.G2S.Client
{
    using System;

    /// <summary>
    ///     Container for constants that are used by the G2S client
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     The Standard G2S prefix
        /// </summary>
        public const string DefaultPrefix = @"G2S_";

        /// <summary>
        ///     The Aristocrat manufacturer prefix
        /// </summary>
        public const string ManufacturerPrefix = @"ATI";

        /// <summary>
        ///     A G2S EGM
        /// </summary>
        public const string EgmType = @"G2S_egm";

        /// <summary>
        ///     None in G2S
        /// </summary>
        public const string None = @"G2S_none";

        /// <summary>
        ///     The default G2S protocol
        /// </summary>
        public const string DefaultSchema = SchemaVersion.v21;

        /// <summary>
        ///     The Egm Host Index
        /// </summary>
        public const int EgmHostIndex = 0;

        /// <summary>
        ///     The Egm Host Identifier
        /// </summary>
        public const int EgmHostId = 0;

        /// <summary>
        ///     The default URI of an unregistered host
        /// </summary>
        public const string DefaultUrl = @"localhost";

        /// <summary>
        ///     The default retry count
        /// </summary>
        public const int DefaultRetryCount = 3;

        /// <summary>
        ///     Default Min Log Entries
        /// </summary>
        public const int DefaultMinLogEntries = 35;

        /// <summary>
        ///     The value used for an unset expiration date
        /// </summary>
        public const int ExpirationNotSet = -1;

        /// <summary>
        ///     The default time to live behavior
        /// </summary>
        public const TimeToLiveBehavior DefaultTimeToLiveBehavior = TimeToLiveBehavior.Strict;

        /// <summary>
        ///     Default Timeout
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = new TimeSpan(0, 0, 30);

        /// <summary>
        ///     Default No Response Timer
        /// </summary>
        public static readonly TimeSpan NoResponseTimer = new TimeSpan(0, 5, 0);
    }
}
