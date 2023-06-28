namespace Aristocrat.Monaco.G2S.Handlers.Progressive
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
    using Gaming.Contracts.Progressives.Linked;
    using Gaming.Progressives;

    /// <inheritdoc />
    public class SetProgressiveValue : ICommandHandler<progressive, setProgressiveValue>
    {
        private readonly IG2SEgm _egm;
        private readonly IProgressiveLevelProvider _progressiveProvider;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveValueAck> _progressiveValueAckCommandBuilder;
        private readonly IProgressiveLevelManager _progressiveLevelManager;
        private readonly IProgressiveDeviceManager _progressiveDeviceManager;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="egm">The G2S EGM</param>
        /// <param name="progressiveValueAckCommandBuilder">Progressive Value Ack Command Builder instance</param>
        /// <param name="progressiveProvider">Progressive provider instance</param>
        /// <param name="progressiveService">Progressive Service Instance</param>
        /// <param name="progressiveDeviceManager">Progressive Device Manager Instance</param>
        public SetProgressiveValue(IG2SEgm egm,
            ICommandBuilder<IProgressiveDevice, progressiveValueAck> progressiveValueAckCommandBuilder,
            IProgressiveLevelProvider progressiveProvider,
            IProgressiveLevelManager progressiveLevelManager,
            IProgressiveDeviceManager progressiveDeviceManager,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _progressiveValueAckCommandBuilder = progressiveValueAckCommandBuilder ??
                throw new ArgumentNullException(nameof(progressiveValueAckCommandBuilder));
            _progressiveLevelManager = progressiveLevelManager ?? throw new ArgumentNullException(nameof(progressiveLevelManager));
            _progressiveDeviceManager = progressiveDeviceManager ?? throw new ArgumentNullException(nameof(progressiveDeviceManager));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, setProgressiveValue> command)
        {
            var error = await Sanction.OnlyOwner<IProgressiveDevice>(_egm, command);
            if (error == null && command.IClass.deviceId > 0)
            {
                var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);
                var levels = _progressiveProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && (_progressiveDeviceManager.VertexDeviceIds.TryGetValue(l.DeviceId, out var value) ? value : l.DeviceId) == device.Id).ToList();
                var lvl = levels.FirstOrDefault();

                if (lvl == null || !_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels(
                        levels.Select(
                            l => l.AssignedProgressiveId.AssignedProgressiveKey),
                            out var linkedLevels))
                {
                    error = new Error(ErrorCode.G2S_PGX003);
                }
                else
                {
                    foreach (var l in command.Command.setLevelValue)
                    {
                        var linkedLevel = linkedLevels.FirstOrDefault(
                            ll => ll.ProtocolLevelId == l.levelId && ll.ProgressiveGroupId == l.progId);
                        var progLevel = levels.FirstOrDefault(p => p.ProgressiveId == l.progId && p.LevelId == linkedLevel?.LevelId);
                        if (progLevel == null)
                        {
                            error = new Error(ErrorCode.G2S_PGX003);
                            break;
                        }

                        if (l.progValueAmt > progLevel.MaximumValue || l.progValueAmt < progLevel.ResetValue)
                        {
                            error = new Error(ErrorCode.G2S_PGX004);
                            break;
                        }
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

            var levels = _progressiveProvider.GetProgressiveLevels().Where(l => l.ProgressiveId == device.ProgressiveId && (_progressiveDeviceManager.VertexDeviceIds.TryGetValue(l.DeviceId, out int value) ? value : l.DeviceId) == device.Id).ToList();
            var lvl = levels.FirstOrDefault();
            _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels(
                levels.Select(
                    l => l.AssignedProgressiveId.AssignedProgressiveKey),
                out var linkedLevels);

            foreach (var level in command.Command.setLevelValue)
            {
                var linkedLevel = linkedLevels.FirstOrDefault(
                    ll => ll.ProtocolLevelId == level.levelId && ll.ProgressiveGroupId == level.progId);
                var progLevel = levels.FirstOrDefault(p => p.ProgressiveId == level.progId && p.LevelId == linkedLevel.LevelId);

                if (progLevel.ProgressiveValueSequence >= level.progValueSeq)
                    continue;

                progLevel.CurrentValue = level.progValueAmt;
                progLevel.ProgressiveValueSequence = level.progValueSeq;
                progLevel.ProgressiveValueText = level.progValueText;

                _progressiveLevelManager.UpdateLinkedProgressiveLevels(progLevel.ProgressiveId, progLevel.LevelId, progLevel.GameId, linkedLevel.ProtocolLevelId, progLevel.CurrentValue.MillicentsToCents());
            }

            device.ResetProgInfoTimer();

            var response = command.GenerateResponse<progressiveValueAck>();
            await _progressiveValueAckCommandBuilder.Build(device, response.Command);
        }
    }
}