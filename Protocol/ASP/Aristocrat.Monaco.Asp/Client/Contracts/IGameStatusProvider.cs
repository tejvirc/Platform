namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using Kernel;

    /// <summary>
    ///     This interface defines methods that gets and sets the Game Status
    /// </summary>
    public interface IGameStatusProvider
    {
        /// <summary>
        ///     Gets Game Status
        /// </summary>
        GameEnableStatus Status { get; }

        /// <summary>
        ///     Gets Game Disabled Reason
        /// </summary>
        GameDisableReason Reason { get; }

        /// <summary>
        ///     Gets Status by Host
        /// </summary>
        GameEnableStatus HostStatus { get; }

        /// <summary>
        ///     Gets Game Disabled Reason by Host
        /// </summary>
        GameDisableReason HostReason { get; }

        /// <summary>
        ///     Sets Game Status and Disabled Reason
        /// </summary>
        void SetGameStatus(GameEnableStatus status, GameDisableReason reason);

        /// <summary>
        ///     Sets Game Status and Disabled Reason by Host
        /// </summary>
        void SetHostStatus(GameEnableStatus status, GameDisableReason reason);

        /// <summary>
        ///     Handles SystemDisableAddedEvent
        /// </summary>
        /// <param name="theEvent"></param>
        void HandleEvent(SystemDisableAddedEvent theEvent);

        /// <summary>
        ///     Handle SystemDisableRemovedEvent
        /// </summary>
        /// <param name="theEvent"></param>
        void HandleEvent(SystemDisableRemovedEvent theEvent);
    }

    public enum GameEnableStatus
    {
        EnableGame = 0x00,
        DisableGameAllowCollect = 0x01,
        DisableGameDisallowCollect = 0x02
    }

    public enum GameDisableReason
    {
        OtherEgmLockups = 0x00,
        HostInitiated = 0x01,
        LinkProgressiveCommsFailure = 0x02,
        VenueShutdown = 0x03,
        TemporarilyUnavailable = 0x04,
        Emergency = 0x05,
        SoftwareSignatureFailure = 0x06,
        LargeWin = 0x07,
        PowerUp = 0x08,
        LogicSealBroken = 0x09
    }
}