namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Kernel;
    using log4net;
    using Aristocrat.Monaco.G2S.Common.Events;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class AnalyticsService : IAnalyticsService, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;

        private bool _disposed;

        public AnalyticsService(
            IG2SEgm egm,
            IEventBus eventBus)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

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

            var command = BuildTrackCommand(evt.Action, evt.Category, evt.Traits);

            device.SendTrack(command);
        }

        private static track BuildTrackCommand(string action, string category, string traits)
        {
            var command = new track
            {
                action = action,
                category = category,
                traits = traits
            };
            //TODO: call command builder when exists
            return command;
        }
    }
}

