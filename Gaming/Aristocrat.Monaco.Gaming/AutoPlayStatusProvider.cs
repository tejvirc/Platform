namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Provides status on whether auto play is on or not and
    ///     sends events to turn auto play on or off
    /// </summary>
    public class AutoPlayStatusProvider : IAutoPlayStatusProvider, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private bool _autoPlayActive;
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="eventBus">reference to the EventBus service</param>
        public AutoPlayStatusProvider(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public bool EndAutoPlayIfActive()
        {
            var active = _autoPlayActive;
            Logger.Debug("Stopping active auto play in EndAutoPlayIfActive");
            if (_autoPlayActive)
            {
                StopSystemAutoPlay();
            }
            else
            // player may have initiated autoplay so shut down player autoplay
            {
                PausePlayerAutoPlay();
            }

            return active;
        }

        /// <inheritdoc />
        public void StartSystemAutoPlay()
        {
            _eventBus.Publish(new AutoPlayRequestedEvent(true));
            _autoPlayActive = true;
            Logger.Debug("Starting active auto play in StartSystemAutoPlay");
        }

        /// <inheritdoc />
        public void StopSystemAutoPlay()
        {
            _eventBus.Publish(new AutoPlayRequestedEvent(false));
            _autoPlayActive = false;
            Logger.Debug("Stopping active auto play in StopSystemAutoPlay");
        }

        /// <inheritdoc />
        public void PausePlayerAutoPlay()
        {
            _eventBus.Publish(new AutoPlayPauseRequestedEvent(true));
            _autoPlayActive = false;
            Logger.Debug("Pausing any active auto play in PausePlayerAutoPlay");
        }

        /// <inheritdoc />
        public void UnpausePlayerAutoPlay()
        {
            _eventBus.Publish(new AutoPlayPauseRequestedEvent(false));
            Logger.Debug("Unpausing auto play in UnpausePlayerAutoPlay");
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAutoPlayStatusProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}