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
        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;

        private bool _gameRegistered;

        private bool _disposed;

        public bool GameRegistered
        {
            get => _gameRegistered && _runtime.Connected;
            private set => _gameRegistered = value;
        }

        public IList<PresentationOverrideTypes> RegisteredPresentations { get; set; }

        public IOverlayMessageStrategy OverlayStrategy { get; private set; }

        public IOverlayMessageStrategy FallBackStrategy { get; private set; }

        public OverlayMessageStrategyController(
            IOverlayMessageStrategyFactory overlayMessageStrategyFactory,
            IPropertiesManager properties,
            IPresentationService presentationService,
            IRuntime runtime,
            IEventBus eventBus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _overlayMessageStrategyFactory = overlayMessageStrategyFactory ?? throw new ArgumentNullException(nameof(overlayMessageStrategyFactory));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            RegisteredPresentations = new List<PresentationOverrideTypes>();
            
            SetOverlayMessageStrategy();

            _eventBus.Subscribe<GameProcessExitedEvent>(this, HandleEvent);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
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

        private void HandleEvent(GameProcessExitedEvent evt)
        {
            GameRegistered = false;
        }
    }
}
