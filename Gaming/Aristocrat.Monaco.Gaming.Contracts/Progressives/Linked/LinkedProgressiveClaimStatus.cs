namespace Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked
{
    using System;

    /// <summary>
    ///     Various states of a linked progressive claim. This is for tracking
    ///     the status of a claim with a host only. 
    /// </summary>
    public enum LinkedClaimState
    {
        /// <summary>
        ///     Indicates no active claims on this level
        /// </summary>
        None,

        /// <summary>
        ///     Initial state when a level is triggered or hit
        /// </summary>
        Hit,

        /// <summary>
        ///     State when a host claims or sets the level, but it is not confirmed or acknowledged
        /// </summary>
        Claimed,

        /// <summary>
        ///     State when a host acknowledges the claim and the level can be awarded.
        /// </summary>
        Awarded
    }

    /// <summary>
    ///     Used for tracking linked progressive claims with hosts
    /// </summary>
    public class LinkedProgressiveClaimStatus
    {
        /// <summary>
        ///     Gets or sets the transaction id associated with the claim
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the win amount associated with the claim
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets or sets the status of the claim. 
        /// </summary>
        public LinkedClaimState Status { get; set; }

        /// <summary>
        ///     Gets or sets the date or time of the level hit
        /// </summary>
        public DateTime HitTime { get; set; }

        /// <summary>
        ///     Gets or sets the claim expiration time
        /// </summary>
        public DateTime ExpiredTime { get; set; }

        /// <summary>
        ///     Converts the object to a string
        /// </summary>
        /// <returns>The string version of the object </returns>
        public override string ToString()
        {
            return "Claim Status: " +
                   $"TransactionId={TransactionId}, " +
                   $"WinAmount={WinAmount}, " +
                   $"ClaimStatus={Status}, " +
                   $"HitTime={HitTime}, " +
                   $"ExpiredTime={ExpiredTime}";
        }
    }
}