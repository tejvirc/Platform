namespace Aristocrat.Monaco.Hhr.UI.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Cabinet.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;
    using Models;

    /// <summary>
    ///     Consumer for the <see cref="DisplayMonitorStatusChangeEvent" /> event.
    /// </summary>
    public class DisplayMonitorStatusChangeEventConsumer : Consumes<DisplayMonitorStatusChangeEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly List<DisplayMonitorStatus> _displayMonitorStatus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayMonitorStatusChangeEventConsumer" /> class.
        /// </summary>
        public DisplayMonitorStatusChangeEventConsumer(
            IEventBus eventBus,
            ICabinetDetectionService cabinetDetectionService)
            : base(eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _cabinetDetectionService = cabinetDetectionService ?? throw new ArgumentNullException(nameof(cabinetDetectionService));
            _displayMonitorStatus = new List<DisplayMonitorStatus>();

            foreach (DisplayRole displayRole in Enum.GetValues(typeof(DisplayRole)))
            {
                _displayMonitorStatus.Add(new DisplayMonitorStatus(displayRole, _cabinetDetectionService.IsDisplayConnected(displayRole)));
            }

            foreach (var x in _displayMonitorStatus)
            {
                Logger.Debug($"Display {x.DisplayRole} is {x.IsConnected}");
            }
        }

        /// <inheritdoc />
        public override void Consume(DisplayMonitorStatusChangeEvent theEvent)
        {
            _displayMonitorStatus.ForEach(x => {
                var isConnected = _cabinetDetectionService.IsDisplayConnected(x.DisplayRole);
                if (isConnected == x.IsConnected)
                {
                    return;
                }

                x.IsConnected = isConnected;
                _eventBus.Publish(new DisplayConnectionChangedEvent(x.DisplayRole, x.IsConnected));
            });
        }
    }
}