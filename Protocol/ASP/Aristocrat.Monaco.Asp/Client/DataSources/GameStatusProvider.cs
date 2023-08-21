namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Contracts;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;

    public class GameStatusProvider : IGameStatusProvider
    {
        private readonly IEventBus _eventBus;
        private readonly IFundsTransferDisable _fundsTransferDisable;

        public GameStatusProvider(
            IEventBus eventBus,
            IFundsTransferDisable fundsTransferDisable)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _fundsTransferDisable = fundsTransferDisable ?? throw new ArgumentNullException(nameof(fundsTransferDisable));
        }

        public GameEnableStatus Status { get; private set; }

        public GameDisableReason Reason { get; private set; }

        public GameEnableStatus HostStatus { get; private set; }

        public GameDisableReason HostReason { get; private set; }

        public void SetGameStatus(GameEnableStatus status, GameDisableReason reason)
        {
            if (Status != status || Reason != reason)
            {
                Status = status;
                Reason = reason;
                _eventBus.Publish(new DacomGameStatusChangedEvent(Status, Reason));
            }
        }

        public void SetHostStatus(GameEnableStatus status, GameDisableReason reason)
        {
            if (HostStatus != status || HostReason != reason)
            {
                HostStatus = status;
                HostReason = reason;
            }
        }

        public void HandleEvent(SystemDisableRemovedEvent theEvent)
        {
            if (theEvent.SystemDisabled) return;
            SetGameStatus(GameEnableStatus.EnableGame, Reason);
        }

        public void HandleEvent(SystemDisableAddedEvent theEvent)
        {
            if (!theEvent.SystemIdleStateAffected) return;

            var gameStatus = _fundsTransferDisable.TransferOffDisabled
                ? GameEnableStatus.DisableGameDisallowCollect
                : GameEnableStatus.DisableGameAllowCollect;
            
            SetGameStatus(
                theEvent.DisableReasons == GetResourceString(ResourceKeys.ProgressiveDisable) ? HostStatus : gameStatus,
                ToGameDisableReason(theEvent.DisableReasons));
        }

        private GameDisableReason ToGameDisableReason(string reason)
        {
            if (reason == GetResourceString(ResourceKeys.LogicSealIsBroken)) return GameDisableReason.LogicSealBroken;
            if (reason == GetResourceString(ResourceKeys.ProgressiveDisconnectText)) return GameDisableReason.LinkProgressiveCommsFailure;
            if (reason == GetResourceString(ResourceKeys.ProgressiveDisable)) return HostReason;
            return GameDisableReason.OtherEgmLockups;
        }

        private static string GetResourceString(string key) => Localizer.For(CultureFor.Operator).GetString(key);
    }
}