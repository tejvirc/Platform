namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using Aristocrat.G2S;
    using Data.OptionConfig;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Validate CommChangeLog
    /// </summary>
    public class OptionChangeLogValidationService : IOptionChangeLogValidationService
    {
        private readonly IOptionChangeLogRepository _changeLogRepository;
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionChangeLogValidationService" /> class.
        /// </summary>
        /// <param name="changeLogRepository">The change log repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public OptionChangeLogValidationService(
            IOptionChangeLogRepository changeLogRepository,
            IMonacoContextFactory contextFactory)
        {
            _changeLogRepository = changeLogRepository ?? throw new ArgumentNullException(nameof(changeLogRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <inheritdoc />
        public string Validate(long transactionId)
        {
            // 1. If the change request is not pending—that is, the status of the change request is not G2S_pending or
            // G2S_authorized—the EGM MUST include error code G2S_OCX006 Transaction Is Not Pending in its response.
            // 2. If the transactionId included in the authorizeOptionChange command references a change request that is
            // not associated with the device identified in the class-level element of the command, the EGM MUST include
            // error code G2S_OCX005 Invalid Transaction Identifier in its response.
            using (var context = _contextFactory.CreateDbContext())
            {
                var commChangeLog = _changeLogRepository.GetByTransactionId(context, transactionId);
                if (commChangeLog == null)
                {
                    return ErrorCode.G2S_OCX005;
                }

                commChangeLog = _changeLogRepository.GetPendingByTransactionId(context, transactionId);
                if (commChangeLog == null)
                {
                    return ErrorCode.G2S_OCX006;
                }
            }

            return string.Empty;
        }
    }
}