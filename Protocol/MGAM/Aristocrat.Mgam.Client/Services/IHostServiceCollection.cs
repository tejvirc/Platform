namespace Aristocrat.Mgam.Client.Services
{
    /// <summary>
    ///     Contains a collection of client services.
    /// </summary>
    internal interface IHostServiceCollection
    {
        /// <summary>
        ///     Gets a reference to a <see cref="IHostService"/> instance.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        TService GetService<TService>()
            where TService : IHostService;

        /// <summary>
        ///     Adds a host service.
        /// </summary>
        /// <param name="service"><see cref="IHostService"/>.</param>
        void Add(IHostService service);

        /// <summary>
        ///     Removes a host service.
        /// </summary>
        /// <param name="service"><see cref="IHostService"/>.</param>
        void Remove(IHostService service);
    }
}
