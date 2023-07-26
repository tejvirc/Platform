namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.leaveMcast G2S message
    /// </summary>
    public class LeaveMCast : ICommandHandler<communications, leaveMcast>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, leaveMcastAck> _commandBuilder;
        private readonly IEventLift _eventLift;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LeaveMCast" /> class.
        ///     Creates a new instance of the LeaveMCast handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">A mechanism to raise a G2S event</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        public LeaveMCast(IG2SEgm egm, IEventLift eventLift, ICommandBuilder<ICommunicationsDevice, leaveMcastAck> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, leaveMcast> command)
        {
            return await Sanction.OnlyOwner<ICommunicationsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, leaveMcast> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                device.RemoveDeviceConnection(command.Command.multicastId);

                _eventLift.Report(device, EventCode.G2S_CME111);

                var response = command.GenerateResponse<leaveMcastAck>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}