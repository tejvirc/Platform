namespace Aristocrat.Monaco.Gaming.Presentation.Services.IdleText;

public interface IIdleTextService
{
    /// <summary>
    ///     Gets the current idle text to display based on precedence:
    ///     Cabinet/Host, else Jurisdiction Override, else Default
    /// </summary>
    /// <returns>Idle text to display</returns>
    string? GetCurrentIdleText();

    /// <summary>
    ///     Sets the default idle text to display if not otherwise specified/overridden.
    ///     Strings come from Localizer service and may be localized.
    /// </summary>
    void SetDefaultIdleText(string? text);

    /// <summary>
    ///     Sets the cabinet or service-provided idle text to display (highest priority preference)
    ///     Strings are not localized but may contain multi-language text.
    /// </summary>
    void SetCabinetIdleText(string? text);

    /// <summary>
    ///     Sets the jurisdiction-specific idle text to display if not otherwise specified/overridden.
    ///     Strings come from jurisdiction resource files and may be localized.
    /// </summary>
    void SetJurisdictionIdleText(string? text);

    /// <summary>
    ///     Initializes idle text strings from cabinet property or default
    /// </summary>
    void InitializeDefaults();
}