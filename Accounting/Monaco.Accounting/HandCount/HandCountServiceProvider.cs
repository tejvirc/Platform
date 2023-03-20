namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.Metering;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts.HandCount;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the HandCountService class.
    /// </summary>
    public class HandCountServiceProvider : IHandCountServiceProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IHandCountService _handCountService;

        /// <summary>
        /// Get hand counts if hand counts are enabled for jurisdiction, else returns 0
        /// </summary> 
        public int HandCount => _handCountService?.HandCount ?? 0;

        public string Name => typeof(HandCountServiceProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountServiceProvider) };

        public bool HandCountServiceEnabled => (bool)ServiceManager.GetInstance().GetService<IPropertiesManager>()
            .GetProperty(AccountingConstants.HandCountServiceEnabled, false);

        public void Initialize()
        {
            if (HandCountServiceEnabled)
            {
                _handCountService = new HandCountService();
                _handCountService.Initialize();

                ServiceManager.GetInstance().AddService(_handCountService);

                ServiceManager.GetInstance().AddServiceAndInitialize(new CashOutAmountCalculator());
            }
        }

        /// <summary>
        /// Decreases hand counts only for the jurisdiction hand counts are enabled for
        /// </summary>
        /// <param name="number"></param>
        public void DecreaseHandCount(int number)
        {
            _handCountService?.DecreaseHandCount(number);
        }

        /// <summary>
        ///     Increments hand counts only for the jurisdiction hand counts are enabled for
        /// </summary>
        public void IncrementHandCount()
        {
            _handCountService?.IncrementHandCount();
        }
    }
}