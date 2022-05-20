namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Kernel;
    using Services;

    /// <summary>
    ///     Implementation of 'cancelMasterReset' command of 'Cabinet' G2S class.
    /// </summary>
    public class CancelMasterReset : ICommandHandler<cabinet, cancelMasterReset>
    {
        private readonly IEventBus _bus;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IMasterResetService _masterResetService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CancelMasterReset" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="bus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="masterResetService">Master reset service.</param>
        public CancelMasterReset(
            IG2SEgm egm,
            IEventLift eventLift,
            IEventBus bus,
            IMasterResetService masterResetService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _masterResetService = masterResetService ?? throw new ArgumentNullException(nameof(masterResetService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, cancelMasterReset> command)
        {
            var error = await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var errorCode = string.Empty;
            if (!_masterResetService.HasMasterReset() ||
                (_masterResetService.RequestId != 0 && _masterResetService.Status != MasterResetStatus.Pending &&
                _masterResetService.Status != MasterResetStatus.Authorized))
            {
                errorCode = "GTK_CBX005";
            }

            return !string.IsNullOrEmpty(errorCode) ? new Error(errorCode) : null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, cancelMasterReset> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);

                if (_masterResetService.RequestId != 0)
                {
                    _masterResetService.Status = MasterResetStatus.Canceled;
                }

                _eventLift.Report(device, EventCode.GTK_CBE009);

                var response = command.GenerateResponse<masterResetStatus>();
                await _masterResetService.BuildStatus(device, response.Command);

                _bus.Publish(new MasterResetCancelEvent());
            }
        }
    }
}
