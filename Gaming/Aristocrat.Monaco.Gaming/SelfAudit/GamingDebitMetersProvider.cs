namespace Aristocrat.Monaco.Gaming.SelfAudit
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.SelfAudit;
    using Application.Contracts;
    using Contracts;
    using Kernel;

    public class GamingDebitMetersProvider : IDebitMetersProvider
    {
        private readonly IMeterManager _meterManager;

        public GamingDebitMetersProvider()
            : this(ServiceManager.GetInstance().GetService<IMeterManager>())
        {
        }

        public GamingDebitMetersProvider(
            IMeterManager meterManager)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        /// <inheritdoc />
        public IEnumerable<IMeter> GetMeters()
        {
            return new List<IMeter> { _meterManager.GetMeter(GamingMeters.WageredAmount) };
        }
    }
}