namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Runtime.Client;

    public class ClearSessionDataCommandHandler : ICommandHandler<ClearSessionData>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object Sync = new object();

        private readonly IGameStorage _gameStorage;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storageManager;

        public ClearSessionDataCommandHandler(
            IGameStorage gameStorage,
            IPropertiesManager properties,
            IPersistentStorageManager storageManager)
        {
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
        }

        public void Handle(ClearSessionData command)
        {
            var games = _properties.GetValues<IGameDetail>(GamingConstants.Games);

            lock (Sync)
            {
                using (var scope = _storageManager.ScopedTransaction())
                {
                    _gameStorage.ClearAllValuesWithKeyName(StorageType.PlayerSession.ToString());

                    foreach (var game in games)
                    {
                        foreach (var denom in game.ActiveDenominations)
                        {
                            _gameStorage.ClearAllValuesWithKeyName(
                                game.Id,
                                denom,
                                StorageType.GamePlayerSession.ToString());
                        }
                    }

                    scope.Complete();
                }
            }

            Logger.Debug("Cleared game session data");
        }
    }
}