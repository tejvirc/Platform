namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using System;
    using Contracts;
    using Contracts.Events;
    using Kernel;
    using Runtime.Client;

    public class AllowCashInEventHandler : IRuntimeEventHandler
    {
        private readonly IPlayerBank _bank;
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;

        public AllowCashInEventHandler(
            IPlayerBank bank,
            IEventBus bus,
            IPropertiesManager properties)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void HandleEvent(GameRoundEvent gameRoundEvent)
        {
            // Ignoring game input here as Jurisdiction already allows money in during game play.
            if (_properties.GetValue(GamingConstants.AllowCashInDuringPlay, false))
            {
                if (gameRoundEvent.Action == GameRoundEventAction.Begin)
                {
                    _bank.Unlock();
                }
                return;
            }

            switch (gameRoundEvent.Action)
            {
                case GameRoundEventAction.Begin:
                    _bank.Unlock();
                    _bus.Publish(new AllowMoneyInEvent());
                    break;
                case GameRoundEventAction.Completed:
                    _bus.Publish(new ProhibitMoneyInEvent());
                    _bank.Lock();
                    break;
            }
        }
    }
}