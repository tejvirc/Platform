namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Provides a mechanism to interact with a central determinant handler
    /// </summary>
    public interface ICentralHandler
    {
        /// <summary>
        ///     Gets one or more outcomes from a central determinant provider
        /// </summary>
        /// <param name="transaction">The central transaction</param>
        /// <param name="isRecovering">Whether call to request outcomes came while game was recovering.</param>
        /// <returns>zero of more outcomes</returns>
        Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false);
    }
}