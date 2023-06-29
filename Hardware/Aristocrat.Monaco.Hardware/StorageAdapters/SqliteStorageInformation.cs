namespace Aristocrat.Monaco.Hardware.StorageAdapters
{
    public class SqliteStorageInformation : ISqliteStorageInformation
    {
        public string Name { get; set; } = StorageConstants.DatabaseFileName;

        public string Password { get; set; } = StorageConstants.DatabasePassword;
    }
}