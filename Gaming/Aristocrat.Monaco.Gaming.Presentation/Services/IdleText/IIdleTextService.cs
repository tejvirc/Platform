namespace Aristocrat.Monaco.Gaming.Presentation.Services.IdleText
{
    public interface IIdleTextService
    {
        /// <summary>
        ///     Gets the default idle text to display if not otherwise specified/overridden
        /// </summary>
        /// <returns>Default text to display</returns>
        string GetDefaultIdleText();
    }
}
