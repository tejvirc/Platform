namespace Aristocrat.Monaco.Mgam.Services.PlayerTracking
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Attributes;
    using Cabinet.Contracts;
    using Common;
    using Common.Events;
    using Gaming.Contracts.InfoBar;
    using Kernel;

    public class PlayerTracking : IService, IPlayerTracking, IDisposable
    {
        private readonly Guid _infoBarOwnershipKey = new Guid("{E1EF5A8C-960F-465D-881C-9BBA702EE3CC}");
        private readonly ILogger _logger;
        private readonly IAttributeManager _attributes;
        private readonly IEventBus _eventBus;

        private readonly object _lock = new object();

        private bool _disposed;
        private decimal _playerTrackingPoints;
        private int _penniesPerPoint;

        /// <summary>
        ///     The MGAM identification validator
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="attributes"><see cref="IAttributeManager"/></param>
        /// <param name="eventBus"><see cref="IEventBus"/></param>
        public PlayerTracking(
            ILogger<PlayerTracking> logger,
            IAttributeManager attributes,
            IEventBus eventBus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<AttributeChangedEvent>(this, HandleAttributeChanged);
            _eventBus.Subscribe<WagerPlacedEvent>(this, HandleWagerPlaced);
        }

        public string Name => typeof(PlayerTracking).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IPlayerTracking) };

        public bool IsSessionActive { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EndPlayerSession()
        {
            IsSessionActive = false;
            ClearPlayerTrackingData();
        }

        public void Initialize()
        {
        }

        public void StartPlayerSession(string playerName, int playerPoints, string promotionalInfo)
        {
            lock (_lock)
            {
                if (IsSessionActive)
                {
                    return;
                }

                IsSessionActive = true;

                _playerTrackingPoints = playerPoints;
                _attributes.Set(AttributeNames.PlayerTrackingPoints, (int)Math.Truncate(_playerTrackingPoints));

                ShowPlayerName(playerName);
                ShowPromotionalInfo(promotionalInfo);
                ShowPlayerPoints((int)Math.Truncate(_playerTrackingPoints));
            }
        }

        private void ClearPlayerTrackingData()
        {
            lock (_lock)
            {
                ClearPlayerName();
                ClearPlayerPoints();
                ClearPromotionalInfo();

                _playerTrackingPoints = 0;
                _penniesPerPoint = 0;
            }
        }

        private void Dispose(bool disposing)
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

        private void HandleWagerPlaced(WagerPlacedEvent e)
        {
            lock (_lock)
            {
                if (!IsSessionActive || _penniesPerPoint == 0)
                {
                    return;
                }

                _playerTrackingPoints += (decimal)e.PenniesWagered / _penniesPerPoint;
                _attributes.Set(AttributeNames.PlayerTrackingPoints, (int)Math.Truncate(_playerTrackingPoints));
            }
        }

        private void HandleAttributeChanged(AttributeChangedEvent e)
        {
            lock (_lock)
            {
                var attributeName = e.AttributeName;

                switch (attributeName)
                {
                    case AttributeNames.PenniesPerPoint:
                        LogAttributeChangedMessage(attributeName);

                        _penniesPerPoint = _attributes.Get(attributeName, 0);
                        break;
                    case AttributeNames.PlayerTrackingPoints:
                        LogAttributeChangedMessage(attributeName);

                        // This should only be updated here if it was modified in the server Attributes
                        // otherwise we may lose the decimal data we're tracking locally
                        if (e.UpdatedFromServer)
                        {
                            _playerTrackingPoints = _attributes.Get(attributeName, 0);
                        }

                        ShowPlayerPoints((int)Math.Truncate(_playerTrackingPoints));
                        break;
                    case AttributeNames.PointsPerEntry:
                        LogAttributeChangedMessage(attributeName);

                        // Nothing else to do with this right now.
                        break;
                    case AttributeNames.PromotionalInfo:
                        LogAttributeChangedMessage(attributeName);

                        ShowPromotionalInfo(_attributes.Get(attributeName, string.Empty));
                        break;
                    case AttributeNames.SweepstakesEntries:
                        LogAttributeChangedMessage(attributeName);

                        // Nothing else to do with this right now.
                        break;
                }
            }
        }

        private void LogAttributeChangedMessage(string attribute)
        {
            _logger.LogDebug($"Processing attribute-change for {attribute}");
        }

        private void ShowPlayerName(string name)
        {
            if (!IsSessionActive)
            {
                return;
            }

            var infoBarEvent = new InfoBarDisplayStaticMessageEvent(
                _infoBarOwnershipKey,
                $"Player: {name}",
                MgamConstants.PlayerMessageDefaultTextColor,
                MgamConstants.PlayerMessageDefaultBackgroundColor,
                InfoBarRegion.Left,
                DisplayRole.VBD);
            _eventBus.Publish(typeof(InfoBarDisplayStaticMessageEvent), infoBarEvent);
        }

        private void ShowPlayerPoints(int points)
        {
            if (!IsSessionActive)
            {
                return;
            }

            var infoBarEvent = new InfoBarDisplayStaticMessageEvent(
                _infoBarOwnershipKey,
                $"Points= {points:N0}",
                MgamConstants.PlayerMessageDefaultTextColor,
                MgamConstants.PlayerMessageDefaultBackgroundColor,
                InfoBarRegion.Right,
                DisplayRole.VBD);
            _eventBus.Publish(typeof(InfoBarDisplayStaticMessageEvent), infoBarEvent);
        }

        private void ShowPromotionalInfo(string message)
        {
            if (!IsSessionActive)
            {
                return;
            }

            var infoBarEvent = new InfoBarDisplayTransientMessageEvent(
                _infoBarOwnershipKey,
                message,
                MgamConstants.PlayerMessageDefaultTextColor,
                MgamConstants.PlayerMessageDefaultBackgroundColor,
                InfoBarRegion.Center,
                DisplayRole.VBD);
            _eventBus.Publish(typeof(InfoBarDisplayTransientMessageEvent), infoBarEvent);
        }

        private void ClearPlayerPoints() => _eventBus.Publish(new InfoBarClearMessageEvent(_infoBarOwnershipKey, DisplayRole.VBD, InfoBarRegion.Right));

        private void ClearPlayerName() => _eventBus.Publish(new InfoBarClearMessageEvent(_infoBarOwnershipKey, DisplayRole.VBD, InfoBarRegion.Left));

        private void ClearPromotionalInfo() => _eventBus.Publish(new InfoBarClearMessageEvent(_infoBarOwnershipKey, DisplayRole.VBD, InfoBarRegion.Center));
    }
}