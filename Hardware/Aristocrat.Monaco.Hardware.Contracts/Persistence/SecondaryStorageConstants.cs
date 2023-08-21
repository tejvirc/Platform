namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    /// <summary>
    ///     Constants.
    /// </summary>
    public static class SecondaryStorageConstants
    {
        /// <summary>
        ///     Path lookup of the data folder
        /// </summary>
        public const string MirrorRootKey = @"MirrorRoot";

        /// <summary>
        ///     Key used to check if secondary storage is required
        /// </summary>
        public const string SecondaryStorageRequired = "SecondaryStorageMedia.Required";
    }
}
