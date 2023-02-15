namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Provides a mechanism for retrieving and transitioning the current game play state.
    /// </summary>
    public interface IGamePlayState : IService
    {
        /// <summary>
        ///     Gets a value indicating whether game play is currently enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        ///     Gets a value indicating whether the game is in play (not in a game-cycle), player input is enabled, player may
        ///     leave the game and browse the various games available on the EGM. Bonus awards MAY be paid while in the gameIdle
        ///     state. The player may adjust some game parameters such as lines bet or keno marks, without initiating a game-cycle.
        /// </summary>
        bool Idle { get; }

        /// <summary>
        ///     Gets a value indicating whether the presentation is idle but we are still in a game round, player input is enabled, player may
        ///     leave the game and browse the various games available on the EGM. Bonus awards MAY be paid while in the gameIdle
        ///     state. The player may adjust some game parameters such as lines bet or keno marks, without initiating a game-cycle.
        /// </summary>
        bool InPresentationIdle { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the game is currently in the middle of a round.
        /// </summary>
        bool InGameRound { get; }

        /// <summary>
        ///     Gets the current play state.
        /// </summary>
        PlayState CurrentState { get; }

        /// <summary>
        ///     Gets the uncommitted state
        /// </summary>
        /// <remarks>
        ///     This is the equivalent of dirty read and shouldn't be used outside of very specific use cases
        /// </remarks>
        PlayState UncommittedState { get; }

        /// <summary>
        ///     Gets the amount of time that the game will delay after a game has ended.  May be useful for bonus awards.
        /// </summary>
        TimeSpan GameDelay { get; }

        /// <summary>
        ///     Called to indicate a game round is starting.
        /// </summary>
        /// <returns>true if the state was advanced, otherwise false</returns>
        bool Prepare();

        /// <summary>
        ///     Escrows the wager for central determinant games
        /// </summary>
        /// <param name="initialWager">The initial wager</param>
        /// <param name="data">The initial recovery blob for the escrowed game.</param>
        /// <param name="request">The outcome request for the game round</param>
        /// <param name="recovering">true, if a game round is being recovered</param>
        /// <returns>true if the state was advanced, otherwise false</returns>
        bool EscrowWager(long initialWager, byte[] data, IOutcomeRequest request, bool recovering);

        /// <summary>
        ///     Called to indicate a failure to initialize a game round.
        ///     This is called when credits are unable to be escrowed.
        /// </summary>
        void InitializationFailed();

        /// <summary>
        ///     Called to indicate the game round has started.
        /// </summary>
        /// <param name="initialWager">The initial wager for the primary game.</param>
        /// <param name="data">The initial recovery blob for the primary game.</param>
        /// <param name="recovering">true, if a game round is being recovered</param>
        void Start(long initialWager, byte[] data, bool recovering);

        /// <summary>
        ///     Called to indicate the secondary game has started
        /// </summary>
        /// <param name="stake">The wager for the secondary game.</param>
        /// <param name="win">The win for the secondary game.</param>
        /// <param name="recovering">Indicated start is being called for a game that is recovering</param>
        void StartSecondaryGame(long stake, long win, bool recovering = false);

        /// <summary>
        ///     Called to set the final result of the game and end the game.  Typically called when the outcome has been presented
        ///     to the player.
        /// </summary>
        /// <param name="finalWin">Final amount won</param>
        void End(long finalWin);

        /// <summary>
        ///     Called to indicate the game round is in a faulted state.  This will prevent the state machine from progressing if
        ///     needed.
        /// </summary>
        void Faulted();

        /// <summary>
        ///     Sets or updates the amount of time that the game will delay after a game has ended.
        /// </summary>
        /// <param name="delay">The amount of time to delay.</param>
        void SetGameEndDelay(TimeSpan delay);

        /// <summary>
        ///     Holds the end of a game round and prevents transition to idle
        /// </summary>
        void SetGameEndHold(bool preventIdle);
    }
}