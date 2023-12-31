﻿namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SetProgressiveState : ICommandHandler<progressive, setProgressiveState>
    {
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initialize a instance of SetProgressiveState class
        /// </summary>
        /// <param name="egm">IG2SEgm</param>
        /// <param name="commandBuilder">An instance of <see cref="ICommandBuilder{TDevice,TCommand}" />.</param>
        public SetProgressiveState(
            IG2SEgm egm,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<progressive, setProgressiveState> command)
        {
            return await Sanction.OnlyOwner<IProgressiveDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<progressive, setProgressiveState> command)
        {
            var progressiveDevice = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);
            if (progressiveDevice == null)
            {
                return;
            }

            var progressiveState = command.Command;

            if (progressiveDevice.HostEnabled != progressiveState.enable)
            {
                progressiveDevice.DisableText = progressiveState.disableText;
                progressiveDevice.HostEnabled = progressiveState.enable;
            }

            var response = command.GenerateResponse<progressiveStatus>();
            var status = response.Command;

            await _commandBuilder.Build(progressiveDevice, status);
        }
    }
}