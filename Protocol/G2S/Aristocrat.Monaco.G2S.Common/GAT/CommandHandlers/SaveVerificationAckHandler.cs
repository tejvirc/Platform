namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Linq;
    using Application.Contracts.Localization;
    using Exceptions;
    using Localization.Properties;
    using Monaco.Common.CommandHandlers;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Save verification-ack handler
    /// </summary>
    public class SaveVerificationAckHandler : IParametersActionHandler<SaveVerificationAckArgs>
    {
        private readonly IGatComponentVerificationRepository _componentVerificationRepository;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IGatVerificationRequestRepository _verificationRequestRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SaveVerificationAckHandler" /> class.
        /// </summary>
        /// <param name="verificationRequestRepository">The verification request repository.</param>
        /// <param name="componentVerificationRepository">The component verification repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        public SaveVerificationAckHandler(
            IGatVerificationRequestRepository verificationRequestRepository,
            IGatComponentVerificationRepository componentVerificationRepository,
            IMonacoContextFactory contextFactory)
        {
            _verificationRequestRepository = verificationRequestRepository ??
                                             throw new ArgumentNullException(nameof(verificationRequestRepository));
            _componentVerificationRepository = componentVerificationRepository ??
                                               throw new ArgumentNullException(nameof(componentVerificationRepository));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        ///     Executes the specified save verification ack arguments.
        /// </summary>
        /// <param name="parameter">The save verification ack arguments.</param>
        public void Execute(SaveVerificationAckArgs parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            using (var context = _contextFactory.CreateDbContext())
            {
                var request = _verificationRequestRepository.Get(
                    context,
                    x =>
                        x.VerificationId == parameter.VerificationId &&
                        x.TransactionId == parameter.TransactionId).SingleOrDefault();

                if (request == null)
                {
                    throw new VerificationRequestNotFoundException(
                        Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.NotFoundVerificationRequestErrorMessageTemplate,
                            parameter.VerificationId,
                            parameter.TransactionId));
                }

                foreach (var componentVerification in parameter.ComponentVerifications)
                {
                    var componentVerificationEntity =
                        _componentVerificationRepository.Get(
                                context,
                                x => x.RequestId == request.Id &&
                                     string.Compare(
                                         x.ComponentId,
                                         componentVerification.ComponentId,
                                         StringComparison.CurrentCultureIgnoreCase) == 0)
                            .SingleOrDefault();

                    if (componentVerificationEntity != null)
                    {
                        componentVerificationEntity.State = componentVerification.Passed
                            ? ComponentVerificationState.Passed
                            : ComponentVerificationState.Failed;

                        _componentVerificationRepository.Update(context, componentVerificationEntity);
                    }
                }
            }
        }
    }
}