namespace Aristocrat.Monaco.Gaming
{
    using Aristocrat.GdkRuntime.V1;
    using Aristocrat.Monaco.Gaming.Runtime.Server;
    using Contracts;
    using Kernel;
    using Aristocrat.CryptoRng;
    using System;
    using System.Timers;

    /// <summary>
    ///     A service that will occasionally call for generation of a random number, so that if a
    ///     player somehow knew the internal state of the RNG, it is harder to tell what number
    ///     will be used for the next game outcome.
    /// </summary>
    public sealed class RngCyclingService : IDisposable
    {
        // Maximum of 5 times per second, minimum of 1 time per second.
        private const double MinimumCycleTimeMs = 200.0;
        private const int MaximumExtraCycleTimeMs = 800;

        private readonly IPropertiesManager _properties;
        private readonly IRandom _prng;

        private Timer _cycleTimer;
        private bool _disposed;

        /// <summary>
        ///     Constructor takes the properties manager and the RPC object that GDK calls in order
        ///     to get random numbers for game outcomes.
        /// </summary>
        /// <param name="properties">The property manager, for settings.</param>
        /// <param name="randomFactory">The random factory</param>
        public RngCyclingService(IPropertiesManager properties, IRandomFactory randomFactory)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _prng = randomFactory?.Create(RandomType.Gaming) ?? throw new ArgumentNullException(nameof(randomFactory));
        }

        private void CycleTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _cycleTimer.Interval = _prng.GetValue(MaximumExtraCycleTimeMs) + MinimumCycleTimeMs;
        }

        /// <summary>
        ///     If the property for RNG cycling is set in the jurisdiction gaming config, start the
        ///     timer, otherwise do nothing.
        /// </summary>
        public void StartCycling()
        {
            if (!_properties.GetValue(GamingConstants.UseRngCycling, false))
            {
                return;
            }

            _cycleTimer = new Timer(MinimumCycleTimeMs + MaximumExtraCycleTimeMs);
            _cycleTimer.Elapsed += CycleTimerOnElapsed;
            _cycleTimer.Start();
        }

        /// <summary>
        ///     Stop the timer, so this object can be garbage collected as we shut down the EGM.
        /// </summary>
        public void StopCycling()
        {
            _cycleTimer?.Stop();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var cycleTimer = _cycleTimer;
            _cycleTimer = null;
            if (cycleTimer != null)
            {
                cycleTimer.Stop();
                cycleTimer.Elapsed -= CycleTimerOnElapsed;
                cycleTimer.Dispose();
            }

            _disposed = true;
        }
    }
}