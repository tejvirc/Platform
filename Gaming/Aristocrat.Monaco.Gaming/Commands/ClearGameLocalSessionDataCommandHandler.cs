namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using log4net;
    using Runtime.Client;

    public class ClearGameLocalSessionDataCommandHandler : ICommandHandler<ClearGameLocalSessionData>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly object Sync = new object();

        private readonly IGameStorage _gameStorage;
        private readonly IPersistentStorageManager _storageManager;

        public ClearGameLocalSessionDataCommandHandler(
            IGameStorage gameStorage,
            IPersistentStorageManager storageManager)
        {
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
        }

        public void Handle(ClearGameLocalSessionData command)
        {
            if (command.Details == null)
            {
                return;
            }

            lock (Sync)
            {
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
        }

        private void ClearData(IGameProfile game, long denom)
        {
            Logger.Debug($"Clearing local session data for {game.ThemeName} -- denom {denom} -- variation {game.VariationId}");
            _gameStorage.ClearAllValuesWithKeyName(game.Id, denom, StorageType.GameLocalSession.ToString());
        }
    }
}