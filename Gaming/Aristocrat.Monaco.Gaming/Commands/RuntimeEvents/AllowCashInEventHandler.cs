namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using System;
    using Contracts;
    using Contracts.Events;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class AllowCashInEventHandler : IRuntimeEventHandler
    {
        private readonly IPlayerBank _bank;
        private readonly IEventBus _bus;
        private readonly IGameRecovery _recovery;
        private readonly IRuntime _runtime;
        private readonly IPropertiesManager _properties;

        public AllowCashInEventHandler(
            IRuntime runtime,
            IPlayerBank bank,
            IEventBus bus,
            IGameRecovery recovery,
            IPropertiesManager properties)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void HandleEvent(GameRoundEvent gameRoundEvent)
        {
            if (_recovery.IsRecovering)
            {
                _runtime.UpdateBalance(_bank.Credits);
            }

            // Ignoring game input here as Jurisdiction already allows money in during game play.
            if (_properties.GetValue(GamingConstants.AllowCashInDuringPlay, false))
            {
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