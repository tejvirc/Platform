namespace Aristocrat.Monaco.G2S.Handlers.Analytics
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
    public class SetTrackInterval : ICommandHandler<analytics, setTrackInterval>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IAnalyticsDevice, setTrackIntervalAck> _setTrackIntervalAckCommandBuilder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="egm">The G2S EGM</param>
        /// <param name="progressiveValueAckCommandBuilder">Progressive Value Ack Command Builder instance</param>
        /// <param name="progressiveProvider">Progressive provider instance</param>
        /// <param name="progressiveService">Progressive Service Instance</param>
        /// <param name="progressiveDeviceManager">Progressive Device Manager Instance</param>
        public SetTrackInterval(
            IG2SEgm egm,
            ICommandBuilder<IAnalyticsDevice, setTrackIntervalAck> setTrackIntervalAckCommandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _setTrackIntervalAckCommandBuilder = setTrackIntervalAckCommandBuilder ??
                                                 throw new ArgumentNullException(
                                                     nameof(setTrackIntervalAckCommandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<analytics, setTrackInterval> command)
        {
            var error = await Sanction.OnlyOwner<IAnalyticsDevice>(_egm, command);
            if (error == null && command.IClass.deviceId > 0)
            {
                if (command.Command.interval < 0)
                {
                    error = new Error(ErrorCode.ATI_ANX001);
                }
            }

            return error;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<analytics, setTrackInterval> command)
        {
            var device = _egm.GetDevice<IAnalyticsDevice>(command.IClass.deviceId);

            if (device == null)
            {
                return;
            }

            var action = command.Command.action;
            var category = command.Command.category;
            var interval = TimeSpan.FromMilliseconds(command.Command.interval);

            device.SetTrackInterval(action, category, interval);

            var response = command.GenerateResponse<setTrackIntervalAck>();
            await _setTrackIntervalAckCommandBuilder.Build(device, response.Command);
        }
    }
}