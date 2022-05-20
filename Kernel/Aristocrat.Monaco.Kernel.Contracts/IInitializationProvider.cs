namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     Defines the interface for the InitializationProvider
    /// </summary>
    public interface IInitializationProvider
    {
        /// <summary>
        ///     Called by the GamingRunnable to indicate that system initialization is complete
        /// </summary>
        void SystemInitializationCompleted();

        /// <summary>
        ///     Indicates whether system initialization has completed or not
        /// </summary>
        bool IsSystemInitializationComplete { get; }
    }
}