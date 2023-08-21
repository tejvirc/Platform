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
    public class GetComponentList : ICommandHandler<gat, getComponentList>
    {
        private readonly IG2SEgm _egm;
        private readonly IGatService _gatService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetComponentList" /> class.
        ///     Constructs a new instance using an egm and the GAT service.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="gatService">An instance of IGatService.</param>
        public GetComponentList(IG2SEgm egm, IGatService gatService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, getComponentList> command)
        {
            return await Sanction.OwnerAndGuests<IGatDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, getComponentList> command)
        {
            var components = _gatService.GetComponentList();

            var response = command.GenerateResponse<componentList>();

            response.Command.component = components.Select(
                c => new component
                {
                    componentId = c.ComponentId,
                    componentType = c.Type.ToG2SComponentType(),
                    description = c.Description,
                    size = c.Size,
                    algorithm = _gatService.GetSupportedAlgorithms(c.Type).Select(
                        a => new algorithm
                        {
                            algorithmType = a.Type.ToG2SAlgorithmType(),
                            supportsOffsets = a.SupportsOffsets,
                            supportsSeed = a.SupportsSeed,
                            supportsSalt = a.SupportsSalt
                        }).ToArray()
                }).ToArray();

            await Task.CompletedTask;
        }
    }
}