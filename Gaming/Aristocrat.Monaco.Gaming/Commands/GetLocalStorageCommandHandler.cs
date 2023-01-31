namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Kernel;
    using Runtime.Client;

    public class GetLocalStorageCommandHandler : ICommandHandler<GetLocalStorage>
    {
        private readonly IGameStorage _gameStorage;
        private readonly IPropertiesManager _properties;

        public GetLocalStorageCommandHandler(IPropertiesManager properties, IGameStorage gameStorage)
        {
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Handle(GetLocalStorage command)
        {
            command.Values = new Dictionary<StorageType, IDictionary<string, string>>();

            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            command.Values.Add(
                StorageType.GameLocalSession,
                (IDictionary<string, string>)_gameStorage.GetValue<Dictionary<string, string>>(gameId, denomId, StorageType.GameLocalSession.ToString())
                ??
                new Dictionary<string, string>());

            command.Values.Add(
                StorageType.LocalSession,
                (IDictionary<string, string>)_gameStorage.GetValue<Dictionary<string, string>>(StorageType.LocalSession.ToString()) ??
                new Dictionary<string, string>());

            command.Values.Add(
                StorageType.PlayerSession,
                (IDictionary<string, string>)_gameStorage.GetValue<Dictionary<string, string>>(StorageType.PlayerSession.ToString()) ??
                new Dictionary<string, string>());

            var value = _gameStorage.GetValue<Dictionary<string, string>>(
                gameId,
                denomId,
                StorageType.GamePlayerSession.ToString());

            if (value?.Any() ?? false)
            {
                command.Values.Add(StorageType.GamePlayerSession, (IDictionary<string, string>)value);
            }
        }
    }
}
