namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Progressives;

    public class GetProgressiveStatus : ICommandHandler<progressive, getProgressiveStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IProgressiveLevelProvider _progressives;
        private readonly ICommandBuilder<IProgressiveDevice, progressiveStatus> _progressiveStatusCommandBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetProgressiveStatus"/> class.
        ///     Creates a new instance of GetProgressiveStatus
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="progressiveStatusCommandBuilder">Progressive status Command builder instance</param>
        /// <param name="progressiveProvider">Progressive Provider instance.</param>
        public GetProgressiveStatus(
            IG2SEgm egm,
            ICommandBuilder<IProgressiveDevice, progressiveStatus> progressiveStatusCommandBuilder,
            IProgressiveLevelProvider progressiveProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _progressiveStatusCommandBuilder = progressiveStatusCommandBuilder ??
                throw new ArgumentNullException(nameof(progressiveStatusCommandBuilder));
            _progressives = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
        }

        /// <inheritdoc/>
        public async Task<Error> Verify(ClassCommand<progressive, getProgressiveStatus> command)
        {
            var error = await Sanction.OwnerAndGuests<IProgressiveDevice>(_egm, command);

            if (error == null && command.IClass.deviceId > 0)
            {
                if (_progressives.GetProgressiveLevels().All(p => p.DeviceId != command.IClass.deviceId))
                {
                    error = new Error(ErrorCode.G2S_PGX001);
                }
            }

            return error;
        }

        /// <inheritdoc/>
        public async Task Handle(ClassCommand<progressive, getProgressiveStatus> command)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return;
            }

            var response = command.GenerateResponse<progressiveStatus>();
            var status = response.Command;
            await _progressiveStatusCommandBuilder.Build(device, status);
        }
    }
}