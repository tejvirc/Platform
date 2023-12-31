﻿namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Timers;
    using Contracts;
    using GDKRuntime.Contract;
    using Kernel;

    /// <summary>
    ///     A service that will occasionally call for generation of a random number, so that if a
    ///     player somehow knew the internal state of the RNG, it is harder to tell what number
    ///     will be used for the next game outcome.
    /// </summary>
    public class RngCyclingService : IDisposable
    {
        // Maximum of 5 times per second, minimum of 1 time per second.
        private const double MinimumCycleTimeMs = 200.0;
        private const int MaximumExtraCycleTimeMs = 800;

        private readonly IPropertiesManager _properties;
        private readonly IGameSession _gameSession;

        private Timer _cycleTimer;
        private bool _disposed;

        /// <summary>
        ///     Constructor takes the properties manager and the RPC object that GDK calls in order
        ///     to get random numbers for game outcomes.
        /// </summary>
        /// <param name="properties">The property manager, for settings.</param>
        /// <param name="gameSession">The RPC object, for the RNG.</param>
        public RngCyclingService(IPropertiesManager properties,
                                 IGameSession gameSession)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        }

        private void CycleTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _cycleTimer.Interval = _gameSession.GetRandomNumberU64(MaximumExtraCycleTimeMs) + MinimumCycleTimeMs;
        }

        /// <summary>
        ///     If the property for RNG cycling is set in the jurisdiction gaming config, start the
        ///     timer, otherwise do nothing.
        /// </summary>
        public void StartCycling()
        {
            if (_properties.GetValue(GamingConstants.UseRngCycling, false))
            {
                _cycleTimer = new Timer(MinimumCycleTimeMs + MaximumExtraCycleTimeMs);
                _cycleTimer.Elapsed += CycleTimerOnElapsed;
                _cycleTimer.Start();
            }
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
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return; 
            }

            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (_cycleTimer != null)
                {
                    _cycleTimer.Stop();
                    _cycleTimer.Dispose();
                }
            }

            _cycleTimer = null;

            _disposed = true;
        }
    }
}