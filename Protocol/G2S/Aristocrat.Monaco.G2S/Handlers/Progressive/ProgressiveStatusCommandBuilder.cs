namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Kernel;
    using Gaming.Contracts.Progressives;

    /// <inheritdoc />
    public class ProgressiveStatusCommandBuilder : ICommandBuilder<IProgressiveDevice, progressiveStatus>
    {
        private readonly IProgressiveLevelProvider _progressives;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveStatusCommandBuilder"></see>
        /// </summary>
        /// <param name="progressiveProvider">This parameter provide the data related to progressive</param>
        public ProgressiveStatusCommandBuilder(IProgressiveLevelProvider progressiveProvider)
        {
            _progressives = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
        }

        /// <inheritdoc />
        public Task Build(IProgressiveDevice device, progressiveStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;
            command.hostLocked = device.HostLocked;

            var progService = ServiceManager.GetInstance().TryGetService<IProgressiveService>();
            if (progService == null)
            {
                return Task.CompletedTask;
            }

            List<ProgressiveLevel> levels = _progressives.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && (progService.VertexDeviceIds.TryGetValue(l.DeviceId, out int value) ? value : l.DeviceId) == device.Id).ToList();


            List<levelStatus> statuses = new List<levelStatus>();
            foreach(ProgressiveLevel level in levels)
            {
                var levelId = progService.LevelIds.GetVertexProgressiveLevelId(level.GameId, level.ProgressiveId, level.LevelId);
                if (levelId == -1)
                {
                    continue;
                }

                if (statuses.Where(s => s.levelId == levelId).Count() > 0)
                {
                    continue;
                }

                levelStatus status = new levelStatus();

                status.progId = level.ProgressiveId;
                status.levelId = levelId;
                status.progValueAmt = level.CurrentValue;

                ProgressiveValue progVal = progService.ProgressiveValues.ContainsKey($"{level.ProgressiveId}|{level.LevelId}") ?
                    progService.ProgressiveValues[$"{level.ProgressiveId}|{level.LevelId}"] : null;
                status.progValueText = progVal?.ProgressiveValueText ?? string.Empty;
                status.progValueSeq = progVal?.ProgressiveValueSequence ?? 0;

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
            if (device.ConfigDateTime != default(DateTime))
            {
                command.configDateTime = device.ConfigDateTime;
                command.configDateTimeSpecified = true;
            }

            return Task.CompletedTask;
        }
    }
}