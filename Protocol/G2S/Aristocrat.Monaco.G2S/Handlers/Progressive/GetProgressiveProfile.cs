namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class GetProgressiveProfile : ICommandHandler<progressive, getProgressiveProfile>
    {
        private readonly ICommandBuilder<IProgressiveDevice, progressiveProfile> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetProgressiveProfile" />
        /// </summary>
        /// <param name="egm">IG2SEgm</param>
        /// <param name="commandBuilder">ICommandBuilder{TDevice,TCommand}</param>
        public GetProgressiveProfile(
            IG2SEgm egm,
            ICommandBuilder<IProgressiveDevice, progressiveProfile> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<progressive, getProgressiveProfile> command)
        {
            return await Sanction.OwnerAndGuests<IProgressiveDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<progressive, getProgressiveProfile> command)
        {
            var device = _egm.GetDevice<IProgressiveDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<progressiveProfile>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}