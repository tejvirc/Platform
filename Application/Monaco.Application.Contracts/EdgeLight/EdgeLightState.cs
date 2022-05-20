namespace Aristocrat.Monaco.Application.Contracts.EdgeLight
{
    /// <summary>
    ///     States the different Edge Light States
    /// </summary>
    public enum EdgeLightState
    {
        /// <summary>
        ///     The state where Door is Open
        /// </summary>
        DoorOpen,

        /// <summary>
        ///     CashOut state
        /// </summary>
        Cashout,

        /// <summary>
        ///     Lobby State
        /// </summary>
        Lobby,

        /// <summary>
        ///     Audit Menu State
        /// </summary>
        OperatorMode,

        /// <summary>
        /// Puts edge light into tower light mode.
        /// </summary>
        TowerLightMode,

        /// <summary>
        /// EdgeLighting Override on Idle and Zero Credit
        /// </summary>
        AttractMode,

        /// <summary>
        /// Default mode to control edgelight when game/anything is not driving edgelight
        /// </summary>
        DefaultMode
    }
}