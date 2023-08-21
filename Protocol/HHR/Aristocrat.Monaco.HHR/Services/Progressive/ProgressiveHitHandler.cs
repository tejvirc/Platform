namespace Aristocrat.Monaco.Hhr.Services.Progressive
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using log4net;

    public class ProgressiveHitHandler : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        private bool _disposed;

        public ProgressiveHitHandler(
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter,
            IEventBus eventBus)
        {
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter
                ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<LinkedProgressiveHitEvent>(this, Handle);
        }

        private void Handle(LinkedProgressiveHitEvent linkedEvent)
        {
            var linkedLevel = linkedEvent.LinkedProgressiveLevels.FirstOrDefault();

            if (linkedLevel == null)
            {
                Logger.Error($"Cannot find linkedLevel for event {linkedEvent.Level.LevelId} {linkedEvent.Level.WagerCredits}");
                return;
            }

            Logger.Debug(
                $"AwardJackpot progressiveLevel = {linkedEvent.Level.LevelName}" +
                $" linkedLevel = {linkedLevel.LevelName} " +
                $"amountInPennies = {linkedLevel.ClaimStatus.WinAmount} " +
                $"CurrentValue = {linkedEvent.Level.CurrentValue}");

            _protocolLinkedProgressiveAdapter.ClaimAndAwardLinkedProgressiveLevel(ProtocolNames.HHR, linkedLevel.LevelName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
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
    }
}