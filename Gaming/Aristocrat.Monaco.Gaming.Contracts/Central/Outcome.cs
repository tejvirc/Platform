// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    ///     Definition of a central outcome
    /// </summary>
    [Serializable]
    public class Outcome : IEquatable<Outcome>
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
        [JsonConstructor]
        public Outcome(
            long id,
            long gameSetId,
            long subsetId,
            OutcomeReference reference,
            OutcomeType type,
            long value,
            int winLevelIndex,
            string lookupData)
        {
            Id = id;
            GameSetId = gameSetId;
            SubsetId = subsetId;
            Reference = reference;
            Type = type;
            Value = value;
            WinLevelIndex = winLevelIndex;
            LookupData = lookupData;
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
        ///     Implements the Equal operator
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Outcome other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id &&
                   GameSetId == other.GameSetId &&
                   SubsetId == other.SubsetId &&
                   Reference == other.Reference &&
                   Type == other.Type &&
                   Value == other.Value &&
                   WinLevelIndex == other.WinLevelIndex &&
                   LookupData == other.LookupData;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Outcome)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ GameSetId.GetHashCode();
                hashCode = (hashCode * 397) ^ SubsetId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Reference;
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                hashCode = (hashCode * 397) ^ WinLevelIndex;
                hashCode = (hashCode * 397) ^ (LookupData != null ? LookupData.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}