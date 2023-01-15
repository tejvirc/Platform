namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Localization;
    using Common;
    using Contracts;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using HandlerMap =
        System.Collections.Generic.Dictionary<string, (System.Func<object> Getter, System.Func<object, bool> Setter)>;

    public class GameStatusDataSource : IDisposableDataSource, ITransaction
    {
        private static readonly Guid ProgressiveDisableGuid = new Guid("{7E711EFF-E1DB-4CB9-9B6B-62425A56F2E3}");
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameStatusProvider _gameStatusProvider;
        private readonly HandlerMap _handlers;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IEventBus _eventBus;

        private bool _disposed;

        public GameStatusDataSource(
            IGameStatusProvider gameStatusProvider,
            IEventBus eventBus,
            ISystemDisableManager systemDisableManager,
            IGamePlayState gamePlayState)
        {
            _gameStatusProvider = gameStatusProvider ?? throw new ArgumentNullException(nameof(gameStatusProvider));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _handlers = GetMembersMap();
            _eventBus.Subscribe<DacomGameStatusChangedEvent>(this, DacomGameStatusChangedEventHandler);
            _eventBus.Subscribe<GameEndedEvent>(this, GameEndedEventHandler);
        }

        private bool DisableRequestPending { get; set; }

        private GameEnableStatus Status { get; set; }

        private GameDisableReason Reason { get; set; }

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public string Name => "Game_Enable";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member) => _handlers[member].Getter();

        public void SetMemberValue(string member, object value) => _handlers[member].Setter(value);

        /// <inheritdoc />
        public void Begin(IReadOnlyList<string> dataMemberNames)
        {

        }

        public void Commit()
        {
            _gameStatusProvider.SetHostStatus(Status, Reason);
            if (Status == GameEnableStatus.EnableGame)
            {
                _systemDisableManager.Enable(ProgressiveDisableGuid);
                return;
            }

            if (!_gamePlayState.InGameRound)
            {
                _systemDisableManager.Disable(
                    ProgressiveDisableGuid,
                    Status == GameEnableStatus.DisableGameDisallowCollect
                        ? SystemDisablePriority.Immediate
                        : SystemDisablePriority.Normal,
                    ResourceKeys.ProgressiveDisable,
                    CultureProviderType.Player);
            }
            else
            {
                DisableRequestPending = true;
            }
        }

        public void RollBack()
        {
            Status = _gameStatusProvider.Status;
            Reason = _gameStatusProvider.Reason;
        }

        private void DacomGameStatusChangedEventHandler(DacomGameStatusChangedEvent theEvent)
        {
            var memberSnapshot = new Dictionary<string, object>
            {
                ["Status"] = theEvent.GameEnableStatus,
                ["Reason_for_Disabling"] = theEvent.GameDisableReason
            };

            MemberValueChanged?.Invoke(this, memberSnapshot);
        }

        private void GameEndedEventHandler(GameEndedEvent theEvent)
        {
            if (!DisableRequestPending)
            {
                return;
            }

            _systemDisableManager.Disable(
                ProgressiveDisableGuid,
                _gameStatusProvider.HostStatus == GameEnableStatus.DisableGameDisallowCollect
                    ? SystemDisablePriority.Immediate
                    : SystemDisablePriority.Normal,
                ResourceKeys.ProgressiveDisable,
                CultureProviderType.Operator);

            DisableRequestPending = false;
        }

        private HandlerMap GetMembersMap()
        {
            return new HandlerMap
            {
                { "Status", (() => _gameStatusProvider.Status, SetStatus) },
                { "Reason_for_Disabling", (() => _gameStatusProvider.Reason, SetReason) }
            };
        }

        private bool SetStatus(object value)
        {
            var (success, enumValue) = EnumParser.Parse<GameEnableStatus>(value);
            if (!success || !enumValue.HasValue) return false;

            Status = enumValue.Value;
            return true;
        }

        private bool SetReason(object value)
        {
            var (success, enumValue) = EnumParser.Parse<GameDisableReason>(value);
            if (!success || !enumValue.HasValue) return false;

            Reason = enumValue.Value;
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}