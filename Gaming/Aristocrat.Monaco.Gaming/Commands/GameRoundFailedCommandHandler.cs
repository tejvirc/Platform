namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;
    using Progressives;

    public class GameRoundFailedCommandHandler : ICommandHandler<GameRoundFailed>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _bus;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IPlayerBank _bank;

        public GameRoundFailedCommandHandler(IPlayerBank bank, IEventBus bus, IProgressiveGameProvider progressiveGameProvider)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _progressiveGameProvider = progressiveGameProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));
        }

        public void Handle(GameRoundFailed command)
        {
            // Clear the wager amounts if they were set
            _progressiveGameProvider.SetProgressiveWagerAmounts(new List<long>());

            _bus.Publish(new GameRequestFailedEvent());

            _bus.Publish(new DisableCountdownTimerEvent(false));

            Logger.Warn("Game round failed.  Any open transactions will be abandoned");

            _bank.Unlock();
        }
    }
}
