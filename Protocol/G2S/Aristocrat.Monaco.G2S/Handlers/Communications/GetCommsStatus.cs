namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getCommsStatus G2S message
    /// </summary>
    public class GetCommsStatus : ICommandHandler<communications, getCommsStatus>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, commsStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommsStatus" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        public GetCommsStatus(IG2SEgm egm, ICommandBuilder<ICommunicationsDevice, commsStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, getCommsStatus> command)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

            return await Task.FromResult(command.Validate(device, CommandRestrictions.RestrictedToOwnerAndGuests));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, getCommsStatus> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                var response = command.GenerateResponse<commsStatus>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}