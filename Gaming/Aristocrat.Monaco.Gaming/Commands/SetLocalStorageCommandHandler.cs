namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Common.PerformanceCounters;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime.Client;

    [CounterDescription("Set Local Storage", PerformanceCounterType.AverageTimer32)]
    public class SetLocalStorageCommandHandler : ICommandHandler<SetLocalStorage>
    {
        private readonly IGameStorage _gameStorage;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;

        private readonly object _sync = new object();

        public SetLocalStorageCommandHandler(
            IPropertiesManager properties,
            IGameStorage gameStorage,
            IPersistentStorageManager persistentStorage)
        {
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Handle(SetLocalStorage command)
        {
            if (command.Values.Count <= 0)
            {
                return;
            }

            lock (_sync)
            {
                using (var scope = _persistentStorage.ScopedTransaction())
                {
                    foreach (var storageValue in command.Values)
                    {
                        switch (storageValue.Key)
                        {
                            case StorageType.GameLocalSession:
                                Update(
                                    _properties.GetValue(GamingConstants.SelectedGameId, 0),
                                    _properties.GetValue(GamingConstants.SelectedDenom, 0L),
                                    storageValue);
                                break;
                            case StorageType.LocalSession:
                            case StorageType.PlayerSession:
                                Update(storageValue);
                                break;
                            case StorageType.GamePlayerSession:
                                Update(
                                    _properties.GetValue(GamingConstants.SelectedGameId, 0),
                                    _properties.GetValue(GamingConstants.SelectedDenom, 0L),
                                    storageValue);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    scope.Complete();
                }
            }
        }

        private void Update(
            int gameId,
            long denomId,
            KeyValuePair<StorageType, IDictionary<string, string>> storageValue)
        {
            storageValue.Value.ToList().ForEach( x=>
                _gameStorage.SetValue(gameId, denomId, storageValue.Key.ToString(), x.Key, x.Value));
        }

        private void Update(KeyValuePair<StorageType, IDictionary<string, string>> storageValue)
        {
            storageValue.Value.ToList().ForEach(x =>
                _gameStorage.SetValue(storageValue.Key.ToString(), x.Key, x.Value));

        }
    }
}