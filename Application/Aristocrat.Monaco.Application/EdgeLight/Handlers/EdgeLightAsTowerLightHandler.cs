namespace Aristocrat.Monaco.Application.EdgeLight.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;

    internal sealed class EdgeLightAsTowerLightHandler : IDisposable, IEdgeLightHandler
    {
        private readonly IEdgeLightingController _edgeLightingController;

        private readonly IEventBus _eventBus;

        private readonly BartopPriorityComparer _normalModePriorityComparer = new BartopPriorityComparer(
            new List<StripPriority>
            {
                StripPriority.BarTopTowerLight,
                StripPriority.BarTopBottomStripDisable,
                StripPriority.AuditMenu,
                StripPriority.GamePriority,
                StripPriority.DoorOpen
            }
        );

        private readonly BartopPriorityComparer _operatorModePriorityComparer = new BartopPriorityComparer(
            new List<StripPriority>
            {
                StripPriority.BarTopBottomStripDisable,
                StripPriority.AuditMenu,
                StripPriority.DoorOpen,
                StripPriority.BarTopTowerLight
            }
        );

        private readonly IPropertiesManager _properties;
        private bool _disposed;

        public EdgeLightAsTowerLightHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEdgeLightingController>()
            )
        {
        }

        public EdgeLightAsTowerLightHandler(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IEdgeLightingController edgeLightingController)
        {
            _edgeLightingController = edgeLightingController ?? throw new ArgumentNullException(nameof(edgeLightingController));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            Initialize();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _disposed = true;
        }

        public string Name => typeof(EdgeLightAsTowerLightHandler).Name;

        public bool Enabled => _properties.GetValue(ApplicationConstants.EdgeLightAsTowerLightEnabled, false);

        private void Initialize()
        {
            if (!Enabled)
            {
                return;
            }

            _edgeLightingController.SetPriorityComparer(_normalModePriorityComparer);

            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, Handle);
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, Handle);
        }

        private void Handle(OperatorMenuEnteredEvent evt)
        {
            _edgeLightingController.SetPriorityComparer(_operatorModePriorityComparer);
        }

        private void Handle(OperatorMenuExitedEvent evt)
        {
            _edgeLightingController.SetPriorityComparer(_normalModePriorityComparer);
        }

        private class BartopPriorityComparer : IComparer<StripPriority>
        {
            private readonly List<StripPriority> _priorityOrder;

            public BartopPriorityComparer(IReadOnlyCollection<StripPriority> desiredOrder)
            {
                var priorityOrder = ((StripPriority[])Enum.GetValues(typeof(StripPriority))).ToList();
                var firstIndex = priorityOrder.IndexOf(desiredOrder.First());
                foreach (var stripPriority in desiredOrder.Skip(1))
                {
                    var currentIndex = priorityOrder.IndexOf(stripPriority);
                    if (currentIndex > firstIndex)
                    {
                        priorityOrder.RemoveAt(currentIndex);
                        priorityOrder.Insert(firstIndex, stripPriority);
                    }
                    else
                    {
                        firstIndex = currentIndex;
                    }

                }

                _priorityOrder = priorityOrder;
            }

            public int Compare(StripPriority x, StripPriority y)
            {
                var indexX = _priorityOrder.IndexOf(x);
                var indexY = _priorityOrder.IndexOf(y);
                if (indexX >= 0 && indexY >= 0)
                {
                    return indexX.CompareTo(indexY);
                }

                return x.CompareTo(y);
            }
        }
    }
}