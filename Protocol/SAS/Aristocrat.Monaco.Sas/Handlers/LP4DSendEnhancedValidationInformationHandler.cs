namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using VoucherValidation;

    /// <summary>
    ///     The LP4D SAS message handler
    /// </summary>
    public class LP4DSendEnhancedValidationInformationHandler
        : ISasLongPollHandler<SendEnhancedValidationInformationResponse, SendEnhancedValidationInformation>
    {
        private readonly IEnhancedValidationProvider _validationProvider;

        /// <summary>
        ///     Creates the LP4DSendEnhancedValidationInformationHandler instance
        /// </summary>
        /// <param name="validationProvider">The secure enhanced validation provider</param>
        public LP4DSendEnhancedValidationInformationHandler(IEnhancedValidationProvider validationProvider)
        {
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendEnhancedValidationInformation
        };

        /// <inheritdoc />
        public SendEnhancedValidationInformationResponse Handle(SendEnhancedValidationInformation data)
        {
            return _validationProvider.GetResponseFromInfo(data);
        }
    }
}