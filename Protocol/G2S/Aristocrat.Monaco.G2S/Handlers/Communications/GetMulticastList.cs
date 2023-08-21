namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getMulticastList G2S message
    /// </summary>
    public class GetMulticastList : ICommandHandler<communications, getMcastList>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetMulticastList" /> class.
        ///     Creates a new instance of the GetMulticastList handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetMulticastList(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, getMcastList> command)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

            return await Task.FromResult(command.Validate(device, CommandRestrictions.RestrictedToOwnerAndGuests));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, getMcastList> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var response = command.GenerateResponse<mcastList>();

                response.Command.member = Enumerable.Empty<member>().ToArray();
            }

            await Task.CompletedTask;
        }
    }
}