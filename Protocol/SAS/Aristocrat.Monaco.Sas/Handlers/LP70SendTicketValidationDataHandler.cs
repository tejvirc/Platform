namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP 70 Send Ticket Validation Data
    /// </summary>
    public class LP70SendTicketValidationDataHandler : ISasLongPollHandler<SendTicketValidationDataResponse, LongPollData>
    {
        private readonly ISasVoucherInProvider _sasVoucherInProvider;
        private readonly SasValidationType _validationType;

        /// <summary>
        ///     Constructs the handler
        /// </summary>
        /// <param name="propMan">reference to properties manager</param>
        /// <param name="sasVoucherInProvider"></param>
        public LP70SendTicketValidationDataHandler(
            IPropertiesManager propMan,
            ISasVoucherInProvider sasVoucherInProvider)
        {
            var propertyManager = propMan ?? throw new ArgumentNullException(nameof(propMan));
            _validationType = propertyManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .ValidationType;
            _sasVoucherInProvider = sasVoucherInProvider ?? throw new ArgumentNullException(nameof(sasVoucherInProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendTicketValidationData
        };

        /// <inheritdoc />
        public SendTicketValidationDataResponse Handle(LongPollData data)
        {
            if (_validationType != SasValidationType.SecureEnhanced &&
                _validationType != SasValidationType.System)
            {
                // Attempt to deny any pending ticket in values as SAS does not validate them in this configuration
                Task.Run(() => _sasVoucherInProvider.DenyTicket());

                // it can be ignored. just let Host cancel it on its own.
                return null;
            }

            if(_sasVoucherInProvider.CurrentState != SasVoucherInState.ValidationRequestPending
               && _sasVoucherInProvider.CurrentState != SasVoucherInState.ValidationRequestPendingWithAcknowledgementPending)
            {
                //There is no ticket is Escrow, so we send back null Barcode so parser puts in FF
                return new SendTicketValidationDataResponse { Barcode = null };
            }

            var response = _sasVoucherInProvider.RequestValidationData();

            return response;
        }
    }
}
