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
    public class RunSpecialFunction : ICommandHandler<gat, runSpecialFunction>
    {
        private readonly IG2SEgm _egm;
        private readonly IGatService _gatService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RunSpecialFunction" /> class.
        ///     Constructs a new instance using an egm and the Gat service.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="gatService">An instance of IGatService.</param>
        public RunSpecialFunction(IG2SEgm egm, IGatService gatService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, runSpecialFunction> command)
        {
            var error = await Sanction.OnlyOwner<IGatDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            var functions = _gatService.GetSpecialFunctions();
            if (functions.All(f => f.Feature != command.Command.feature))
            {
                return new Error(ErrorCode.G2S_GAX002);
            }

            // TODO: Check for a parameter mismatch when supported by the library
            // return new Error(ErrorCode.G2S_GAX004);
            return await Task.FromResult<Error>(null);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, runSpecialFunction> command)
        {
            var response = command.GenerateResponse<specialFunctionResult>();

            var functions = _gatService.GetSpecialFunctions();
            var function = functions.FirstOrDefault(f => f.Feature == command.Command.feature);
            if (function == null)
            {
                return;
            }

            //// TODO G2S_GAX014Result No Longer Available
            // When we're actually ready to support this we should inject a list of feature/function handlers (one for each special function...
            //// TODO Event: G2S_GAE107 Special Function Executed
            // do something async...
            response.Command.verificationId = command.Command.verificationId;
            response.Command.transactionId = 1; // TODO: Get the transaction Id
            response.Command.feature = command.Command.feature;
            response.Command.gatExec = function.GatExec;
            response.Command.rawData = new rawData { Value = new byte[0] };

            await Task.CompletedTask;
        }
    }
}