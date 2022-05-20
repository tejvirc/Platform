namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using ExpressMapper;

    /// <summary>
    ///     Implementation of 'getCommHostList' command of 'CommConfig' G2S class.
    /// </summary>
    public class GetCommHostList : ICommandHandler<commConfig, getCommHostList>
    {
        private readonly ICommHostListCommandBuilder _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetCommHostList" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">The command builder.</param>
        public GetCommHostList(IG2SEgm egm, ICommHostListCommandBuilder commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<commConfig, getCommHostList> command)
        {
            return await Sanction.OwnerAndGuests<ICommConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<commConfig, getCommHostList> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<commHostList>();

            var commandBuilderParameters =
                Mapper.Map<getCommHostList, CommHostListCommandBuilderParameters>(command.Command);

            await _commandBuilder.Build(device, response.Command, commandBuilderParameters);
        }
    }
}