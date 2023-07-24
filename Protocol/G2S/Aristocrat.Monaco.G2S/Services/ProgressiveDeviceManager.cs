namespace Aristocrat.Monaco.G2S.Services
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.Monaco.G2S.Options;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.G2S.Common.Events;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Client;
    using System;
    using Aristocrat.Monaco.Application.Contracts;

    public class ProgressiveDeviceManager : IProgressiveDeviceManager
    {
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;
        private readonly IDeviceFactory _deviceFactory;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly IProtocolLinkedProgressiveAdapter _linkedProgressiveAdapter;
        private readonly IProgressiveDeviceObserver _progressiveDeviceStateObserver;
        private readonly bool _g2sProgressivesEnabled;

        public ProgressiveDeviceManager(
            IG2SEgm egm,
            IEventLift eventLift,
            IGameProvider gameProvider,
            IDeviceFactory deviceFactory,
            IPropertiesManager propertiesManager,
            IProgressiveLevelProvider progressiveLevelProvider,
            IProtocolLinkedProgressiveAdapter linkedProgressiveAdapter,
            IProgressiveDeviceObserver progressiveDeviceStateObserver
        )
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _progressiveLevelProvider = progressiveLevelProvider ?? throw new ArgumentNullException(nameof(progressiveLevelProvider));
            _linkedProgressiveAdapter = linkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(linkedProgressiveAdapter));
            _progressiveDeviceStateObserver = progressiveDeviceStateObserver ?? throw new ArgumentNullException(nameof(progressiveDeviceStateObserver));
            _g2sProgressivesEnabled = (bool)propertiesManager.GetProperty(G2S.Constants.G2SProgressivesEnabled, false);
        }

        /// <inheritdoc />
        public Dictionary<int, int> VertexDeviceIds { get; set; } = new Dictionary<int, int>();

        /// <inheritdoc />
        public void OnConfiguredProgressives(bool fromConfig = false, bool fromBase = false)
        {
            if (!_g2sProgressivesEnabled)
            {
                return;
            }

            if (fromBase)
            {
                AddProgressiveDevices();
            }

            if (fromConfig)
            {
                ServiceManager.GetInstance().TryGetService<IEventBus>().Publish(new RestartProtocolEvent());
                AddProgressiveDevices();
            }

            var vertexLevelIds = (Dictionary<int, (int linkedGroupId, int linkedLevelId)>)_propertiesManager.GetProperty(GamingConstants.ProgressiveConfiguredLinkedLevelIds,
                new Dictionary<int, (int linkedGroupId, int linkedLevelId)>());
            VertexDeviceIds.Clear();
            foreach (var kvp in vertexLevelIds)
            {
                VertexDeviceIds[kvp.Key] = kvp.Value.linkedGroupId;
            }
        }

        public void AddProgressiveDevices(bool initialCreation = false)
        {
            var hosts = _propertiesManager.GetValues<IHost>(G2S.Constants.RegisteredHosts).ToList();
            var registeredGuests = hosts.Where(h => !h.IsEgm() && h.Registered).ToList();
            var progressiveHost = hosts.FirstOrDefault(h => h.IsProgressiveHost);
            var egmHost = _egm.GetHostById(Constants.EgmHostId) as IHost;
            var defaultNoProgInfoTimeout = (int)(progressiveHost?.ProgressiveHostOfflineTimerInterval.TotalMilliseconds ??
                                          G2S.Constants.DefaultNoProgInfoTimeout);

            //check which games (and thereby which progressives) are currently active. No need to create devices for disabled games. 
            var enabledGames = _gameProvider.GetEnabledGames().ToDictionary(g => g.Id, g => g.ActiveDenominations.ToList());
            var linkedProgressives = _linkedProgressiveAdapter.ViewLinkedProgressiveLevels().Where(ll => ll.ProtocolName == ProtocolNames.G2S);
            var enabledProgressives = _progressiveLevelProvider.GetProgressiveLevels().Where(l => enabledGames.ContainsKey(l.GameId) && l.Denomination.Intersect(enabledGames[l.GameId]).Any());

            //linked progressives levels where at least one of the progressives linked to it is enabled
            var progressiveDeviceIdsToCreate = linkedProgressives
                .Where(ll => enabledProgressives.Any(l => l.AssignedProgressiveId.AssignedProgressiveKey == ll.LevelName))
                .Select(ll => ll.ProgressiveGroupId)
                .Distinct().ToList();

            foreach (var deviceId in progressiveDeviceIdsToCreate)
            {
                if (!initialCreation && _egm.GetDevice<IProgressiveDevice>(deviceId) != null) continue;
                var device = _deviceFactory.Create(progressiveHost ?? egmHost, registeredGuests,
                    () => new ProgressiveDevice(deviceId, _progressiveDeviceStateObserver, _eventLift, defaultNoProgInfoTimeout));
            }

            if (initialCreation && !_egm.GetDevices<IProgressiveDevice>().Any())
            {
                _deviceFactory.Create(progressiveHost ?? egmHost, registeredGuests,
                    () => new ProgressiveDevice(0, _progressiveDeviceStateObserver, _eventLift, defaultNoProgInfoTimeout));
            }
        }
    }
}
