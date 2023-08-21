namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     The state of the game play activity.
    /// </summary>
    public enum PlayState
    {
        /// <summary>
        ///     In Idle state, the game is not in play (not in a game-cycle), player input is enabled, player may leave the game
        ///     and browse the various games available on the EGM. Bonus awards MAY be paid while in the gameIdle state. The player
        ///     may adjust some game parameters such as lines bet or keno marks, without initiating a game-cycle.
        /// </summary>
        Idle,

        /// <summary>
        ///     In Initiated state, game play has been initiated, but there is insufficient data to initiate recovery.  If the game
        ///     process exits in this state nothing will be recorded in the game history.
        /// </summary>
        Initiated,

        /// <summary>
        ///     The PrimaryGameEscrow state only applies when a central determinant game is being played. It indicates that the
        ///     player has started a primary game-cycle with a wager; however the wager is in escrow and has NOT been committed
        ///     yet. This state will remain until either the central determination logic has provided a valid primary game outcome,
        ///     or when the central determination logic has failed to obtain a valid primary game outcome.
        /// </summary>
        PrimaryGameEscrow,

        /// <summary>
        ///     The PrimaryGameStarted state indicates that a primary game-cycle has been started by the player and the initial
        ///     wager has been committed to the accounting meters. This state also indicates that the ‘primary-game-cycle’ can not
        ///     be aborted. Some game types may permit additional wagers (blackjack double-down, insurance, split, etc.) while in
        ///     the primary game cycle, therefore additional wager changes may be committed to the accounting meters while in this
        ///     state.
        /// </summary>
        PrimaryGameStarted,

        /// <summary>
        ///     The results for the primary game-cycle have been determined and any winnings have been paid.
        /// </summary>
        PrimaryGameEnded,

        /// <summary>
        ///     The ProgressivePending state only applies when a game hits one or more progressive wins. Game play suspend when
        ///     waiting for the progressive logic to provide a winning value.
        /// </summary>
        ProgressivePending,

        /// <summary>
        ///     The SecondaryGameChoice state allows the player to select whether the primary game winnings or winnings from
        ///     previous secondary game cycles within the same overall game cycle may be risked in a secondary game, such as
        ///     double-or-nothing.
        /// </summary>
        SecondaryGameChoice,

        /// <summary>
        ///     The SecondaryGameEscrow state only applies when a central determinant game is being played. It indicates that the
        ///     player has requested to start a secondary game cycle with a wager; however the initial wager is in escrow and has
        ///     NOT been committed at this state. This state will remain until either the central determination logic has provided
        ///     a valid secondary game outcome, or when the central determination logic has failed to provide a valid secondary
        ///     game outcome.
        /// </summary>
        SecondaryGameEscrow,

        /// <summary>
        ///     The SecondaryGameStarted state indicates that a secondary game cycle has been initiated by the player and the
        ///     secondary wager has been committed to the accounting meters. This state also the secondary game cycle and cannot be
        ///     aborted.
        /// </summary>
        SecondaryGameStarted,

        /// <summary>
        ///     The results for the secondary game-cycle have been determined and any final winnings have been recorded.
        /// </summary>
        SecondaryGameEnded,

        /// <summary>
        ///     The PayGameResults state indicates the final win for the game cycle has been determined and payment is in progress.
        ///     The game will remain in the payGameResults states until all payments have completed. This state also indicates that
        ///     the primary game and any secondary games associated with this game cycle have concluded.
        /// </summary>
        PayGameResults,

        /// <summary>
        ///     In FatalError state, the game has triggered a fatal error.  This state is entered due to a failed liability or legitimacy check.
        /// </summary>
        FatalError,

        /// <summary>
        ///     The GameEnded state indicates the results for the game cycle has been determined any winnings have been paid, thus
        ///     ending the game-cycle. While in the gameEnded state, no player input will be accepted. This state provides a
        ///     ‘delay’ before returning input to the player. Bonus awards MAY be paid while in the gameEnded state.
        /// </summary>
        GameEnded,

        /// <summary>
        ///     The PresentationIdle state indicates the results for the game cycle has been determined any winnings have been paid, thus
        ///     ending the game-cycle. Player input is accepted and will finalize the game if play is initiated.  This state provides a
        ///     ‘delay’ before closing the game round. All money operations are permitted while in the PresentationIdle.
        /// </summary>
        PresentationIdle
    }
}