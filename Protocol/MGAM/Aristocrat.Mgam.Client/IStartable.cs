namespace Aristocrat.Mgam.Client
{
    using System.Threading.Tasks;

    /// <summary>
    ///     
    /// </summary>
    internal interface IStartable
    {
        /// <summary>
        ///     Initializes resources.
        /// </summary>
        /// <returns></returns>
        Task Start();

        /// <summary>
        ///     Checks to see if the service is able to be started.
        /// </summary>
        /// <returns>A value that indicates whether the service is able to be started.</returns>
        bool CanStart();

        /// <summary>
        ///     Synchronizes shutting down resources.
        /// </summary>
        /// <returns></returns>
        Task Stop();
    }
}
