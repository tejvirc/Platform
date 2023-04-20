namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    /// <summary>
    ///     Provides a mechanism to interact with a secondary storage device
    /// </summary>
    public interface ISecondaryStorageManager
    {
        /// <summary>
        ///     Sets root path for primary and secondary storage
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="secondary"></param>
        void SetPaths(string primary, string secondary);

        /// <summary>
        ///     Verifies secondary storage integrity.
        /// </summary>
        /// <returns>true if storage integrity check succeeds, else false</returns>
        bool Verify();

        /// <summary>
        ///     This API evaluates
        ///      - Raise lockup , If secondary storage is required and not connected
        ///      - Raise lockup , If secondary storage is NOT required, but is connected
        /// </summary>
        void VerifyConfiguration();

        /// <summary>
        ///     Gets the location to the mirror storage
        /// </summary>
        /// <param name="primaryStorageRoot">The storage to get the mirror location for</param>
        /// <returns>The location to the mirrored storage</returns>
        string GetMirrorPath(string primaryStorageRoot);
    }
}