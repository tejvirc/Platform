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
    ///     Handles the <see cref="keepAlive" /> command
    /// </summary>
    public class KeepAlive : ICommandHandler<communications, keepAlive>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeepAlive" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public KeepAlive(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, keepAlive> command)
        {
            return await Sanction.OwnerAndGuests<ICommunicationsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, keepAlive> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                command.GenerateResponse<keepAliveAck>();

                await Task.CompletedTask;
            }
        }
    }
}