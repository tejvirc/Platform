namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    /// <summary>
    ///     Manages mapping progressives defined on the back-end with the Platform progressive configuration.
    /// </summary>
    public interface IProgressiveController
    {
        /// <summary>
        ///     Awards the progressive pool win to a progressive level.
        /// </summary>
        /// <param name="poolName">The name of the pool defined on the back-end.</param>
        /// <param name="amountInPennies">The amount to award.</param>
        void AwardJackpot(string poolName, long amountInPennies);

        /// <summary>
        ///     Configures the progressives for the game configuration.
        /// </summary>
        void Configure();
    }
}
