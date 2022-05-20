namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Contracts;
    using Extensions;
    using Gaming.Contracts.Events.OperatorMenu;
    using Kernel;

    /// <summary>
    ///     supports get member, report event where operator menu game configuration changed
    /// </summary>
    public class MultiGameDenomAttributeDataSource : IDataSource, IDisposable
    {
        public const string MultiGameDenomInfo = "Multi_Game_Denom_Info";
        public const string AccountingDenomValue = "Accounting_Denom_Value";
        private readonly IAspGameProvider _aspGameProvider;
        private readonly IEventBus _eventBus;
        private bool _disposed;

        public MultiGameDenomAttributeDataSource(IAspGameProvider aspGameProvider, IEventBus eventBus)
        {
            _aspGameProvider = aspGameProvider ?? throw new ArgumentNullException(nameof(aspGameProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<GameConfigurationSaveCompleteEvent>(this, OnGameConfigurationSaveComplete);
        }

        public IReadOnlyList<string> Members => new List<string> { MultiGameDenomInfo, AccountingDenomValue };

        public string Name { get; } = "Multi_Game_Denom";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member)
        {
            object result = null;
            var enabledGames = _aspGameProvider.GetEnabledGames();
            var uniqueActiveDenomsCount = enabledGames.Select(x => x.denom.Value).Distinct().Count();
            switch (member)
            {
                case MultiGameDenomInfo:
                    result = uniqueActiveDenomsCount > 1
                        ? AspConstants.DenomAttribute.MultiDenom
                        : AspConstants.DenomAttribute.SingleDenom;
                    break;
                case AccountingDenomValue:
                    result = uniqueActiveDenomsCount == 1
                        ? enabledGames.First().denom.Value.MillicentsToCents()
                        : AspConstants.AccountingDenomination;
                    break;
            }

            return result;
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

        protected void OnGameConfigurationSaveComplete(GameConfigurationSaveCompleteEvent @event)
        {
            MemberValueChanged?.Invoke(this, this.GetMemberSnapshot(new[] {MultiGameDenomInfo, AccountingDenomValue}));
        }
    }
}