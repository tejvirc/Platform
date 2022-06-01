namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using PRNGLib;


    public class MysteryProgressiveProvider : IMysteryProgressiveProvider
    {
        private readonly ConcurrentDictionary<string, decimal> _magicNumberIndex;
        private readonly IPersistentBlock _saveBlock;
        private readonly IPRNG _prng;
        private readonly string _saveKey;

        public MysteryProgressiveProvider(
            IRandomFactory randomFactory,
            IPersistenceProvider persistenceProvider
        )
        {
            _prng = randomFactory?.Create(RandomType.Gaming) ?? throw new ArgumentNullException(nameof(randomFactory));

            _saveKey = nameof(MysteryProgressiveProvider);

            _saveBlock = persistenceProvider?.GetOrCreateBlock(_saveKey, PersistenceLevel.Static) ??
                         throw new ArgumentNullException(nameof(persistenceProvider));

            _magicNumberIndex = _saveBlock.GetOrCreateValue<ConcurrentDictionary<string, decimal>>(_saveKey);
        }

        /// <inheritdoc />
        public decimal GenerateMagicNumber(IViewableProgressiveLevel progressiveLevel)
        {
            var randomNumber = _prng.GetValue((ulong)( progressiveLevel.MaximumValue - progressiveLevel.ResetValue ));
            var magicNumber = randomNumber + (ulong)progressiveLevel.ResetValue;

            Save(progressiveLevel, magicNumber);

            return magicNumber;
        }

        /// <inheritdoc />
        public bool GetMagicNumber(IViewableProgressiveLevel progressiveLevel, out decimal magicNumber)
        {
            var index = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey;

            return _magicNumberIndex.TryGetValue(index, out magicNumber);
        }

        /// <inheritdoc />
        public bool CheckMysteryJackpot(IViewableProgressiveLevel progressiveLevel)
        {
            if (!GetMagicNumber(progressiveLevel, out var magicNumber))
            {
                return false;
            }

            return progressiveLevel.CurrentValue >= magicNumber;
        }

        private void Save(IViewableProgressiveLevel progressiveLevel, decimal magicNumber)
        {
            var index = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey;
            _magicNumberIndex.AddOrUpdate(index, magicNumber, (_, _) => magicNumber);

            using var transaction = _saveBlock.Transaction();
            transaction.SetValue(_saveKey, _magicNumberIndex);
            transaction.Commit();
        }
    }
}