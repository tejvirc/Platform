namespace Aristocrat.Monaco.Asp.Progressive
{
    /// <summary>
    /// Model that represents state of a single progressive level
    /// </summary>
    public interface ILinkProgressiveLevel
    {
        /// <summary>
        /// The level id
        /// </summary>
        int LevelId { get; }

        /// <summary>
        /// The name of the level
        /// </summary>
        string Name { get; }

        //These 3 properties are driven by meters which track per device and level
        //The rest use meters that track per level only

        /// <summary>
        /// The total amount won
        /// </summary>
        long TotalAmountWon { get; }

        /// <summary>
        /// The total jackpot hit count
        /// </summary>
        long TotalJackpotHitCount { get; }

        /// <summary>
        /// The jackpot reset counter
        /// </summary>
        long JackpotResetCounter { get; }

        /// <summary>
        /// The progressive jackpot amount update
        /// </summary>
        long ProgressiveJackpotAmountUpdate { get; }

        /// <summary>
        /// The jackpot hit status
        /// </summary>
        long JackpotHitStatus { get; }

        /// <summary>
        /// The jackpot hit amount Won
        /// </summary>
        long LinkJackpotHitAmountWon { get; }

        /// <summary>
        /// The current jackpot number
        /// </summary>
        long CurrentJackpotNumber { get; }

        /// <summary>
        /// Integer representation of first block of jackpot controller id
        /// </summary>
        int JackpotControllerIdByteOne { get; }

        /// <summary>
        /// Integer representation of middle block of jackpot controller id
        /// </summary>
        int JackpotControllerIdByteThree { get; }

        /// <summary>
        /// Integer representation of last block of jackpot controller id
        /// </summary>
        int JackpotControllerIdByteTwo { get; }
    }
}