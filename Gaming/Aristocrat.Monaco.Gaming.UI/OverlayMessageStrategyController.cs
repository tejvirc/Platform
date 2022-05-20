namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using CompositionRoot;
    using Contracts;
    using Kernel;
    using Runtime;

    public class OverlayMessageStrategyController : IOverlayMessageStrategyController
    {
        private readonly IOverlayMessageStrategyFactory _overlayMessageStrategyFactory;
        private readonly IPropertiesManager _properties;
        private readonly IPresentationService _presentationService;

        public bool GameRegistered { get; private set; }

        public IList<PresentationOverrideTypes> RegisteredPresentations { get; set; }

        public IOverlayMessageStrategy OverlayStrategy { get; private set; }

        public IOverlayMessageStrategy FallBackStrategy { get; private set; }

        public OverlayMessageStrategyController(
            IOverlayMessageStrategyFactory overlayMessageStrategyFactory,
            IPropertiesManager properties,
            IPresentationService presentationService)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _overlayMessageStrategyFactory = overlayMessageStrategyFactory ?? throw new ArgumentNullException(nameof(overlayMessageStrategyFactory));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));

            RegisteredPresentations = new List<PresentationOverrideTypes>();

            SetOverlayMessageStrategy();
        }

        public Task<bool> RegisterPresentation(bool registered, IEnumerable<PresentationOverrideTypes> types)
        {
            GameRegistered = registered;

            if (GameRegistered)
            {
                foreach (var t in types)
                {
                    RegisteredPresentations.Add(t);
                }
            }

            SetOverlayMessageStrategy();

            return Task.FromResult(true);
        }

        public void SetLastCashOutAmount(long cashOutAmount)
        {
            OverlayStrategy.LastCashOutAmount = cashOutAmount;
            FallBackStrategy.LastCashOutAmount = cashOutAmount;
        }

        public void SetHandpayAmountAndType(long handpayAmount, HandpayType handpayType)
        {
            OverlayStrategy.HandpayAmount = handpayAmount;
            FallBackStrategy.HandpayAmount = handpayAmount;

            OverlayStrategy.LastHandpayType = handpayType;
            FallBackStrategy.LastHandpayType = handpayType;
        }

        public void ClearGameDrivenPresentation()
        {
            _presentationService.PresentOverriddenPresentation(new List<PresentationOverrideData>());
        }

        private void SetOverlayMessageStrategy()
        {
            var isEnhanced = _properties.GetValue(ApplicationConstants.PlatformEnhancedDisplayEnabled, true);
            var fallbackStrategy =
                isEnhanced ? OverlayMessageStrategyOptions.Enhanced : OverlayMessageStrategyOptions.Basic;

            var strategy = GameRegistered ? OverlayMessageStrategyOptions.GameDriven : fallbackStrategy;

            OverlayStrategy = _overlayMessageStrategyFactory.Create(strategy);
            FallBackStrategy = _overlayMessageStrategyFactory.Create(fallbackStrategy);
        }
    }
}
