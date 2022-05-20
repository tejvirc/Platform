namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
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

            var levels = _progressives.GetProgressiveLevels().Where(p => p.DeviceId == device.Id);

            command.levelStatus = (from level in levels
                select new levelStatus
                {
                    //progId = progressive.ProgId,
                    levelId = level.LevelId,
                    progValueAmt = level.CurrentValue,
                    //progValueText = level. ?? string.Empty,
                    //progValueSeq = level.Jackpot?.SequenceNumber ?? 0L
                }).ToArray();

            if (command.levelStatus == null)
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