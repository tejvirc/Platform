namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Monaco.Application.Contracts;
    using Kernel;
    using log4net;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;

    public class AnalyticsService : IAnalyticsService, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;

        private bool _disposed;
        private readonly ICommandBuilder<IAnalyticsDevice, track> _trackCommandBuilder;

        public AnalyticsService(
            IG2SEgm egm,
            IEventBus eventBus,
            ICommandBuilder<IAnalyticsDevice, track> trackCommandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _trackCommandBuilder = trackCommandBuilder ?? throw new ArgumentNullException(nameof(trackCommandBuilder));

            SubscribeEvents();
        }

        ///<inheritdoc />
        public string Name => GetType().ToString();

        ///<inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IAnalyticsService) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S ProgressiveService.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
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

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<AnalyticsTrackEvent>(this, Handle);
        }

        private void Handle(AnalyticsTrackEvent evt)
        {
            var device = _egm.GetDevice<IAnalyticsDevice>();

            var command = BuildTrackCommand(device, evt.Action, evt.Category, evt.Traits);

            device.SendTrack(command);
        }

        private track BuildTrackCommand(IAnalyticsDevice device, string action, string category, string traits)
        {
            var command = new track
            {
                action = action,
                category = category,
                traits = traits
            };

            _trackCommandBuilder.Build(device, command);
            return command;
        }
    }
}

