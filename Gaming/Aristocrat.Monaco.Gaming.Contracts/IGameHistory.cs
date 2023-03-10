namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using Central;
    using Models;
    using Progressives;

    /// <summary>
    ///     Provides a mechanism to save game history.
    /// </summary>
    [CLSCompliant(false)]
    public interface IGameHistory : IFreeGameHistory
    {
        /// <summary>
        ///     Gets the log sequence number of the most recent game history record within the log; 0 (zero) if no records.
        /// </summary>
        long LogSequence { get; }

        /// <summary>
        ///     Gets the value of the pending currency In.
        /// </summary>
        long PendingCurrencyIn { get; }

        /// <summary>
        ///     Gets the log sequence number of the most recent game history log; null if there are no log records.
        /// </summary>
        IGameHistoryLog CurrentLog { get; }

        /// <summary>
        ///     Gets the total number of entries within the log.
        /// </summary>
        int TotalEntries { get; }

        /// <summary>
        ///     Gets the total number of entries before queue-cycling.
        /// </summary>
        int MaxEntries { get; }

        /// <summary>
        ///     Gets a value indicating whether we need to recover.
        /// </summary>
        bool IsRecoveryNeeded { get; }

        /// <summary>
        ///     Gets a value indicating whether game diagnostics active.
        /// </summary>
        bool IsDiagnosticsActive { get; }

        /// <summary>
        ///     Gets a value indicating whether we have a pending cashout.
        /// </summary>
        bool HasPendingCashOut { get; }

        /// <summary>
        ///     Gets a value indicating whether the last game had a fatal error.
        /// </summary>
        bool IsGameFatalError { get; }

        /// <summary>
        ///     Gets the last recorded play state.
        /// </summary>
        PlayState LastPlayState { get; }

        /// <summary>
        ///     Used to Adding Metering SnapShot in GameHistory Log, include persistent into DB
        /// </summary>
        void AddMeterSnapShotWithPersistentLog();

        /// <summary>
        ///     Used to start a game round in the escrowed state.  This will only be used for central determinant games
        /// </summary>
        /// <param name="initialWager">The initial wager.</param>
        /// <param name="data">The initial recovery blob.</param>
        void Escrow(long initialWager, byte[] data);

        /// <summary>
        ///     Used to indicate a game round has failed while in the escrowed state.  This will only be used for central
        ///     determinant games
        /// </summary>
        void Fail();

        /// <summary>
        ///     Used to add the results of an outcome request.  This will only be used for central determinant games
        /// </summary>
        /// <param name="outcomes">The central determinant outcomes</param>
        void AppendOutcomes(IEnumerable<Outcome> outcomes);

        /// <summary>
        ///     Used to save the starting game history.
        /// </summary>
        /// <param name="initialWager">The initial wager.</param>
        /// <param name="data">The initial recovery blob.</param>
        /// <param name="jackpotSnapshot">A jackpot snapshot</param>
        void Start(long initialWager, byte[] data, IEnumerable<Models.Jackpot> jackpotSnapshot);

        /// <summary>
        ///     Adds an additional wager to the game round.
        /// </summary>
        /// <param name="amount"></param>
        void AdditionalWager(long amount);

        /// <summary>
        ///     Logs win that is not committed to the game round.
        /// </summary>
        /// <param name="win">The uncommitted win for the primary game.</param>
        void IncrementUncommittedWin(long win);

        /// <summary>
        ///     Commits the win for the base game
        /// </summary>
        void CommitWin();

        /// <summary>
        ///     Ends the primary game round and sets the initial win.
        /// </summary>
        /// <param name="initialWin">The initial win from primary game, including progressive win amounts.</param>
        void EndPrimaryGame(long initialWin);

        /// <summary>
        ///     Indicates the player chose to play a secondary game
        /// </summary>
        void SecondaryGameChoice();

        /// <summary>
        ///     Starts the secondary game
        /// </summary>
        /// <param name="stake">The amount wagered for the secondary game.</param>
        void StartSecondaryGame(long stake);

        /// <summary>
        ///     Starts the secondary game
        /// </summary>
        /// <param name="win">The amount won for the secondary game.</param>
        void EndSecondaryGame(long win);

        /// <summary>
        ///     Used to post the final results.
        /// </summary>
        /// <param name="finalWin">The total amount won.</param>
        void Results(long finalWin);

        /// <summary>
        ///     Used to post game win bonus results
        /// </summary>
        /// <param name="win">The win results for the game win bonus</param>
        void AddGameWinBonus(long win);

        /// <summary>
        ///     Used to signify the presentation has completed
        /// </summary>
        void PresentationFinished();

        /// <summary>
        ///     Used to signify the final win has been paid.
        /// </summary>
        void PayResults();

        /// <summary>
        ///     Used to save game round specific text description required for logs.
        ///     An example of this might be the player picks.
        /// </summary>
        /// <param name="newDescriptions">List of game round descriptions to append.</param>
        void AppendGameRoundEventInfo(IList<string> newDescriptions);

        /// <summary>
        ///     Used to save jackpot hits that are associated to the game round.
        /// </summary>
        /// <param name="jackpot">The jackpot info to associate to the game round.</param>
        void AppendJackpotInfo(JackpotInfo jackpot);

        /// <summary>
        ///     Logs the game round details for this game
        /// </summary>
        /// <param name="details">The details to log</param>
        void LogGameRoundDetails(GameRoundDetails details);

        /// <summary>
        ///     Clears the game round when recovering
        /// </summary>
        void ClearForRecovery();

        /// <summary>
        ///     Logs a fatal game error.  This should be called when a legitimacy or liability check fails.
        /// </summary>
        void LogFatalError(GameErrorCode errorCode);

        /// <summary>
        ///     Used to end the game round.
        /// </summary>
        void EndGame();

        /// <summary>
        ///     Used to finalize the game history.
        /// </summary>
        void End();

        /// <summary>
        ///     Associates the specified transactions with the active game round
        /// </summary>
        /// <param name="transactions">The transactions to add to the game round</param>
        /// <param name="applyToFreeGame">Optionally, assign the amount out to the last free game</param>
        void AssociateTransactions(IEnumerable<TransactionInfo> transactions, bool applyToFreeGame = false);

        /// <summary>
        ///     Adds the specified cash out to the list of tracked cash outs
        /// </summary>
        /// <param name="cashOut"></param>
        void AppendCashOut(CashOutInfo cashOut);

        /// <summary>
        ///     Marks the associate cash out as complete
        /// </summary>
        /// <param name="referenceId">The reference/trace id of the cash out</param>
        void CompleteCashOut(Guid referenceId);

        /// <summary>
        ///     Gets the game history by the index in the log
        /// </summary>
        /// <param name="index">The index of the game history log.</param>
        /// <returns>A game history record if it exists.</returns>
        IGameHistoryLog GetByIndex(int index);

        /// <summary>
        ///     Retrieves the current game history data.
        /// </summary>
        /// <returns>a collection representing the current game history log.</returns>
        IEnumerable<IGameHistoryLog> GetGameHistory();

        /// <summary>
        ///     Retrieves recovery point data for a specific replay.
        /// </summary>
        /// <param name="replayIndex">The replayIndex relative to game history list.</param>
        /// <param name="data">The recovery point data retrieved.</param>
        /// <returns>A flag indicating whether or not the recovery point data was retrieved.</returns>
        bool LoadReplay(int replayIndex, out byte[] data);

        /// <summary>
        ///     Retrieves the last recovery point data.
        /// </summary>
        /// <param name="data">The recovery point data retrieved.</param>
        /// <returns>A flag indicating whether or not the recovery point data was retrieved.</returns>
        bool LoadRecoveryPoint(out byte[] data);

        /// <summary>
        ///     Used to save the given recovery point data.
        /// </summary>
        /// <param name="data">The recovery point data to save.</param>
        void SaveRecoveryPoint(byte[] data);

        /// <summary>
        ///     Logs the recovery data.
        /// </summary>
        /// <param name="data">The recovery data.</param>
        /// <param name="header">Log header text.</param>
        void LogRecoveryData(byte[] data, string header);

        /// <summary>
        ///     Used to accumulate the wager amount provided from the promo player bank
        /// </summary>
        /// <param name="amount">The wager amount</param>
        void AddPromoWager(long amount);
    }
}