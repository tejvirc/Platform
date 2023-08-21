namespace Aristocrat.Monaco.G2S.Handlers.Voucher
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
    ///     Handles the v21.setVoucherLockOut G2S message
    /// </summary>
    public class SetVoucherLockOut : ICommandHandler<voucher, setVoucherLockOut>
    {
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEgmStateManager _egmStateManager;
        private readonly IEventLift _eventLift;
        private readonly IVoucherDataService _voucherDataService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetVoucherLockOut" /> class.
        ///     Creates a new instance of the SetVoucherState handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="egmStateManager">EGM state manager.</param>
        /// <param name="eventLift">Event Lift.</param>
        /// <param name="voucherDataService">Voucher Data service.</param>
        /// <param name="commandBuilder">Voucher status command builder.</param>
        public SetVoucherLockOut(
            IG2SEgm egm,
            IEgmStateManager egmStateManager,
            IEventLift eventLift,
            IVoucherDataService voucherDataService,
            ICommandBuilder<IVoucherDevice, voucherStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _egmStateManager = egmStateManager ?? throw new ArgumentNullException(nameof(egmStateManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _voucherDataService = voucherDataService ?? throw new ArgumentNullException(nameof(voucherDataService));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<voucher, setVoucherLockOut> command)
        {
            return await Sanction.OnlyOwner<IVoucherDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<voucher, setVoucherLockOut> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IVoucherDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                device.HostLocked = command.Command.lockOut;

                if (command.Command.lockOut)
                {
                    _egmStateManager.Lock(
                        device,
                        EgmState.HostLocked,
                        () => command.Command.lockText,
                        TimeSpan.FromMilliseconds(command.Command.lockTimeOut),
                        () =>
                        {
                            device.HostLocked = false;
                            GenerateEvent(device);
                        });
                }
                else
                {
                    _egmStateManager.Enable(device, EgmState.HostLocked);
                }

                GenerateEvent(device);

                var response = command.GenerateResponse<voucherStatus>();
                var status = response.Command;
                status.configComplete = device.ConfigComplete;

                if (device.ConfigDateTime != default(DateTime))
                {
                    status.configDateTime = device.ConfigDateTime;
                    status.configDateTimeSpecified = true;
                }

                status.configurationId = device.ConfigurationId;
                status.egmEnabled = device.Enabled;
                status.hostEnabled = device.HostEnabled;

                var data = _voucherDataService.ReadVoucherData();
                if (data != null)
                {
                    status.validationListId = data.ListId;
                    status.validationIdsExpired =
                        data.ListTime.AddMilliseconds(device.ValueIdListLife) > DateTime.UtcNow
                            ? t_g2sBoolean.G2S_true
                            : t_g2sBoolean.G2S_false;

                    status.validationIdsRemaining = _voucherDataService.VoucherIdAvailable();
                }
            }

            await Task.CompletedTask;
        }

        private void GenerateEvent(IVoucherDevice device)
        {
            var status = new voucherStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostLocked ? EventCode.G2S_VCE009 : EventCode.G2S_VCE010,
                device.DeviceList(status));
        }
    }
}
