namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get verification status by device handler
    /// </summary>
    public class GetVerificationStatusByDeviceHandler : BaseGetVerificationStatusHandler,
        IParametersFuncHandler<GetVerificationStatusByDeviceArgs, VerificationStatusResult>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVerificationStatusByDeviceHandler" /> class.
        /// </summary>
        /// <param name="verificationRequestRepository">The verification request repository.</param>
        /// <param name="componentVerificationRepository">The component verification repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public GetVerificationStatusByDeviceHandler(
            IGatVerificationRequestRepository verificationRequestRepository,
            IGatComponentVerificationRepository componentVerificationRepository,
            IMonacoContextFactory contextFactory)
            : base(verificationRequestRepository, componentVerificationRepository, contextFactory)
        {
        }

        /// <summary>
        ///     Executes the specified get verification status by device arguments.
        /// </summary>
        /// <param name="parameter">The get verification status by device arguments.</param>
        /// <returns>Verification status</returns>
        public VerificationStatusResult Execute(GetVerificationStatusByDeviceArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var componentVerificationRequest = GetVerificationRequest(
                parameter.VerificationId,
                0,
                parameter.DeviceId);

            var result = CreateResult(componentVerificationRequest);
            return result;
        }
    }
}