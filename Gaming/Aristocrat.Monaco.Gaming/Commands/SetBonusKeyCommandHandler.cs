namespace Aristocrat.Monaco.Gaming.Commands
{
    using Contracts;
    using Kernel;
    using System;

    /// <summary>
    ///     SetBonusKey command handler
    /// </summary>
    public class SetBonusKeyCommandHandler : ICommandHandler<SetBonusKey>
    {
        private readonly IPropertiesManager _properties;
        private readonly IGameStorage _gameStorage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetBonusKeyCommandHandler" /> class.
        /// </summary>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        /// <param name="gameStorage">An <see cref="IGameStorage" /> instance.</param>
        public SetBonusKeyCommandHandler(IPropertiesManager properties, IGameStorage gameStorage)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameStorage = gameStorage ?? throw new ArgumentNullException(nameof(gameStorage));
        }

        /// <inheritdoc />
        public void Handle(SetBonusKey command)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            var valueName = command.PoolName + GamingConstants.BonusKey;

            _gameStorage.SetValue(gameId, denomId, valueName, command.Key);
        }
    }
}
