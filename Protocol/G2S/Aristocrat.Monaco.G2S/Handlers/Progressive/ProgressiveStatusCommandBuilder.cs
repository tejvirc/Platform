namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives;

    /// <inheritdoc />
    public class ProgressiveStatusCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveStatus>
    {
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveStatusCommandBuilder"></see>
        /// </summary>
        /// <param name="progressiveLevelProvider">This parameter provide the data related to progressive</param>
        /// <param name="protocolLinkedProgressiveAdapter">Adapter to access LinkedProgressiveLevel objects</param>
        public ProgressiveStatusCommandBuilder(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public Task Build(IProgressiveDevice device, progressiveStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.hostLocked = device.HostLocked;

            var linkedLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels().Where(l => l.ProgressiveGroupId == device.ProgressiveId).ToList();


            var statuses = new List<levelStatus>();
            foreach(var linkedLevel in linkedLevels)
            {
                if (statuses.Any(s => s.levelId == linkedLevel.LevelId))
                {
                    continue;
                }

                var status = new levelStatus();

                status.progId = linkedLevel.ProgressiveGroupId;
                status.levelId = linkedLevel.LevelId;
                status.progValueAmt = linkedLevel.Amount;
                status.progValueText = linkedLevel.ProgressiveValueText;
                status.progValueSeq = linkedLevel.ProgressiveValueSequence;

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