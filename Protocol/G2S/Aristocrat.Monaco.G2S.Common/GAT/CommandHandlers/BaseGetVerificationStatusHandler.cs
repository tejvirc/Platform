namespace Aristocrat.Monaco.G2S.Common.GAT.CommandHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Application.Contracts.Authentication;
    using Application.Contracts.Localization;
    using Exceptions;
    using Localization.Properties;
    using Models;
    using Monaco.Common.Storage;
    using Storage;

    /// <summary>
    ///     Base get verification status handler
    /// </summary>
    public abstract class BaseGetVerificationStatusHandler
    {
        /// <summary>
        ///     The component verification repository
        /// </summary>
        private readonly IGatComponentVerificationRepository _componentVerificationRepository;

        /// <summary>
        ///     The context factory.
        /// </summary>
        private readonly IMonacoContextFactory _contextFactory;

        /// <summary>
        ///     The verification request repository
        /// </summary>
        private readonly IGatVerificationRequestRepository _verificationRequestRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseGetVerificationStatusHandler" /> class.
        /// </summary>
        /// <param name="verificationRequestRepository">The verification request repository.</param>
        /// <param name="componentVerificationRepository">The component verification repository.</param>
        /// <param name="contextFactory">The context factory.</param>
        protected BaseGetVerificationStatusHandler(
            IGatVerificationRequestRepository verificationRequestRepository,
            IGatComponentVerificationRepository componentVerificationRepository,
            IMonacoContextFactory contextFactory)
        {
            _verificationRequestRepository = verificationRequestRepository;
            _componentVerificationRepository = componentVerificationRepository;
            _contextFactory = contextFactory;

            if (verificationRequestRepository == null)
            {
                throw new ArgumentNullException(nameof(verificationRequestRepository));
            }

            if (componentVerificationRepository == null)
            {
                throw new ArgumentNullException(nameof(componentVerificationRepository));
            }

            if (contextFactory == null)
            {
                throw new ArgumentNullException(nameof(contextFactory));
            }
        }

        /// <summary>
        ///     Gets the verification request repository.
        /// </summary>
        /// <value>
        ///     The verification request repository.
        /// </value>
        protected IGatVerificationRequestRepository VerificationRequestRepository => _verificationRequestRepository;

        /// <summary>
        ///     Gets the component verification repository.
        /// </summary>
        /// <value>
        ///     The component verification repository.
        /// </value>
        protected IGatComponentVerificationRepository ComponentVerificationRepository =>
            _componentVerificationRepository;

        /// <summary>
        ///     Gets the verification request.
        /// </summary>
        /// <param name="verificationId">The verification identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>Verification request by verification, transaction, device identifier.</returns>
        protected GatVerificationRequest GetVerificationRequest(
            long verificationId,
            long transactionId,
            int deviceId)
        {
            GatVerificationRequest verificationRequest;

            using (var context = _contextFactory.CreateDbContext())
            {
                verificationRequest =
                    _verificationRequestRepository.Get(
                        context,
                        x => x.VerificationId == verificationId).SingleOrDefault();

                if (transactionId > 0)
                {
                    if (verificationRequest != null && verificationRequest.TransactionId != transactionId)
                    {
                        throw new TransactionIdNotCorrespondingVerificationIdException(
                            Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.TransactionIdNotCorrespondingVerificationIdErrorMessageTemplate,
                                transactionId,
                                verificationId));
                    }
                }
                else
                {
                    if (verificationRequest != null && verificationRequest.DeviceId != deviceId)
                    {
                        throw new DeviceIdNotCorrespondingVerificationIdException(
                            Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.DeviceIdNotCorrespondingVerificationIdErrorMessageTemplate,
                                deviceId,
                                verificationId));
                    }
                }

                verificationRequest =
                    _verificationRequestRepository.Get(
                            context,
                            CreateSelectExpression(verificationId, transactionId, deviceId))
                        .SingleOrDefault();
            }

            if (verificationRequest == null)
            {
                var replacementText = transactionId > 0 ? "transactionId" : "deviceId";
                var replacementValue = transactionId > 0 ? transactionId : deviceId;

                throw new VerificationRequestNotFoundException(
                    Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.VerificationRequestNotFoundErrorMessageTemplate,
                        verificationId,
                        replacementText,
                        replacementValue));
            }

            return verificationRequest;
        }

        /// <summary>
        ///     Creates the result.
        /// </summary>
        /// <param name="verificationRequest">The verification request.</param>
        /// <returns>Verification status</returns>
        protected VerificationStatusResult CreateResult(GatVerificationRequest verificationRequest)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                if (verificationRequest.Completed)
                {
                    var componentVerifications = new List<ComponentVerificationResult>();
                    foreach (var v in _componentVerificationRepository.Get(
                        context,
                        x => x.RequestId == verificationRequest.Id))
                    {
                        componentVerifications.Add(
                            new ComponentVerificationResult(
                                v.ComponentId,
                                v.State,
                                v.GatExec,
                                ConvertExtensions.ToGatResultString(v.Result, v.AlgorithmType != AlgorithmType.Crc32)));
                    }

                    return new VerificationStatusResult(true, componentVerifications.ToList(), null);
                }

                var componentStatuses = new List<ComponentStatus>();
                foreach (var x in _componentVerificationRepository.Get(
                    context,
                    x => x.RequestId == verificationRequest.Id))
                {
                    componentStatuses.Add(new ComponentStatus(x.ComponentId, x.State));
                }

                return new VerificationStatusResult(
                    false,
                    null,
                    new VerificationStatus(
                        verificationRequest.VerificationId,
                        verificationRequest.TransactionId,
                        componentStatuses));
            }
        }

        private Expression<Func<GatVerificationRequest, bool>> CreateSelectExpression(
            long verificationId,
            long transactionId,
            int deviceId)
        {
            Expression<Func<GatVerificationRequest, bool>> expression;

            if (transactionId > 0)
            {
                expression = x => x.VerificationId == verificationId && x.TransactionId == transactionId;
            }
            else
            {
                expression = x => x.VerificationId == verificationId && x.DeviceId == deviceId;
            }

            return expression;
        }
    }
}