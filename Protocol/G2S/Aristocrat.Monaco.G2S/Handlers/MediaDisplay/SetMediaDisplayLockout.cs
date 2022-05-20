namespace Aristocrat.Monaco.G2S.Handlers.MediaDisplay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handle the <see cref="setMediaDisplayLockOut" /> request from host
    /// </summary>
    public class SetMediaDisplayLockout : ICommandHandler<mediaDisplay, setMediaDisplayLockOut>
    {
        private readonly ICommandBuilder<IMediaDisplay, mediaDisplayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEgmStateManager _egmStateManager;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Constructor for <see cref="SetMediaDisplayLockout" />
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="egmStateManager">The <see cref="IEgmStateManager" /> object</param>
        /// <param name="eventLift">The <see cref="IEventLift" /> object</param>
        /// <param name="commandBuilder">A command builder for <see cref="mediaDisplayStatus" /> response</param>
        public SetMediaDisplayLockout(
            IG2SEgm egm,
            IEgmStateManager egmStateManager,
            IEventLift eventLift,
            ICommandBuilder<IMediaDisplay, mediaDisplayStatus> commandBuilder)

        {
            _egmStateManager = egmStateManager ?? throw new ArgumentNullException(nameof(egmStateManager));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<mediaDisplay, setMediaDisplayLockOut> command)
        {
            var device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

            var error = await Sanction.OnlyOwner<IMediaDisplay>(_egm, command);
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
        public async Task Handle(ClassCommand<mediaDisplay, setMediaDisplayLockOut> command)
        {
            var device = _egm.GetDevice<IMediaDisplay>(command.IClass.deviceId);

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

            var response = command.GenerateResponse<mediaDisplayStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void GenerateEvent(IMediaDisplay device)
        {
            var status = new mediaDisplayStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(
                device,
                device.HostLocked ? EventCode.IGT_MDE007 : EventCode.IGT_MDE008,
                device.DeviceList(status));
        }
    }
}
