namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Services;

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
                var linkedLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                    .Where(ll => ll.ProgressiveGroupId == device.ProgressiveId && ll.ProtocolName == ProtocolNames.G2S)
                    .Cast<LinkedProgressiveLevel>().ToList();
                var linkedLevelNames = linkedLevels.Select(ll => ll.LevelName).ToList();
                var levels = _progressiveProvider.GetProgressiveLevels().Where(
                    l => linkedLevelNames.Contains(l.AssignedProgressiveId.AssignedProgressiveKey)).ToList();

                if (!linkedLevels.Any())
                {
                    error = new Error(ErrorCode.G2S_PGX003);
                }
                else
                {
                    foreach (var hostLevel in command.Command.setLevelValue)
                    {
                        var linkedLevel = linkedLevels.Single(
                            ll => ll.LevelId == hostLevel.levelId && ll.ProgressiveGroupId == hostLevel.progId);
                        if (linkedLevel == null)
                        {
                            error = new Error(ErrorCode.G2S_PGX003);
                            break;
                        }

                        var progLevels = levels.Where(
                            l => l.AssignedProgressiveId.AssignedProgressiveKey == linkedLevel?.LevelName
                                 && (hostLevel.progValueAmt > l.MaximumValue ||
                                     hostLevel.progValueAmt < l.ResetValue));
                        if (progLevels.Any())
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

            var linkedLevels = _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevels()
                .Where(ll => ll.ProgressiveGroupId == device.ProgressiveId && ll.ProtocolName == ProtocolNames.G2S)
                .Cast<LinkedProgressiveLevel>().ToList();
            var linkedLevelNames = new HashSet<string>(linkedLevels.Select(ll => ll.LevelName));

            var levels = _progressiveProvider.GetProgressiveLevels().Where(
                l => linkedLevelNames.Contains(l.AssignedProgressiveId.AssignedProgressiveKey)).ToList();

            foreach (var hostLevel in command.Command.setLevelValue)
            {
                var linkedLevel = linkedLevels.Single(ll => ll.LevelId == hostLevel.levelId && ll.ProgressiveGroupId == hostLevel.progId);

                // if this update came out of order, just ignore it
                if (linkedLevel.ProgressiveValueSequence >= hostLevel.progValueSeq)
                {
                    continue;
                }

                _progressiveLevelManager.UpdateLinkedProgressiveLevels(
                    hostLevel.progId,
                    hostLevel.levelId,
                    hostLevel.progValueAmt.MillicentsToCents(),
                    hostLevel.progValueSeq,
                    hostLevel.progValueText,
                    linkedLevel.FlavorType);
            }

            device.ResetProgressiveInfoTimer();

            var response = command.GenerateResponse<progressiveValueAck>();
            await _progressiveValueAckCommandBuilder.Build(device, response.Command);
        }
    }
}