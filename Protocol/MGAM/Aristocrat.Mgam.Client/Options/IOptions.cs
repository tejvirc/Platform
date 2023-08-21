namespace Aristocrat.Mgam.Client.Options
{
    /// <summary>
    ///     Used to retrieve configured TOptions instances.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IOptions<out TOptions>
        where TOptions : class
    {
        /// <summary>
        ///     Gets the options instance.
        /// </summary>
        TOptions Value { get; }
    }
}
