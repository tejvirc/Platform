namespace Aristocrat.Mgam.Client.Options
{
    using SimpleInjector;

    /// <summary>
    ///     Use to register services based on specified options.
    /// </summary>
    public interface IOptionsExtension
    {
        /// <summary>
        ///     Register services to the container.
        /// </summary>
        /// <param name="container"></param>
        void RegisterServices(Container container);
    }
}
