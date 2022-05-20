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
    ///     Implementation of 'commConfigModeStatus' command of 'CommConfig' G2S class.
    /// </summary>
    public class GetCommConfigModeStatus : ICommandHandler<commConfig, getCommConfigModeStatus>
    {
        private readonly ICommandBuilder<ICommConfigDevice, commConfigModeStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommConfigModeStatus" /> class using an egm and a commConfig status
        ///     command builder.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="commandBuilder">An instance of ICommandBuilder&lt;commConfig&gt;.</param>
        public GetCommConfigModeStatus(
            IG2SEgm egm,
            ICommandBuilder<ICommConfigDevice, commConfigModeStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, getCommConfigModeStatus> command)
        {
            return await Sanction.OwnerAndGuests<ICommConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, getCommConfigModeStatus> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);

            var response = command.GenerateResponse<commConfigModeStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}