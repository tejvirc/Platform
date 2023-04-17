namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;
    using Events;
    using Kernel;
    using log4net;

    public class HandpayValidator : IHandpayValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool AllowLocalHandpay => true;

        public bool HostOnline { get; private set; }

        public string Name => nameof(HandpayValidator);

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayValidator) };

        public HandpayValidator(IEventBus eventBus)
        {
            var bus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            bus.Subscribe<LinkStatusChangedEvent>(this, (e) => HostOnline = e.IsLinkUp);
        }

        public void Initialize()
        {
        }

        public bool LogTransactionRequired(Accounting.Contracts.ITransaction transaction = null) => true;

        public Task RequestHandpay(HandpayTransaction transaction) => Task.CompletedTask;

        public Task HandpayKeyedOff(HandpayTransaction transaction) => Task.CompletedTask;

        public bool ValidateHandpay(long cashableAmount, long promoAmount, long nonCashAmount, HandpayType handpayType)
        {
            Logger.Debug($"Validating handpay (cashable={cashableAmount}, promo={promoAmount}, nonCash={nonCashAmount}, handpayType={handpayType})");
            return true;
        }
    }
}