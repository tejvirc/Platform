namespace Aristocrat.Mgam.Client.Options
{
    /// <summary>
    ///     Manages options extensions.
    /// </summary>
    public interface IOptionsBuilder
    {
        /// <summary>
        ///     Adds or updates an extension.
        /// </summary>
        /// <param name="extension"></param>
        void AddOrUpdateExtension(IOptionsExtension extension);

        /// <summary>
        ///     Gets options extender.
        /// </summary>
        OptionsExtender Options { get; }
    }
}
