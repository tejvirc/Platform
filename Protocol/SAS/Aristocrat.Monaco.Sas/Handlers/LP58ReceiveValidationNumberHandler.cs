namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using VoucherValidation;

    /// <inheritdoc />
    public class LP58ReceiveValidationNumberHandler : ISasLongPollHandler<ReceiveValidationNumberResult, ReceiveValidationNumberData>
    {
        private const byte ValidationDenied = 0x00;

        private readonly IPropertiesManager _propertiesManager;
        private readonly IHostValidationProvider _hostValidationProvider;

        /// <summary>
        ///     Creates and instance of LP58ReceiveValidationNumberHandler
        /// </summary>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager"/></param>
        /// <param name="hostValidationProvider">An instance of <see cref="IHostValidationProvider"/></param>
        public LP58ReceiveValidationNumberHandler(
            IPropertiesManager propertiesManager,
            IHostValidationProvider hostValidationProvider)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _hostValidationProvider =
                hostValidationProvider ?? throw new ArgumentNullException(nameof(hostValidationProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.ReceiveValidationNumber };

        /// <inheritdoc />
        public ReceiveValidationNumberResult Handle(ReceiveValidationNumberData data)
        {
            if (_propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ValidationType !=
                SasValidationType.System)
            {
                return new ReceiveValidationNumberResult(
                    false,
                    ReceiveValidationNumberStatus.ImproperValidationRejected);
            }

            var currentState = _hostValidationProvider.CurrentState;
            var invalidValidation = currentState != ValidationState.ValidationNumberPending &&
                                    data.ValidationSystemId != ValidationDenied;
            var result = _hostValidationProvider.SetHostValidationResult(
                invalidValidation || data.ValidationSystemId == ValidationDenied
                    ? null
                    : new HostValidationResults(data.ValidationSystemId, $"{data.ValidationNumber:D16}"));
            ReceiveValidationNumberStatus currentStatus;
            if (!result)
            {
                currentStatus = ReceiveValidationNumberStatus.NotInCashout;
            }
            else if (invalidValidation)
            {
                currentStatus = ReceiveValidationNumberStatus.ImproperValidationRejected;
            }
            else
            {
                currentStatus = ReceiveValidationNumberStatus.CommandAcknowledged;
            }

            return new ReceiveValidationNumberResult(true, currentStatus);
        }
    }
}