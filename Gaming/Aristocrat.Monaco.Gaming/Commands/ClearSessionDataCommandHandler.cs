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
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storageManager;
        private readonly ILocalStorageProvider _localStorageProvider;

        public ClearSessionDataCommandHandler(
            IPropertiesManager properties,
            IPersistentStorageManager storageManager,
            ILocalStorageProvider localStorageProvider)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _localStorageProvider = localStorageProvider;
        }

        public void Handle(ClearSessionData command)
        {
            var games = _properties.GetValues<IGameDetail>(GamingConstants.Games);
            using var scope = _storageManager.ScopedTransaction();
            _localStorageProvider.ClearLocalData(StorageType.PlayerSession);

            foreach (var game in games)
            {
                foreach (var denom in game.ActiveDenominations)
                {
                    _localStorageProvider.ClearLocalData(StorageType.GamePlayerSession, game.Id, denom);
                }
            }

            scope.Complete();
            Logger.Debug("Cleared game session data");
        }
    }
}