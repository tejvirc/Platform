namespace Aristocrat.Monaco.G2S.Handlers.Gat
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class GetSpecialFunctions : ICommandHandler<gat, getSpecialFunctions>
    {
        private readonly IG2SEgm _egm;
        private readonly IGatService _gatService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetSpecialFunctions" /> class.
        ///     Constructs a new instance using an egm.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="gatService">An instance of IGatService.</param>
        public GetSpecialFunctions(IG2SEgm egm, IGatService gatService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, getSpecialFunctions> command)
        {
            return await Sanction.OwnerAndGuests<IGatDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, getSpecialFunctions> command)
        {
            var response = command.GenerateResponse<specialFunctions>();

            var functions = _gatService.GetSpecialFunctions();

            response.Command.function = functions.Select(
                f => new function
                {
                    feature = f.Feature,
                    gatExec = f.GatExec,
                    parameter = new string[0] // TODO: This is missing from Gat Service library
                }).ToArray();

            await Task.CompletedTask;
        }
    }
}