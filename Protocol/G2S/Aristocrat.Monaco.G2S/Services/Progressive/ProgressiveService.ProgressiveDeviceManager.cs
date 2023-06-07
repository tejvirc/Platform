namespace Aristocrat.Monaco.G2S.Services.Progressive
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

    public partial class ProgressiveService : IProgressiveDeviceManager
    {
        /// <inheritdoc />
        public IEngine Engine { private get; set; }

        /// <inheritdoc />
        public Dictionary<int, int> VertexDeviceIds { get; set; } = new Dictionary<int, int>();

        /// <inheritdoc />
        public List<int> VertexProgressiveIds { get; set; } = new List<int>();

        /// <inheritdoc />
        public Dictionary<int, int> DeviceProgIdMap { get; set; } = new Dictionary<int, int>();

        /// <inheritdoc />
        public void OnConfiguredProgressives(bool fromConfig = false, bool fromBase = false)
        {
            if (!_g2sProgressivesEnabled)
            {
                return;
            }

            if (fromBase)
            {
                Engine.AddProgressiveDevices(this);
            }

            var propertiesManager = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            if (fromConfig)
            {
                var vertexLevelIds = (Dictionary<string, int>)propertiesManager.GetProperty(GamingConstants.ProgressiveConfiguredLevelIds,
                    new Dictionary<string, int>());
                LevelIds.SetProgressiveLevelIds(vertexLevelIds);
                propertiesManager.SetProperty(G2S.Constants.VertexProgressiveLevelIds, vertexLevelIds);

                var vertexProgressiveIds = (List<int>)propertiesManager.GetProperty(GamingConstants.ProgressiveConfiguredIds, new List<int>());
                VertexProgressiveIds = vertexProgressiveIds;
                propertiesManager.SetProperty(G2S.Constants.VertexProgressiveIds, VertexProgressiveIds);

                ServiceManager.GetInstance().TryGetService<IEventBus>().Publish(new RestartProtocolEvent());
                Engine.AddProgressiveDevices(this);
            }

            var levelProvider = ServiceManager.GetInstance().GetService<IProgressiveLevelProvider>();
            var devices = _egm.GetDevices<IProgressiveDevice>();
            var denoms = new List<IDenomination>();
            var games = _gameProvider.GetAllGames();
            games.ToList().ForEach(g => g.Denominations.ToList().ForEach(d => denoms.Add(d)));

            for (var i = 0; i < devices.Count(); i++)
            {
                var device = (ProgressiveDevice)devices.ElementAt(i);
                var oldId = device.Id;
                var success = DeviceProgIdMap.TryGetValue(device.Id, out var newId);
                device.ProgressiveId = success ? newId : oldId;

                var progLevels = levelProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && l.DeviceId != 0);
                if (progLevels?.Any() == true)
                {
                    foreach (var level in progLevels)
                    {
                        if (LevelIds.VertexContainsLevel(level))
                        {
                            VertexDeviceIds.AddOrUpdate(level.DeviceId, device.Id);
                        }
                    }
                }
            }
        }
    }
}
