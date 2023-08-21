namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Extensions;
    using Gaming.Contracts;
    using Gaming.Contracts.Events.OperatorMenu;
    using Kernel;

    /// <summary>
    ///     supports get member, mandatory event report where operator menu game configuration changed
    /// </summary>
    public class EnabledGamesHashDataSource : IDisposableDataSource
    {
        public const string HashValueFieldName = "Hash_Value";
        private readonly IEventBus _eventBus;
        private readonly IAspGameProvider _aspGameProvider;
        private bool _disposed;

        public EnabledGamesHashDataSource(IAspGameProvider aspGameProvider, IEventBus eventBus)
        {
            _aspGameProvider = aspGameProvider ?? throw new ArgumentNullException(nameof(aspGameProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, OnGameConfigurationSaveComplete);
        }

        public IReadOnlyList<string> Members => new List<string> { HashValueFieldName };

        public string Name { get; } = "Hash_Of_Enabled_Games";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member)
        {
            return GetCustomHash(_aspGameProvider.GetEnabledGames()).ToString("X8");
        }

        public void SetMemberValue(string member, object value)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disposes of this IDisposable
        /// </summary>
        /// <param name="disposing">Whether the object is being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void OnGameConfigurationSaveComplete(GameConfigurationSaveCompleteEvent @event)
        {
            MemberValueChanged?.Invoke(this, this.GetMemberSnapshot(HashValueFieldName));
        }

        /// <summary>
        ///     Provide a custom hash in a combination of:
        ///     Denomination, Variation, Game Version, Minimum RTP
        /// </summary>
        /// <param name="denomsEnabled">List of game enabled combos</param>
        /// <returns>A final hash through the Exclusive OR from all individual hashes</returns>
        private static int GetCustomHash(IReadOnlyList<(IGameDetail game, IDenomination denom)> denomsEnabled)
        {
            var activeDenomHash = 0;
            var variationHash = 0;
            var minRtpHash = 0;
            var gameVersionHash = 0;
            foreach (var (game, denom) in denomsEnabled)
            {
                activeDenomHash ^= denom.Value.GetHashCode();
                variationHash ^= game.VariationId.GetHashCode();
                minRtpHash ^= game.MinimumPaybackPercent.GetHashCode();
                gameVersionHash ^= game.Version.GetHashCode();
            }

            return activeDenomHash ^ variationHash ^ minRtpHash ^ gameVersionHash;
        }
    }
}