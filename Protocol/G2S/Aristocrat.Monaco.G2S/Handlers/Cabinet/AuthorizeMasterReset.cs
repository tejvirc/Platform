namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Implementation of 'authorizeMasterReset' command of 'Cabinet' G2S class.
    /// </summary>
    public class AuthorizeMasterReset : ICommandHandler<cabinet, authorizeMasterReset>
    {
        private readonly IG2SEgm _egm;
        private readonly IMasterResetService _service;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizeMasterReset" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="service">Master reset service.</param>
        public AuthorizeMasterReset(
            IG2SEgm egm,
            IMasterResetService service)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, authorizeMasterReset> command)
        {
            var error = await Sanction.OwnerAndGuests<ICabinetDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var errorCode = string.Empty;

            if (!_service.HasMasterReset() || (_service.RequestId != 0 && _service.Status != MasterResetStatus.Pending))
            {
                errorCode = _service.Status == MasterResetStatus.InProcess ? "GTK_CBX003" : "GTK_CBX005";
            }

            return string.IsNullOrEmpty(errorCode) ? null : new Error(errorCode);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, authorizeMasterReset> command)
        {
            await _service.Handle(command);
        }
    }
}
