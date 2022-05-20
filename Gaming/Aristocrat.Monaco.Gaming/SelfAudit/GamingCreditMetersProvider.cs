namespace Aristocrat.Monaco.Gaming.SelfAudit
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.SelfAudit;
    using Application.Contracts;
    using Contracts;
    using Kernel;

    public class GamingCreditMetersProvider : ICreditMetersProvider
    {
        private readonly IMeterManager _meterManager;

        public GamingCreditMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IMeterManager>())
        {
        }

        public GamingCreditMetersProvider(
            IMeterManager meterManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        public IEnumerable<IMeter> GetMeters()
        {
            return new List<IMeter>
            {
                _meterManager.GetMeter(GamingMeters.TotalPaidAmtExcludingTotalPaidLinkedProgAmt)
            };
        }
    }
}