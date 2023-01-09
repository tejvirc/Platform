namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;


    /// <summary>
    ///     Contract for hand count session service instance.
    /// </summary>
    public interface IHandCountSessionService : IService
    {
        /// <summary>
        ///     Gets hand count
        /// </summary>
        /// <returns>GameCategorySetting</returns>
        int HandCount { get; }

        /// <summary>
        ///     Increase hand count
        /// </summary>
        void IncreaseHandCount();

        /// <summary>
        ///     Decrease hand count
        /// <param name="n">Decrease hand count by n.</param>
        /// </summary>
        void DecreaseHandCount(int n);
        /// <summary>
        ///     Reset hand count
        /// </summary>
        void ResetHandCount();
    }
}