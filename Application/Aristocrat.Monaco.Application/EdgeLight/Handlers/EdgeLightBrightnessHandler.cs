namespace Aristocrat.Monaco.Application.EdgeLight.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This code that implements the operator
    ///     setting of the Minimum and Maximum Brightness.
    /// </summary>
    public class EdgeLightBrightnessHandler : IDisposable, IEdgeLightHandler
    {
        private const int MaxChannelBrightness = 100;

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEdgeLightingController _edgeLightingController;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        private readonly List<StripPriority> _stripPrioritiesForBrightnessLimit =
            ((StripPriority[])Enum.GetValues(typeof(StripPriority)))
            .Where(x => x <= StripPriority.AuditMenu).ToList();

        private bool _disposed;

        public EdgeLightBrightnessHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IEdgeLightingController>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>()
            )
        {
        }

        public EdgeLightBrightnessHandler(
            IEventBus eventBus,
            IEdgeLightingController edgeLightingController,
            IPropertiesManager propertiesManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(EdgeLightBrightnessHandler).Name;

        public bool Enabled => _properties.GetValue(ApplicationConstants.EdgeLightingBrightnessControlEnabled, false);

        // Do relevant thing to update the color/brightness on startup.
        private void Initialize()
        {
            Logger.Debug(Name + " Initialize()");

            if (!Enabled)
            {
                Logger.Debug(Name + " Edge Light Brightness Control Not Supported by this Cabinet.");
                return;
            }

            _eventBus.Subscribe<MaximumOperatorBrightnessChangedEvent>(this, Handle);

            //Initialize the Brightness as saved in non volatile RAM on startup.
            // If the RAM doesn't have value make it 100.
            var brightnessLimit = new EdgeLightingBrightnessLimits
            {
                MaximumAllowed = _properties.GetValue(
                    ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey,
                    _properties.GetValue(
                        ApplicationConstants.EdgeLightingBrightnessControlDefault,
                        MaxChannelBrightness)),
                MinimumAllowed =
                    _properties.GetValue(
                        ApplicationConstants.EdgeLightingBrightnessControlMin,
                        ApplicationConstants.DefaultEdgeLightingMinimumBrightness)
            };

            _stripPrioritiesForBrightnessLimit.ForEach(
                x => _edgeLightingController.SetBrightnessLimits(brightnessLimit, x));

            // During startup restore the brightness to what operator had set.
            // Brightness Priority is LowPriority as the game should override it.
            _edgeLightingController.SetBrightnessForPriority(
                brightnessLimit.MaximumAllowed,
                StripPriority.LowPriority);
        }

        private void Handle(MaximumOperatorBrightnessChangedEvent evt)
        {
            _stripPrioritiesForBrightnessLimit.ForEach(
                x =>
                {
                    var limit = _edgeLightingController.GetBrightnessLimits(x);
                    limit.MaximumAllowed = evt.MaxOperatorBrightness;
                    _edgeLightingController.SetBrightnessLimits(limit, x);
                });
        }

        /// <summary>Releases allocated resources.</summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; False to release only unmanaged
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
    }
}