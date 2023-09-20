namespace Aristocrat.Monaco.G2S.Handlers.Analytics
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getAnalyticsStatus G2S message
    /// </summary>
    public class GetAnalyticsStatus : ICommandHandler<analytics, getAnalyticsStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IAnalyticsDevice, analyticsStatus> _command;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetAnalyticsStatus" /> class.
        ///     Creates a new instance of the GetAnalyticsStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="command">Command builder.</param>
        public GetAnalyticsStatus(IG2SEgm egm, ICommandBuilder<IAnalyticsDevice, analyticsStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<analytics, getAnalyticsStatus> command)
        {
            return await Sanction.OwnerAndGuests<IAnalyticsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<analytics, getAnalyticsStatus> command)
        {
            var device = _egm.GetDevice<IAnalyticsDevice>(command.IClass.deviceId);
            if (device == null)
            {
                return;
            }

            var response = command.GenerateResponse<analyticsStatus>();
            var status = response.Command;
            await _command.Build(device, status);
        }
    }
}
