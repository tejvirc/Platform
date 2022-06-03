namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using log4net;
    using PRNGLib;


    public class MysteryProgressiveProvider : IMysteryProgressiveProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
        public bool TryGetMagicNumber(IViewableProgressiveLevel progressiveLevel, out decimal magicNumber)
        {
            var index = GetProgressiveLevelKey(progressiveLevel);

            return _magicNumberIndex.TryGetValue(index, out magicNumber);
        }

        /// <inheritdoc />
        public bool CheckMysteryJackpot(IViewableProgressiveLevel progressiveLevel)
        {
            if (!TryGetMagicNumber(progressiveLevel, out var magicNumber))
            {
                return false;
            }

            return progressiveLevel.CurrentValue >= magicNumber;
        }

        private void Save(IViewableProgressiveLevel progressiveLevel, decimal magicNumber)
        {
            var index = GetProgressiveLevelKey(progressiveLevel);

            Logger.Debug($"Logging Magic number - {index} - {magicNumber}");
            _magicNumberIndex.AddOrUpdate(index, magicNumber, (_, _) => magicNumber);

            using var transaction = _saveBlock.Transaction();
            transaction.SetValue(_saveKey, _magicNumberIndex);
            transaction.Commit();
        }

        private string GetProgressiveLevelKey(IViewableProgressiveLevel progressiveLevel)
        {
            var key = progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey;

            if (progressiveLevel.AssignedProgressiveId == null ||
                progressiveLevel.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.None)
            {
                key = GenerateUniqueStandaloneProgressiveKey(progressiveLevel);
            }

            return key;
        }

        private string GenerateUniqueStandaloneProgressiveKey(IViewableProgressiveLevel progressiveLevel)
        {
            return SharedSapProviderExtensions.GeneratedLevelName(
                progressiveLevel.ProgressivePackName,
                progressiveLevel.ProgressivePackId,
                progressiveLevel.LevelName,
                progressiveLevel.Denomination.First(),
                progressiveLevel.BetOption);
        }
    }
}