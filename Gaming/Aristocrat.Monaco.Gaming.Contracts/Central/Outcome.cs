// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using Newtonsoft.Json;

    /// <summary>
    ///     Definition of a central outcome
    /// </summary>
    public class Outcome
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Outcome" /> class.
        /// </summary>
        /// <param name="id">The unique outcome identifier</param>
        /// <param name="gameSetId">The game set Id</param>
        /// <param name="subsetId">The subset Id</param>
        /// <param name="reference">The outcome reference</param>
        /// <param name="type">The outcome type</param>
        /// <param name="value">The outcome value</param>
        /// <param name="winLevelIndex">The win level index</param>
        /// <param name="lookupData">The host lookup data</param>
        /// <param name="gameId">The game id</param>
        /// <param name="gameIndex">The game index</param>
        [JsonConstructor]
        public Outcome(
            long id,
            long gameSetId,
            long subsetId,
            OutcomeReference reference,
            OutcomeType type,
            long value,
            int winLevelIndex,
            string lookupData,
            int gameId = 0,
            int gameIndex = 0)
        {
            Id = id;
            GameSetId = gameSetId;
            SubsetId = subsetId;
            Reference = reference;
            Type = type;
            Value = value;
            WinLevelIndex = winLevelIndex;
            LookupData = lookupData;
            GameId = gameId;
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     Gets the outcome identifier
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        ///     Gets the set identifier assigned by the host; identifies a specific deal
        /// </summary>
        public long GameSetId { get; private set; }

        /// <summary>
        ///     Gets the subset identifier assigned by the host; identifies a section of the deal
        /// </summary>
        public long SubsetId { get; private set; }

        /// <summary>
        ///     Gets the prize value reference
        /// </summary>
        public OutcomeReference Reference { get; private set; }

        /// <summary>
        ///     Gets the outcome type
        /// </summary>
        public OutcomeType Type { get; private set; }

        /// <summary>
        ///     Gets the outcome value
        /// </summary>
        public long Value { get; private set; }

        /// <summary>
        ///     Gets a value that identifies a specific winLevelIndex within a paytable
        /// </summary>
        public int WinLevelIndex { get; private set; }

        /// <summary>
        ///     Gets an optional manufacturer specific data used to assist the EGM in determining the characteristics of the
        ///     display to be presented to the player as a result of the outcome.
        /// </summary>
        public string LookupData { get; private set; }

        /// <summary>
        ///     Gets the GameId for the outcome.
        /// </summary>
        public int GameId { get; private set; }

        /// <summary>
        ///     Gets the GameIndex for the outcome.
        /// </summary>
        public int GameIndex { get; private set; }

        /// <summary>
        ///     Add a progressive win to the total value.
        /// </summary>
        /// <param name="amount">The amount of the progressive win</param>
        public void AddProgressiveWin(long amount)
        {
            Value += amount;
        }
    }
}