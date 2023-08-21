namespace Aristocrat.Monaco.Hardware.StorageAdapters
{
    using Contracts.Persistence;

    /// <summary>
    ///     Constants.
    /// </summary>
    internal static class StorageConstants
    {
        /// <summary>
        ///     Block name for last time that static level was cleared.
        /// </summary>
        public const string PersistenceStaticCleared = "PersistenceStaticCleared";

        /// <summary>
        ///     Block name for last time that critical level was cleared.
        /// </summary>
        public const string PersistenceCriticalCleared = "PersistenceCriticalCleared";

        /// <summary>
        ///     Block name for last time that transient level was cleared.
        /// </summary>
        public const string PersistenceTransientCleared = "PersistenceTransientCleared";

        /// <summary>
        ///     Path lookup of the data folder
        /// </summary>
        public const string MirrorRootKey = SecondaryStorageConstants.MirrorRootKey;

        /// <summary>
        ///     Database file name
        /// </summary>
        public const string DatabaseFileName = "NVRam.sqlite";

        /// <summary>
        ///     Database file name extension
        /// </summary>
        public const string DatabasePassword = "tk7tjBLQ8GpySFNZTHYD";
    }
}
