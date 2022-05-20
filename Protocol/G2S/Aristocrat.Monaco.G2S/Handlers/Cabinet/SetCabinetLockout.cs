namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class SetCabinetLockout : ICommandHandler<cabinet, setCabinetLockOut>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEgmStateManager _egmStateManager;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetCabinetLockout" /> class.
        /// </summary>
        /// <param name="egm">An instance of <see cref="IG2SEgm" />.</param>
        /// <param name="egmStateManager">An instance of <see cref="IEgmStateManager" />.</param>
        /// <param name="commandBuilder">An instance of <see cref="ICommandBuilder{TDevice,TCommand}" />.</param>
        /// <param name="eventLift">An instance of <see cref="IEventLift" />.</param>
        public SetCabinetLockout(
            IG2SEgm egm,
            IEgmStateManager egmStateManager,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _egmStateManager = egmStateManager ?? throw new ArgumentNullException(nameof(egmStateManager));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<cabinet, setCabinetLockOut> command)
        {
            var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);

            var error = await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            // EGMs MUST NOT process this request for a disabled device if the lockOut attribute for the request is set to true
            if (command.Command.lockOut && (!device.Enabled || !device.HostEnabled))
            {
                return new Error(ErrorCode.G2S_APX006);
            }

            return null;
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<cabinet, setCabinetLockOut> command)
        {
            var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);

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

            device.HostLocked = command.Command.lockOut;

            GenerateEvent(device);

            var response = command.GenerateResponse<cabinetStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void GenerateEvent(ICabinetDevice device)
        {
            var status = new cabinetStatus();

            _commandBuilder.Build(device, status);

            _eventLift.Report(
                device,
                device.HostLocked ? EventCode.G2S_CBE009 : EventCode.G2S_CBE010,
                device.DeviceList(status));
        }
    }
}
