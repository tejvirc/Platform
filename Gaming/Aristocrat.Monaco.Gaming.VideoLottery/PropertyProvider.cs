namespace Aristocrat.Monaco.Gaming.VideoLottery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Lobby;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A <see cref="IPropertyProvider" /> implementation for the gaming layer
    /// </summary>
    public class PropertyProvider : IPropertyProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly Dictionary<string, Tuple<object, bool>> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyProvider" /> class.
        /// </summary>
        /// <param name="storageManager">the storage manager</param>
        public PropertyProvider(IPersistentStorageManager storageManager)
        {
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            var storageName = GetType().ToString();

            var blockExists = storageManager.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Transient, storageName, 1);

            _properties = new Dictionary<string, Tuple<object, bool>>
            {
                { LobbyConstants.RGTimeLimitsInMinutes, Tuple.Create((object)new[] { 15.0, 30.0, 45.0, 60.0 }, false) },
                { LobbyConstants.RGPlayBreaksInMinutes, Tuple.Create((object)new[] { 0.0, 0.0, 0.0, 0.0 }, false) },
                { GamingConstants.LobbyConfig, Tuple.Create((object)new LobbyConfiguration(), false) },
                {
                    LobbyConstants.LobbyIsTimeLimitDlgVisible,
                    Tuple.Create(InitFromStorage(LobbyConstants.LobbyIsTimeLimitDlgVisible), true)
                },
                {
                    LobbyConstants.LobbyShowTimeLimitDlgPending,
                    Tuple.Create(InitFromStorage(LobbyConstants.LobbyShowTimeLimitDlgPending), true)
                },
                {
                    LobbyConstants.LobbyPlayTimeRemainingInSeconds,
                    Tuple.Create(InitFromStorage(LobbyConstants.LobbyPlayTimeRemainingInSeconds), true)
                },
                {
                    LobbyConstants.LobbyPlayTimeElapsedInSeconds,
                    Tuple.Create(InitFromStorage(LobbyConstants.LobbyPlayTimeElapsedInSeconds), true)
                },
                { LobbyConstants.LobbyPlayTimeElapsedInSecondsOverride, Tuple.Create((object)null, false) },
                {
                    LobbyConstants.LobbyPlayTimeSessionCount,
                    Tuple.Create(InitFromStorage(LobbyConstants.LobbyPlayTimeSessionCount), true)
                },
                { LobbyConstants.LobbyPlayTimeSessionCountOverride, Tuple.Create((object)null, false) },
                { LobbyConstants.LobbyPlayTimeDialogTimeoutInSeconds, Tuple.Create((object)60, false) },
                {
                    LobbyConstants.ResponsibleGamingPlayBreakTimeoutInSeconds,
                    Tuple.Create(InitFromStorage(LobbyConstants.ResponsibleGamingPlayBreakTimeoutInSeconds), true)
                },
                {
                    LobbyConstants.ResponsibleGamingPlayBreakHit,
                    Tuple.Create(InitFromStorage(LobbyConstants.ResponsibleGamingPlayBreakHit), true)
                },
                { LobbyConstants.DefaultGameDisplayOrderByThemeId, Tuple.Create((object)new string[0], false) }
            };

            if (!blockExists)
            {
                // TODO
                // This is just weird, but because the storage block accessor is typed it will return the default value vs. a null
                // It renders the default passed in to GetProperty useless, since it returns the default type.
                SetProperty(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
                SetProperty(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            }
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            => new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.Item1;
            }

            var errorMessage = "Unknown game property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            // NOTE:  Not all properties are persisted
            if (value.Item2)
            {
                Logger.Debug(
                    $"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");
                _persistentStorageAccessor[propertyName] = propertyValue;
            }

            _properties[propertyName] = Tuple.Create(propertyValue, value.Item2);
        }

        private object InitFromStorage(string propertyName)
        {
            return _persistentStorageAccessor[propertyName];
        }
    }
}
