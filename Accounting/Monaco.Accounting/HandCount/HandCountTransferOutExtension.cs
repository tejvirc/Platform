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
        private bool isCashOut { get; set; }
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);
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
            _bus.Subscribe<HandCountDialogEvent>(this, Handle);
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsVisible { get; set; }

        private void Handle(HandCountDialogEvent obj)
        {
            isCashOut = obj.IsCashout;
            autoResetEvent.Set();
        }

        public long PreProcessor(long amount)
        {
            amount = _cashOutAmountCalculator.GetCashableAmount(amount);
            _bus.Publish(new HandCountChangedEvent(_handCountService.HandCount));
            _bus.Publish(new HandCountCashoutEvent(amount));
            autoResetEvent.WaitOne();
            if (isCashOut)
            {
                CashOutAsync(amount).Wait();
            }
            
            else
            {
                return 0;
            }
            return amount;
        }

        private async Task CashOutAsync(long amount)
        {
            if (amount > 2000000) 
            {
                _bus.Publish(new CashOutVisiblEventcs(true));
                var keyOff = Initiate();
                await keyOff.Task;

                _systemDisableManager.Enable(ApplicationConstants.LargePayoutDisableKey);
            }
        }

        private TaskCompletionSource<object> Initiate()
        {
            var keyOff = new TaskCompletionSource<object>();

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    keyOff.TrySetResult(null);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
            _systemDisableManager.Disable(
                 ApplicationConstants.LargePayoutDisableKey,
                 SystemDisablePriority.Immediate,
                  () => Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.PayOutLimit),
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
