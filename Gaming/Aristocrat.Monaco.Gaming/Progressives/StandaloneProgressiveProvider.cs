namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using Application.Contracts;
    using Contracts.Progressives;
    using Kernel;

    public sealed class StandaloneProgressiveProvider : ISapProvider, IDisposable
    {
        private readonly IProgressiveCalculatorFactory _calculatorFactory;
        private readonly IMysteryProgressiveProvider _mysteryProgressiveProvider;
        private readonly IEventBus _bus;

        public StandaloneProgressiveProvider(
            IProgressiveCalculatorFactory calculatorFactory,
            IEventBus bus,
            IMysteryProgressiveProvider mysteryProgressiveProvider)
        {
            _calculatorFactory = calculatorFactory ?? throw new ArgumentNullException(nameof(calculatorFactory));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mysteryProgressiveProvider = mysteryProgressiveProvider ?? throw new ArgumentNullException(nameof(mysteryProgressiveProvider));

            _bus.Subscribe<ProgressiveHitEvent>(this, Handle);
            _bus.Subscribe<ProgressiveCommitEvent>(this, Handle);
        }

        public void Increment(ProgressiveLevel level, long wager, long ante, IMeter hiddenTotalMeter)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            var calculator = _calculatorFactory.Create(level.FundingType);
            calculator?.Increment(level, wager, ante, hiddenTotalMeter);
        }

        public void ProcessHit(ProgressiveLevel level, IViewableJackpotTransaction transaction)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var calculator = _calculatorFactory.Create(level.FundingType);

            var awardAmount = calculator?.Claim(level) ?? 0L;

            if (level.TriggerControl == TriggerType.Mystery)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(level);
            }

            _bus.Publish(new SapAwardedEvent(transaction.TransactionId, awardAmount, String.Empty, PayMethod.Any));
        }

        public void Reset(ProgressiveLevel level)
        {
            if (level == null)
            {
                throw new ArgumentNullException(nameof(level));
            }

            var calculator = _calculatorFactory.Create(level.FundingType);
            
            calculator?.Reset(level);
        }

        public void Dispose()
        {
            _bus?.UnsubscribeAll(this);
        }

        private void Handle(ProgressiveHitEvent theEvent)
        {
            var assignedLevelId = theEvent.Level.AssignedProgressiveId;


            if (theEvent.Level.LevelType == ProgressiveLevelType.Sap &&
                (theEvent.Level.AssignedProgressiveId == null ||
                 theEvent.Level.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.None))
            {
                ProcessHit(theEvent.Level as ProgressiveLevel, theEvent.Jackpot);
            }
        }

        private void Handle(ProgressiveCommitEvent theEvent)
        {
            // reset is handled by the calculator automatically on claim... should it?
        }
    }
}