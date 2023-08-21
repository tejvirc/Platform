namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using VoucherValidation;

    /// <inheritdoc />
    public class LP57SendPendingCashoutInformationHandler : ISasLongPollHandler<SendPendingCashoutInformation, LongPollData>
    {
        private static readonly IReadOnlyDictionary<TicketType, CashoutTypeCode> CashoutTicketMapping =
            new Dictionary<TicketType, CashoutTypeCode>
            {
                { TicketType.CashOut, CashoutTypeCode.CashableTicket },
                { TicketType.Restricted, CashoutTypeCode.RestrictedPromotionalTicket }
            };

        private readonly IPropertiesManager _propertiesManager;
        private readonly IHostValidationProvider _hostValidationProvider;

        /// <summary>
        ///     Creates a LP57SendPendingCashoutInformationHandler instance
        /// </summary>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager"/></param>
        /// <param name="hostValidationProvider">An instance of <see cref="IHostValidationProvider"/></param>
        public LP57SendPendingCashoutInformationHandler(
            IPropertiesManager propertiesManager,
            IHostValidationProvider hostValidationProvider)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _hostValidationProvider =
                hostValidationProvider ?? throw new ArgumentNullException(nameof(hostValidationProvider));
        }

        /// <inheritdoc />
        public SendPendingCashoutInformation Handle(LongPollData data)
        {
            if (_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType !=
                SasValidationType.System)
            {
                return new SendPendingCashoutInformation(false);
            }

            var hostValidationData = _hostValidationProvider.GetPendingValidationData();
            return hostValidationData == null ||
                   !CashoutTicketMapping.TryGetValue(hostValidationData.TicketType, out var code)
                ? new SendPendingCashoutInformation(CashoutTypeCode.NotWaitingForSystemValidation, 0)
                : new SendPendingCashoutInformation(code, (ulong)((long)hostValidationData.Amount).MillicentsToCents());
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendPendingCashoutInformation };
    }
}