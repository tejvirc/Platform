namespace Aristocrat.Monaco.Accounting.SelfAudit
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Contracts.SelfAudit;
    using Kernel;

    public class AccountingCreditMetersProvider : ICreditMetersProvider
    {
        private readonly IMeterManager _meterManager;

        public AccountingCreditMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IMeterManager>())
        {
        }

        public AccountingCreditMetersProvider(
            IMeterManager meterManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        /// <inheritdoc />
        public IEnumerable<IMeter> GetMeters()
        {
            return new List<IMeter>
            {
                _meterManager.GetMeter(AccountingMeters.TotalCashCoinTicketInAmount),
                _meterManager.GetMeter(AccountingMeters.ElectronicTransfersOnTotalAmount)
            };
        }
    }
}