namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Handles the v21.getMasterResetStatus G2S message
    /// </summary>
    public class GetMasterResetStatus : ICommandHandler<cabinet, getMasterResetStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IMasterResetService _masterResetService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetMasterResetStatus" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="masterResetService">An <see cref="IMasterResetService" /> instance.</param>
        public GetMasterResetStatus(IG2SEgm egm, IMasterResetService masterResetService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _masterResetService = masterResetService ?? throw new ArgumentNullException(nameof(masterResetService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, getMasterResetStatus> command)
        {
            var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

            return await Task.FromResult(command.Validate(device, CommandRestrictions.RestrictedToOwnerAndGuests));
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, getMasterResetStatus> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);

                var response = command.GenerateResponse<masterResetStatus>();

                await _masterResetService.BuildStatus(device, response.Command);

                if (string.IsNullOrEmpty(response.Command.masterResetStatus))
                {
                    command.Error.SetErrorCode("GTK_CBX005");
                }
            }
        }
    }
}