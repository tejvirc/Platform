namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Aristocrat.CryptoRng;
    using log4net;


    public class MysteryProgressiveProvider : IMysteryProgressiveProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!?.DeclaringType);
        private readonly Dictionary<string, long> _magicNumberSapIndex;
        private readonly IPersistentBlock _saveBlock;
        private readonly IRandom _prng;
        private readonly string _saveKey;

        public MysteryProgressiveProvider(
            IRandomFactory randomFactory,
            IPersistenceProvider persistenceProvider)
        {
            _prng = randomFactory?.Create(RandomType.Gaming) ?? throw new ArgumentNullException(nameof(randomFactory));

            _saveKey = nameof(MysteryProgressiveProvider);

            _saveBlock = persistenceProvider?.GetOrCreateBlock(_saveKey, PersistenceLevel.Critical) ??
                         throw new ArgumentNullException(nameof(persistenceProvider));

            _magicNumberSapIndex = _saveBlock.GetOrCreateValue<Dictionary<string, long>>(_saveKey);
        }

        /// <inheritdoc />
        public long GenerateMagicNumber(IViewableProgressiveLevel progressiveLevel)
        {
            // When initially setting progressiveLevel CurrentValue == ResetValue
            // After applying overflow after a progressive win, then CurrentValue > ResetValue
            ulong randomNumber;
            var range = progressiveLevel.MaximumValue - progressiveLevel.CurrentValue;

            if (range <= 0)
            {
                // For cases where progressive overflow makes the CurrentValue equal to or more than progressive ceiling.
                // Set random number to 0 so that magic number becomes progressive ceiling.
                randomNumber = 0;
            }
            else
            {
                randomNumber = _prng.GetValue((ulong)range);
            }

            var magicNumber = (long)randomNumber + progressiveLevel.CurrentValue;

            Save(progressiveLevel, magicNumber);

            return magicNumber;
        }

        /// <inheritdoc />
        public bool TryGetMagicNumber(IViewableProgressiveLevel progressiveLevel, out long magicNumber)
        {
            var index = GetProgressiveLevelKey(progressiveLevel);

            return _magicNumberSapIndex.TryGetValue(index, out magicNumber);
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

        private void Save(IViewableProgressiveLevel progressiveLevel, long magicNumber)
        {
            var index = GetProgressiveLevelKey(progressiveLevel);

            Logger.Debug($"Logging Magic number - {index} - {magicNumber}");

            _magicNumberSapIndex[index] = magicNumber;

            using var transaction = _saveBlock.Transaction();
            transaction.SetValue(_saveKey, _magicNumberSapIndex);
            transaction.Commit();
        }

        private string GetProgressiveLevelKey(IViewableProgressiveLevel progressiveLevel) =>
            IsStandaloneProgressive(progressiveLevel)
                ? GenerateUniqueStandaloneProgressiveKey(progressiveLevel)
                : progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey;


        private string GenerateUniqueStandaloneProgressiveKey(IViewableProgressiveLevel progressiveLevel)
        {
            var progressiveLevelName = SharedSapProviderExtensions.GeneratedLevelName(
                progressiveLevel.ProgressivePackName,
                progressiveLevel.ProgressivePackId,
                progressiveLevel.LevelName,
                progressiveLevel.Denomination.First(),
                progressiveLevel.BetOption
            );

            return $"{progressiveLevel.GameId} - {progressiveLevelName}";
        }

        private bool IsStandaloneProgressive(IViewableProgressiveLevel progressiveLevel) =>
            progressiveLevel.AssignedProgressiveId == null ||
            progressiveLevel.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.None;
    }
}