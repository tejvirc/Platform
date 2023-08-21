namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Exceptions;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="CurrencyInCompletedEvent" /> event.
    ///     This event is sent by the CurrencyInProvider when a bill
    ///     has been stacked and the bill value and count have been metered.
    /// </summary>
    public class CurrencyInCompletedConsumer : Consumes<CurrencyInCompletedEvent>
    {
        private readonly ISasNoteAcceptorProvider _noteAcceptorProvider;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IMeterManager _meterManager;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyInCompletedConsumer" /> class.
        /// </summary>
        /// <param name="noteAcceptorProvider">the note acceptor provider</param>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="propertiesManager">the properties manager</param>
        public CurrencyInCompletedConsumer(
            ISasNoteAcceptorProvider noteAcceptorProvider,
            ISasExceptionHandler exceptionHandler,
            IMeterManager meterManager,
            IPropertiesManager propertiesManager)
        {
            _noteAcceptorProvider = noteAcceptorProvider ?? throw new ArgumentNullException(nameof(noteAcceptorProvider));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(CurrencyInCompletedEvent theEvent)
        {
            if (theEvent.Amount == 0 || theEvent.Note is null)
            {
                return;
            }

            var denomMultiplier = _propertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            var amountInCents = ((long)(theEvent.Note.Value * denomMultiplier)).MillicentsToCents();
            var meterName = DenominationToMeterName.ToMeterName(amountInCents);

            _exceptionHandler.ReportException(
                new BillDataExceptionBuilder(
                    new BillData
                    {
                        AmountInCents = amountInCents,
                        CountryCode = SASCountryCodes.ToSASCountryCode(theEvent.Note.CurrencyCode),
                        LifetimeCount = _meterManager.IsMeterProvided(meterName) ? _meterManager.GetMeter(meterName).Lifetime : 0
                    }));

            if (_noteAcceptorProvider.BillDisableAfterAccept)
            {
                // according to SAS Spec 6.03 page 7-7 the note acceptor should still
                // accept vouchers but no longer accept bills. Send an empty list to
                // indicate we don't send any bills
                _noteAcceptorProvider.ConfigureBillDenominations(new List<ulong>());
            }
        }
    }
}
