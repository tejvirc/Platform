namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using Aristocrat.G2S;
    using Data.CommConfig;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Provides a mechanism to check the current state of the comm change log
    /// </summary>
    public class CommChangeLogValidationService : ICommChangeLogValidationService
    {
        private readonly ICommChangeLogRepository _changeLogRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommChangeLogValidationService" /> class.
        /// </summary>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public CommChangeLogValidationService(
            ICommChangeLogRepository changeLogRepository,
            IMonacoContextFactory contextFactory)
        {
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public string Validate(long transactionId)
        {
            // If the changes are no longer pending(changeStatus is set to G2S_cancelled, G2S_applied, G2S_aborted, or
            // G2S_error) and the specified transactionId is present in the commConfig class log, the EGM MUST respond
            // with error code G2S_CCX006 Transaction Is Not Pending.If the specified transactionId is not present in
            // the commConfig class log, the EGM MUST respond with error code G2S_CCX005 Invalid Transaction Identifier.
            // If the transactionId included in the authorizeCommChange command references a change request that is not
            // associated with the device identified in the class-level element of the command, the EGM MUST include error
            // code G2S_CCX005 Invalid Transaction Identifier in its response.
            using (var context = _contextFactory.Create())
            {
                var commChangeLog = _changeLogRepository.GetByTransactionId(context, transactionId);
                if (commChangeLog == null)
                {
                    return ErrorCode.G2S_CCX005;
                }

                commChangeLog = _changeLogRepository.GetPendingByTransactionId(context, transactionId);
                if (commChangeLog == null)
                {
                    return ErrorCode.G2S_CCX006;
                }
            }

            return string.Empty;
        }
    }
}