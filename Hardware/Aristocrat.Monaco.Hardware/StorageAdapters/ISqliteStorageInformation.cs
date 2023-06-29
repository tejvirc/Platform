namespace Aristocrat.Monaco.Hardware.StorageAdapters
{
    public interface ISqliteStorageInformation
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; }
    }
}