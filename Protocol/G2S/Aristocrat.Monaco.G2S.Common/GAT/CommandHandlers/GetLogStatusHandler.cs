namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using Models;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Get log status handler
    /// </summary>
    public class GetLogStatusHandler : IFuncHandler<GetLogStatusResult>
    {
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IGatVerificationRequestRepository _verificationRequestRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetLogStatusHandler" /> class.
        /// </summary>
        /// <param name="verificationRequestRepository">The verification request repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public GetLogStatusHandler(
            IGatVerificationRequestRepository verificationRequestRepository,
            IMonacoContextFactory contextFactory)
        {
            _verificationRequestRepository = verificationRequestRepository ??
                                             throw new ArgumentNullException(nameof(verificationRequestRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        ///     Executes this instance.
        /// </summary>
        /// <returns>GAT log</returns>
        public GetLogStatusResult Execute()
        {
            using (var context = _contextFactory.Create())
            {
                var verificationRequestsCount = _verificationRequestRepository.Count(context);
                var maxLastSequence = verificationRequestsCount != 0
                    ? _verificationRequestRepository.GetMaxLastSequence(context)
                    : 0;

                var result = new GetLogStatusResult(maxLastSequence, verificationRequestsCount);

                return result;
            }
        }
    }
}