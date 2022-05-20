namespace Aristocrat.Monaco.Application.EdgeLight.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Reflection;
    using Contracts;
    using Contracts.EdgeLight;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;

    public sealed class EdgeLightBottomStripHandler : IDisposable, IEdgeLightHandler
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEdgeLightingController _edgeLightingController;

        private readonly IEventBus _eventBus;

        private readonly IPropertiesManager _properties;
        private IEdgeLightToken _bottomEdgeLightingToken;
        private bool _disposed;

        public EdgeLightBottomStripHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IEdgeLightingController>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>()
            )
        {
        }

        public EdgeLightBottomStripHandler(
            IEventBus eventBus,
            IEdgeLightingController edgeLightingController,
            IPropertiesManager propertiesManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _edgeLightingController =
                edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        public string Name => typeof(EdgeLightBottomStripHandler).Name;

        public bool Enabled => _properties.GetValue(ApplicationConstants.BottomStripEnabled, false);

        private void Handle(BottomStripOffEvent obj)
        {
            HandleBottomEdgeLight(false);
        }

        private void Handle(BottomStripOnEvent obj)
        {
            HandleBottomEdgeLight(true);
        }

        private void Initialize()
        {
            Logger.Debug(Name + " Initialize()");

            if (!Enabled)
            {
                Logger.Debug(Name + " Bottom Strip Control Not Supported by this Cabinet.");
                return;
            }

            _eventBus.Subscribe<BottomStripOffEvent>(this, Handle);
            _eventBus.Subscribe<BottomStripOnEvent>(this, Handle);
            HandleBottomEdgeLight(_properties.GetValue(ApplicationConstants.BottomEdgeLightingOnKey, false));
        }

        private void HandleBottomEdgeLight(bool bottomEdgeLightingOn)
        {
            if (bottomEdgeLightingOn)
            {
                _edgeLightingController.RemoveEdgeLightRenderer(_bottomEdgeLightingToken);
            }
            else
            {
                _bottomEdgeLightingToken = _edgeLightingController.AddEdgeLightRenderer(
                    new SolidColorPatternParameters
                    {
                        Strips = new List<int> { (int)StripIDs.MainCabinetBottom },
                        Color = Color.Black,
                        Priority = StripPriority.BarTopBottomStripDisable
                    });
            }
        }
    }
}