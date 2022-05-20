namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getEventHandlerStatus G2S message
    /// </summary>
    public class GetEventHandlerStatus : ICommandHandler<eventHandler, getEventHandlerStatus>
    {
        private readonly ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> _command;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetEventHandlerStatus" /> class.
        ///     Creates a new instance of the GetEventHandlerStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="command">Command builder.</param>
        public GetEventHandlerStatus(IG2SEgm egm, ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, getEventHandlerStatus> command)
        {
            return await Sanction.OwnerAndGuests<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, getEventHandlerStatus> command)
        {
            var eventHandlerDevice = _egm.GetDevice<IEventHandlerDevice>(command.IClass.deviceId);
            if (eventHandlerDevice == null)
            {
                return;
            }

            var response = command.GenerateResponse<eventHandlerStatus>();
            var status = response.Command;

            await _command.Build(eventHandlerDevice, status);
        }
    }
}