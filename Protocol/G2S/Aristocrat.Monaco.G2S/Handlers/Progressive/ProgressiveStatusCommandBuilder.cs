namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Kernel;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Gaming.Progressives;

    /// <inheritdoc />
    public class ProgressiveStatusCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveStatus>
    {
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveStatusCommandBuilder"></see>
        /// </summary>
        /// <param name="progressiveLevelProvider">This parameter provide the data related to progressive</param>
        /// <param name="protocolLinkedProgressiveAdapter">Adapter to access LinkedProgressiveLevel objects</param>
        public ProgressiveStatusCommandBuilder(
            IProgressiveLevelProvider progressiveLevelProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _progressiveLevelProvider = progressiveLevelProvider ?? throw new ArgumentNullException(nameof(progressiveLevelProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public Task Build(IProgressiveDevice device, progressiveStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.hostLocked = device.HostLocked;

            var levels = _progressiveLevelProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == device.Id && l.DeviceId != 0).ToList();


            var statuses = new List<levelStatus>();
            foreach(var level in levels)
            {
                IViewableLinkedProgressiveLevel linkedLevel = null;

                if (!string.IsNullOrEmpty(level?.AssignedProgressiveId?.AssignedProgressiveKey))
                {
                    _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(level?.AssignedProgressiveId?.AssignedProgressiveKey, out linkedLevel);
                }

                var levelId = linkedLevel?.ProtocolLevelId ?? level.LevelId;

                if (levelId == -1)
                {
                    continue;
                }

                if (statuses.Any(s => s.levelId == levelId))
                {
                    continue;
                }

                var status = new levelStatus();

                status.progId = level.ProgressiveId;
                status.levelId = levelId;
                status.progValueAmt = level.CurrentValue;
                status.progValueText = level.ProgressiveValueText;
                status.progValueSeq = level.ProgressiveValueSequence;

                statuses.Add(status);
            }
            command.levelStatus = statuses.ToArray();

            if (!(command.levelStatus?.Length > 0))
            {
                command.levelStatus = new[]
                {
                    new levelStatus
                    {
                        progId = 0,
                        levelId = 0,
                        progValueAmt = 0,
                        progValueText = string.Empty,
                        progValueSeq = 0
                    }
                };
            }

            command.configComplete = device.ConfigComplete;
            if (device.ConfigDateTime != default)
            {
                command.configDateTime = device.ConfigDateTime;
                command.configDateTimeSpecified = true;
            }

            return Task.CompletedTask;
        }
    }
}