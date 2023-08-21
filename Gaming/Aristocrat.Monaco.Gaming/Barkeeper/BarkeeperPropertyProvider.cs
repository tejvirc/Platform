namespace Aristocrat.Monaco.Gaming.Barkeeper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Newtonsoft.Json;

    /// <summary>
    ///     Handles storing and retrieving barkeeper settings.
    /// </summary>
    public sealed class BarkeeperPropertyProvider : IBarkeeperPropertyProvider, IPropertyProvider, IService
    {
        private const string BarkeeperRewardLevelsXml = @"Aristocrat.Monaco.Gaming.Barkeeper.BarkeeperRewardLevels.xml";
        private readonly IPersistentStorageAccessor _accessor;

        /// <summary>
        ///     Creates an instance of <see cref="BarkeeperPropertyProvider" />.
        /// </summary>
        public BarkeeperPropertyProvider()
        {
            var storageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            _accessor = GetAccessor(storageManager);
        }

        private string GetBarkeeperRewardLevels => (string)_accessor[GamingConstants.BarkeeperRewardLevels];

        private string GetLastCoinInReward => (string)_accessor[GamingConstants.BarkeeperActiveCoinInReward];

        private long GetCoinIn => (long)_accessor[GamingConstants.BarkeeperCoinIn];

        private long GetCashIn => (long)_accessor[GamingConstants.BarkeeperCashIn];

        private long GetRateOfPlayElapsedMilliseconds => (long)_accessor[GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds];

        /// <inheritdoc />
        public BarkeeperStorageData GetBarkeeperStorageData()
        {
            return new BarkeeperStorageData
            {
                BarkeeperRewardLevels = BarkeeperRewardLevelHelper.ToRewards(GetBarkeeperRewardLevels),
                ActiveCoinInLevel = JsonConvert.DeserializeObject<RewardLevel>(GetLastCoinInReward),
                CoinIn = GetCoinIn,
                CashIn = GetCashIn,
                RateOfPlayElapsedMilliseconds = GetRateOfPlayElapsedMilliseconds
            };
        }

        /// <inheritdoc />
        public void StoreBarkeeperData(IBarkeeperSettings data)
        {
            if (data is BarkeeperStorageData storageData)
            {
                UpdateStorageData(_accessor, storageData);

                return;
            }

            UpdateStorageData(_accessor, data);
        }

        public ICollection<KeyValuePair<string, object>> GetCollection => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>(
                GamingConstants.BarkeeperActiveCoinInReward,
                GetLastCoinInReward),
            new KeyValuePair<string, object>(
                GamingConstants.BarkeeperRewardLevels,
                GetBarkeeperRewardLevels),
            new KeyValuePair<string, object>(
                GamingConstants.BarkeeperCoinIn,
                GetCoinIn),
            new KeyValuePair<string, object>(
                GamingConstants.BarkeeperCashIn,
                GetCashIn),
            new KeyValuePair<string, object>(
                GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds,
                GetRateOfPlayElapsedMilliseconds)
        };

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case GamingConstants.BarkeeperRewardLevels:
                    return GetBarkeeperRewardLevels;
                case GamingConstants.BarkeeperActiveCoinInReward:
                    return GetLastCoinInReward;
                case GamingConstants.BarkeeperCoinIn:
                    return GetCoinIn;
                case GamingConstants.BarkeeperCashIn:
                    return GetCashIn;
                case GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds:
                    return GetRateOfPlayElapsedMilliseconds;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(propertyName)} is not implemented.");
            }
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            switch (propertyName)
            {
                case GamingConstants.BarkeeperActiveCoinInReward:
                    SetInStorage(
                        GamingConstants.BarkeeperActiveCoinInReward,
                        propertyValue);
                    break;
                case GamingConstants.BarkeeperRewardLevels:
                    SetInStorage(
                        GamingConstants.BarkeeperRewardLevels,
                        propertyValue);
                    break;
                case GamingConstants.BarkeeperCoinIn:
                    SetInStorage(GamingConstants.BarkeeperCoinIn, propertyValue, false);
                    break;
                case GamingConstants.BarkeeperCashIn:
                    SetInStorage(GamingConstants.BarkeeperCashIn, propertyValue, false);
                    break;
                case GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds:
                    SetInStorage(GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds, propertyValue, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(propertyName)} is not implemented.");
            }
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IBarkeeperPropertyProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void UpdateStorageData(IPersistentStorageAccessor accessor, BarkeeperStorageData data)
        {
            if (accessor is null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            if (data is null)
            {
                return;
            }

            using (var transaction = accessor.StartTransaction())
            {
                transaction[GamingConstants.BarkeeperActiveCoinInReward] =
                    JsonConvert.SerializeObject(data.ActiveCoinInLevel);
                transaction[GamingConstants.BarkeeperRewardLevels] =
                    data.BarkeeperRewardLevels.ToXml();
                transaction[GamingConstants.BarkeeperCoinIn] = data.CoinIn;
                transaction[GamingConstants.BarkeeperCashIn] = data.CashIn;
                transaction[GamingConstants.BarkeeperRateOfPlayElapsedMilliseconds] =
                    data.RateOfPlayElapsedMilliseconds;

                transaction.Commit();
            }
        }

        private void UpdateStorageData(IPersistentStorageAccessor accessor, IBarkeeperSettings settings)
        {
            if (accessor is null)
            {
                throw new ArgumentNullException(nameof(accessor));
            }

            if (settings is null)
            {
                return;
            }

            using (var transaction = accessor.StartTransaction())
            {
                var rewardLevels = new BarkeeperRewardLevels
                {
                    Enabled = settings.Enabled,
                    CoinInStrategy = new CoinInStrategy { CoinInRate = settings.CoinInRate },
                    RewardLevels = settings.CoinInRewardLevels.Append(settings.CashInRewardLevel).ToArray()
                };
                transaction[GamingConstants.BarkeeperRewardLevels] = rewardLevels.ToXml();
                transaction.Commit();
            }
        }

        private void SetInStorage(string propertyName, object propertyValue, bool serializeValue = true)
        {
            using (var transaction = _accessor.StartTransaction())
            {
                transaction[propertyName] = propertyValue is string stringValue
                    ? stringValue
                    : serializeValue
                        ? JsonConvert.SerializeObject(propertyValue)
                        : propertyValue;

                transaction.Commit();
            }
        }

        private IPersistentStorageAccessor GetAccessor(IPersistentStorageManager storageManager)
        {
            if (storageManager is null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            if (storageManager.BlockExists(Name))
            {
                return storageManager.GetBlock(Name);
            }

            var accessor = storageManager.CreateBlock(PersistenceLevel.Transient, Name, 1);

            var rewardLevelsXml = "";
            using (var stream = GetType().Assembly.GetManifestResourceStream(BarkeeperRewardLevelsXml))
            {
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    rewardLevelsXml = reader.ReadToEnd();
                }
            }

            UpdateStorageData(
                accessor,
                new BarkeeperStorageData
                {
                    BarkeeperRewardLevels = BarkeeperRewardLevelHelper.ToRewards(rewardLevelsXml),
                    ActiveCoinInLevel = null,
                    CoinIn = 0L,
                    CashIn = 0L,
                    RateOfPlayElapsedMilliseconds = 0L
                });

            return accessor;
        }
    }
}