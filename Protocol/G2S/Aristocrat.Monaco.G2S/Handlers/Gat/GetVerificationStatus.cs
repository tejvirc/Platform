namespace Aristocrat.Monaco.G2S.Handlers.Gat
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.GAT.CommandHandlers;

    /// <summary>
    ///     Defines a new instance of an ICommandHandler.
    /// </summary>
    public class GetVerificationStatus : ICommandHandler<gat, getVerificationStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IGatService _gatService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVerificationStatus" /> class.
        ///     Constructs a new instance using an egm.
        /// </summary>
        /// <param name="egm">An instance of an IG2SEgm.</param>
        /// <param name="gatService">An instance of IGatService.</param>
        public GetVerificationStatus(IG2SEgm egm, IGatService gatService)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gatService = gatService ?? throw new ArgumentNullException(nameof(gatService));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gat, getVerificationStatus> command)
        {
            var error = await Sanction.OwnerAndGuests<IGatDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            if (!_gatService.HasVerificationId(command.Command.verificationId))
            {
                return new Error(ErrorCode.G2S_GAX005);
            }

            if (!_gatService.HasTransactionId(command.Command.transactionId))
            {
                return new Error(ErrorCode.G2S_GAX012);
            }

            return await Task.FromResult<Error>(null);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gat, getVerificationStatus> command)
        {
            //// TODO:  G2S_GAX012 transactionId Mismatch ?? command references a verification request that is not associated with the device identified in the class-level element of the command
            var args = new GetVerificationStatusByTransactionArgs(
                command.Command.transactionId,
                command.Command.verificationId);
            var status = _gatService.GetVerificationStatus(args);

            var response = command.GenerateResponse<verificationStatus>();

            response.Command.transactionId = command.Command.transactionId;
            response.Command.verificationId = command.Command.verificationId;

            if (status.ComponentVerificationResults != null)
            {
                response.Command.componentStatus = status.ComponentVerificationResults.Select(
                    v => new componentStatus
                    {
                        componentId = v.ComponentId,
                        verifyState = v.State.ToG2SVerifyState()
                    }).ToArray();
            }
            else
            {
                response.Command.componentStatus = status.VerificationStatus.ComponentStatuses.Select(
                    v => new componentStatus
                    {
                        componentId = v.ComponentId,
                        verifyState = v.State.ToG2SVerifyState()
                    }).ToArray();
            }

            await Task.CompletedTask;
        }
    }
}