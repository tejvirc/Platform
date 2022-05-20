namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    /// <summary>
    ///     An implementation of <see cref="ICabinetService" />
    /// </summary>
    public class CabinetService : ICabinetService
    {
        private readonly ICabinetState _cabinetState;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetService" /> class.
        /// </summary>
        /// <param name="cabinetState">An instance of <see cref="ICabinetState" /></param>
        public CabinetService(ICabinetState cabinetState)
        {
            _cabinetState = cabinetState ?? throw new ArgumentNullException(nameof(cabinetState));
        }

        /// <inheritdoc />
        public bool Idle => _cabinetState.Idle;

        /// <inheritdoc />
        public TimeSpan IdleTime => _cabinetState.IdleTime;

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ICabinetService) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}