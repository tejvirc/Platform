namespace Aristocrat.Monaco.Accounting.SelfAudit
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Contracts;
    using Contracts.SelfAudit;
    using Kernel;

    public class AccountingDebitMetersProvider : IDebitMetersProvider
    {
        private readonly IMeterManager _meterManager;

        public AccountingDebitMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IMeterManager>())
        {
        }

        public AccountingDebitMetersProvider(
            IMeterManager meterManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        /// <inheritdoc />
        public IEnumerable<IMeter> GetMeters()
        {
            return new List<IMeter>
            {
                _meterManager.GetMeter(AccountingMeters.ElectronicTransfersOffTotalAmount),
                _meterManager.GetMeter(AccountingMeters.TotalCancelCreditAmount)
            };
        }
    }
}