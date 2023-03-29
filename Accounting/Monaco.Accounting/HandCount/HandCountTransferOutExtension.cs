namespace Aristocrat.Monaco.Accounting.HandCount
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Localization.Properties;
    using Contracts;
    using Kernel;

    public class HandCountTransferOutExtension : ITransferOutExtension
    {
        private readonly IHandCountService _handCountService;
        private readonly ICashOutAmountCalculator _cashOutAmountCalculator;
        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _systemDisableManager;
        
        private readonly IPropertiesManager _properties;

        public string Name => typeof(HandCountTransferOutExtension).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ITransferOutExtension) };
        public HandCountTransferOutExtension()
            : this(ServiceManager.GetInstance().GetService<IHandCountService>(),
        ServiceManager.GetInstance().GetService<ICashOutAmountCalculator>(),
        ServiceManager.GetInstance().GetService<IEventBus>(),
        ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>()
        )
        {
        }

        public HandCountTransferOutExtension(
            IHandCountService handCountService,
            ICashOutAmountCalculator cashOutAmountCalculator,
            IEventBus bus,
            ISystemDisableManager systemDisableManager,
            IPropertiesManager properties
            )
        {
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashOutAmountCalculator = cashOutAmountCalculator ?? throw new ArgumentNullException(nameof(cashOutAmountCalculator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public long PreProcessor(long amount)
        {
            amount = _cashOutAmountCalculator.GetCashableAmount(amount);
            _bus.Publish(new HandCountChangedEvent(_handCountService.HandCount, amount));
            //Thread.Sleep(5000);
            CashOutAsync(amount).Wait();
            //_bus.Publish(new HandCountChangedEvent(_handCountService.HandCount, amount, false));
            return amount;
        }

        private async Task CashOutAsync(long amount)
        {
            if (amount > 1200000) 
            {
                var keyOff = Initiate(amount);
                await keyOff.Task;

                _systemDisableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }
        }

        private TaskCompletionSource<object> Initiate(long amount)
        {
            var keyOff = new TaskCompletionSource<object>();

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
            //var divisor = _properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            //OverlayMessageUtils.ToCredits(HandpayAmount).FormattedCurrencyString();
            //var check = (double)amount.MillicentsToDollars();
            //var res = check.FormattedCurrencyString();
            //Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PayOutLimit),
            //var divisor = _properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            //var amt = amount / divisor;
            //Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.PayOutLimit,
            //                amt.FormattedCurrencyString());

            //() => Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.PayOutLimit,
            //            amt.FormattedCurrencyString())
            _systemDisableManager.Disable(
                ApplicationConstants.LargePayoutDisableKey,
                SystemDisablePriority.Immediate,
                () =>Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PayOutLimit),
            true,
                () => Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PayOutLimit));
            return keyOff;
        }

        public void PosProcessor(long amount)
        {
            var handCount = _cashOutAmountCalculator.GetHandCountUsed(amount);
            _handCountService.DecreaseHandCount(handCount);

        }

        public void Initialize()
        {
            
        }
    }
}
