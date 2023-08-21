namespace Aristocrat.Monaco.Gaming
{
    using Contracts;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Responsible for storing/managing game related data
    /// </summary>
    public class GameStorageManager : StorageBase, IGameStorage
    {
        public GameStorageManager(IPersistentStorageManager persistentStorage)
            : base(persistentStorage, PersistenceLevel.Critical)
        {
        }
    }
}
