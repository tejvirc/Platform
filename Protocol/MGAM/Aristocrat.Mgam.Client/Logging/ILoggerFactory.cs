namespace Aristocrat.Mgam.Client.Logging
{
    /// <summary>
    ///     Creates new logger instances.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        ///     Creates a new logger instance with <typeparamref name="TCategory"/> type as the category.
        /// </summary>
        /// <typeparam name="TCategory"></typeparam>
        /// <returns><see cref="ILogger"/>.</returns>
        ILogger Create<TCategory>()
            where TCategory : class;
    }
}
