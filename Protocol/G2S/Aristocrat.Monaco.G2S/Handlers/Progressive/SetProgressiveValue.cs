﻿namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.G2S.Services.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Kernel;

    /// <inheritdoc />
    public class SetProgressiveValue : ICommandHandler<progressive, setProgressiveValue>
    {
        private readonly IG2SEgm _egm;
        private readonly IProgressiveLevelProvider _progressiveProvider;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveValueAck> _progressiveValueAckCommandBuilder;
        private readonly IProgressiveService _progressiveService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="egm">The G2S EGM</param>
        /// <param name="progressiveValueAckCommandBuilder">Progressive Value Ack Command Builder instance</param>
        /// <param name="progressiveProvider">Progressive provider instance</param>
        /// <param name="progressiveService">Progressive Service Instance</param>
        public SetProgressiveValue(IG2SEgm egm,
            ICommandBuilder<IProgressiveDevice, progressiveValueAck> progressiveValueAckCommandBuilder,
            IProgressiveLevelProvider progressiveProvider,
            IProgressiveService progressiveService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _progressiveValueAckCommandBuilder = progressiveValueAckCommandBuilder ??
                throw new ArgumentNullException(nameof(progressiveValueAckCommandBuilder));
            _progressiveService = progressiveService ?? throw new ArgumentNullException(nameof(progressiveService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, setProgressiveValue> command)
        {
            Error error = await Sanction.OnlyOwner<IProgressiveDevice>(_egm, command);
            if (error == null && command.IClass.deviceId > 0)
            {
                if (_progressiveService == null) return new Error(ErrorCode.G2S_APX999);

                var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);
                List<ProgressiveLevel> levels = _progressiveProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && (_progressiveService.VertexDeviceIds.TryGetValue(l.DeviceId, out int value) ? value : l.DeviceId) == device.Id).ToList();
                var lvl = levels.FirstOrDefault();

                foreach (var l in command.Command.setLevelValue)
                {
                    var levelId = _progressiveService.LevelIds.GetMonacoProgressiveLevelId(lvl.GameId, l.progId, l.levelId);
                    var progLevel = levels.Where(p => p.ProgressiveId == l.progId && p.LevelId == levelId).FirstOrDefault();
                    if (progLevel == null)
                    {
                        error = new Error(ErrorCode.G2S_PGX003);
                        break;
                    }
                    if (l.progValueAmt > progLevel.MaximumValue || l.progValueAmt < progLevel.InitialValue)
                    {
                        error = new Error(ErrorCode.G2S_PGX004);
                        break;
                    }
                }
            }

            return error;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<progressive, setProgressiveValue> command)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return;
            }
            if (_progressiveService == null) return;

            _progressiveService.OnReceiveProgressiveValueUpdate();

            List<ProgressiveLevel> levels = _progressiveProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && (_progressiveService.VertexDeviceIds.TryGetValue(l.DeviceId, out int value) ? value : l.DeviceId) == device.Id).ToList();
            var lvl = levels.FirstOrDefault();

            List<ProgressiveLevel> levelsToUpdate = new List<ProgressiveLevel>();
            foreach (var level in command.Command.setLevelValue)
            {
                long progValueSeq = level.progValueSeq;
                if (_progressiveService.ProgressiveValues != null)
                {
                    foreach (var key in _progressiveService.ProgressiveValues.Keys)
                    {
                        if (_progressiveService.ProgressiveValues[key].ProgressiveValueSequence >= progValueSeq)
                        {
                            progValueSeq = _progressiveService.ProgressiveValues[key].ProgressiveValueSequence + 1;
                        }
                    }
                }

                var progLevel = levels.FirstOrDefault(p => p.ProgressiveId == level.progId && p.LevelId ==
                                    _progressiveService.LevelIds.GetMonacoProgressiveLevelId(lvl.GameId, level.progId, level.levelId));
                progLevel.CurrentValue = level.progValueAmt;

                ProgressiveValue newValue = new ProgressiveValue();
                newValue.LevelId = level.levelId - 1;
                newValue.ProgressiveId = level.progId;
                newValue.ProgressiveValueAmount = level.progValueAmt;
                newValue.ProgressiveValueText = level.progValueText ?? string.Empty;
                newValue.ProgressiveValueSequence = level.progValueSeq;


                if (_progressiveService.ProgressiveValues.ContainsKey($"{progLevel.ProgressiveId}|{progLevel.LevelId}"))
                {
                    _progressiveService.ProgressiveValues[$"{progLevel.ProgressiveId}|{progLevel.LevelId}"] = newValue;
                }
                else
                {
                    _progressiveService.ProgressiveValues.Add($"{progLevel.ProgressiveId}|{progLevel.LevelId}", newValue);
                }

                levelsToUpdate.Add(progLevel);
            }

            foreach (var level in levelsToUpdate)
            {
                _progressiveProvider.UpdateProgressiveLevels(level.ProgressivePackName, level.GameId,
                    level.Denomination.FirstOrDefault(), new List<ProgressiveLevel> { level });

                _progressiveService.UpdateLinkedProgressiveLevels(level.ProgressiveId, level.LevelId, level.CurrentValue.MillicentsToCents());
            }

            var response = command.GenerateResponse<progressiveValueAck>();
            await _progressiveValueAckCommandBuilder.Build(device, response.Command);
        }
    }
}