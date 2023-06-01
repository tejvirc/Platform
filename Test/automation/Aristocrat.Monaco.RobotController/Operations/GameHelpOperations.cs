namespace Aristocrat.Monaco.RobotController
{
    using System;
    using System.Threading.Tasks;
    using Gaming.Contracts;
    using Gaming.Contracts.Events;
    using JetBrains.Annotations;
    using Kernel;

    internal class GameHelpOperations : IRobotOperations
    {
        private static readonly TimeSpan LengthOfTimeToLeaveHelpUpBeforeActing = TimeSpan.FromSeconds(5);

        private readonly IEventBus _eventBus;
        private readonly RobotLogger _logger;

        private bool _disposed;

        public bool IsHelpDisplayed { get; private set; }

        public GameHelpOperations(
            [NotNull] IEventBus eventBus,
            [NotNull] RobotLogger logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Reset()
        {
            _disposed = false;
        }

        public void Execute()
        {
            SubscribeToEvents();
        }

        public void Halt()
        {
            _logger.Info("Halt Request is Received!", GetType().Name);
            Dispose();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameRequestedPlatformHelpEvent>(this, Handle);
            _eventBus.Subscribe<ExitHelpEvent>(this, Handle);
        }

        private void Handle(GameRequestedPlatformHelpEvent evt)
        {
            _logger.Info("Game requested platform help", GetType().Name);
            IsHelpDisplayed = true;

            // We should clear help by clicking on the button to exit help (this is handled by TouchOperations),
            // but, if that takes too long (or doesn't happen at all), we need to just exit help so play can continue.
            Task
                .Delay(LengthOfTimeToLeaveHelpUpBeforeActing)
                .ContinueWith(
                    _ =>
                    {
                        if (IsHelpDisplayed)
                        {
                            _logger.Info("Requesting exit help", GetType().Name);
                            _eventBus.Publish(new ExitHelpEvent());
                        }
                    });
        }

        private void Handle(ExitHelpEvent evt)
        {
            _logger.Info("Help exited", GetType().Name);
            IsHelpDisplayed = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }
    }
}