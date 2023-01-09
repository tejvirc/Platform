namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the HandCountSessionService class.
    /// </summary>
    public class HandCountSessionService : IHandCountSessionService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;

        private int _handCount = 0;

        public HandCountSessionService(
            IEventBus eventBus,
            IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string Name => typeof(GameCategoryService).FullName;

        public int HandCount => _handCount;

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountSessionService) };

        public void Initialize()
        {
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, HandleEvent);
        }

        private void HandleEvent(PrimaryGameStartedEvent evt)
        {
            IncreaseHandCount();

        }

        public void IncreaseHandCount()
        {
            _handCount++;

            _eventBus.Publish(new HandCountChangedEvent(HandCount));

            Logger.Info($"IncreaseHandCount:{HandCount}");
        }

        public void DecreaseHandCount(int n)
        {
            _handCount-=n;

            _eventBus.Publish(new HandCountChangedEvent(HandCount));
            Logger.Info($"DecreaseHandCount by {n} to {HandCount}");
        }

        public void ResetHandCount()
        {
            _handCount = 0;

            _eventBus.Publish(new HandCountChangedEvent(HandCount));
            Logger.Info($"ResetHandCount:{HandCount}");
        }
    }
}