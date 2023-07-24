namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     Represents jackpot data typically associated with a game round
    /// </summary>
    [Serializable]
    public class JackpotInfo
    {
        /// <summary>
        ///     Gets or sets the transaction identifier
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the JackpotHit date time.
        /// </summary>
        public DateTime HitDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the pay method.
        /// </summary>
        public PayMethod PayMethod { get; set; }

        /// <summary>
        ///     Gets or sets the device identifier
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the progressive pack name
        /// </summary>
        public string PackName { get; set; }

        /// <summary>
        ///     Gets or sets the progressive level identifier
        /// </summary>
        public int LevelId { get; set; }

        /// <summary>
        ///     Gets or sets the win amount
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets or sets Gets or sets the win text.
        /// </summary>
        public string WinText { get; set; }

        /// <summary>
        ///     /// <summary>
        ///     Gets the WagerCredits in cents associated with the progressive level that's hit
        /// </summary>
        public long WagerCredits { get; set; }
/// <inheritdoc/>

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            JackpotInfo other = (JackpotInfo)obj;

            return TransactionId == other.TransactionId &&
                   HitDateTime == other.HitDateTime &&
                   PayMethod == other.PayMethod &&
                   DeviceId == other.DeviceId &&
                   PackName == other.PackName &&
                   LevelId == other.LevelId &&
                   WinAmount == other.WinAmount &&
                   WinText == other.WinText &&
                   WagerCredits == other.WagerCredits;
        }
/// <inheritdoc/>

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = TransactionId.GetHashCode();
                hashCode = (hashCode * 397) ^ HitDateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ PayMethod.GetHashCode();
                hashCode = (hashCode * 397) ^ DeviceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (PackName != null ? PackName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LevelId.GetHashCode();
                hashCode = (hashCode * 397) ^ WinAmount.GetHashCode();
                hashCode = (hashCode * 397) ^ (WinText != null ? WinText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ WagerCredits.GetHashCode();
                return hashCode;
            }
        }

    }
}