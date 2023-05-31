namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using log4net;
    using Runtime.Client;

    public class ClearGameLocalSessionDataCommandHandler : ICommandHandler<ClearGameLocalSessionData>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPersistentStorageManager _storageManager;
        private readonly ILocalStorageProvider _localStorageProvider;

        public ClearGameLocalSessionDataCommandHandler(
            IPersistentStorageManager storageManager,
            ILocalStorageProvider localStorageProvider)
        {
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _localStorageProvider = localStorageProvider;
        }

        public void Handle(ClearGameLocalSessionData command)
        {
            if (command.Details == null)
            {
                return;
            }

            using var scope = _storageManager.ScopedTransaction();
            if (command.Denom != null)
            {
                // If a denom is specified, clear only that denom
                ClearData(command.Details, command.Denom.Value);
            }
            else
            {
                // If no denom is specified, clear all denoms for this game
                foreach (var denom in command.Details.Denominations)
                {
                    ClearData(command.Details, denom.Value);
                }
            }

            scope.Complete();
        }

        private void ClearData(IGameProfile game, long denom)
        {
            Logger.Debug($"Clearing local session data for {game.ThemeName} -- denom {denom} -- variation {game.VariationId}");
            _localStorageProvider.ClearLocalData(StorageType.GameLocalSession, game.Id, denom);
        }
    }
}