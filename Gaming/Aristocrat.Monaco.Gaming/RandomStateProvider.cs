namespace Aristocrat.Monaco.Gaming
{
    using Aristocrat.CryptoRng;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;

    public class RandomStateProvider : IRandomStateProvider
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;
        private const string KeyFieldName = @"Key";

        private readonly IPersistentStorageAccessor _accessor;

        public RandomStateProvider(IPersistentStorageManager storageManager)
        {
            var blockName = GetType().ToString();

            _accessor = storageManager.GetAccessor(Level, blockName);
        }

        public byte[] ReadKey()
        {
            return (byte[])_accessor[KeyFieldName];
        }

        public void WriteKey(byte[] key)
        {
            _accessor[KeyFieldName] = key;
        }
    }
}