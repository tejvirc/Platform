namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getCommConfigProfile G2S message
    /// </summary>
    public class GetCommConfigProfile : ICommandHandler<commConfig, getCommConfigProfile>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommConfigProfile" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        public GetCommConfigProfile(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, getCommConfigProfile> command)
        {
            return await Sanction.OwnerAndGuests<ICommConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, getCommConfigProfile> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<commConfigProfile>();
            response.Command.configurationId = device.ConfigurationId;
            response.Command.minLogEntries = device.MinLogEntries;
            response.Command.noResponseTimer = (int)device.NoResponseTimer.TotalMilliseconds;
            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;

            await Task.CompletedTask;
        }
    }
}