namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get verification status by transaction handler
    /// </summary>
    public class GetVerificationStatusByTransactionHandler : BaseGetVerificationStatusHandler,
        IParametersFuncHandler<GetVerificationStatusByTransactionArgs, VerificationStatusResult>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVerificationStatusByTransactionHandler" /> class.
        /// </summary>
        /// <param name="verificationRequestRepository">The verification request repository.</param>
        /// <param name="componentVerificationRepository">The component verification repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public GetVerificationStatusByTransactionHandler(
            IGatVerificationRequestRepository verificationRequestRepository,
            IGatComponentVerificationRepository componentVerificationRepository,
            IMonacoContextFactory contextFactory)
            : base(verificationRequestRepository, componentVerificationRepository, contextFactory)
        {
        }

        /// <summary>
        ///     Executes the specified get verification status arguments.
        /// </summary>
        /// <param name="parameter">The get verification status arguments.</param>
        /// <returns>Verification status</returns>
        public VerificationStatusResult Execute(GetVerificationStatusByTransactionArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var componentVerificationRequest = GetVerificationRequest(
                parameter.VerificationId,
                parameter.TransactionId,
                0);

            var result = CreateResult(componentVerificationRequest);
            return result;
        }
    }
}