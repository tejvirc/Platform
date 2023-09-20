namespace Aristocrat.Monaco.Gaming.Presentation.Services.IdleText
{
    public interface IIdleTextService
    {
        /// <summary>
        ///     Gets the cabinet specified idle text to display, which is generally highest precedence
        /// </summary>
        /// <returns>Default text to display</returns>
        string? GetCabinetIdleText();

        /// <summary>
        ///     Gets the default idle text to display if not otherwise specified/overridden
        /// </summary>
        /// <returns></returns>
        string? GetDefaultIdleText();
    }
}
