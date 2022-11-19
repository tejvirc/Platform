namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Player culture provider interface.
    /// </summary>
    public interface IPlayerCultureProvider : ICultureProvider
    {
        /// <summary>
        ///     The default language culture.
        /// </summary>
        CultureInfo DefaultCulture { get; set; }

        /// <summary>
        ///     List of languages, including both jurisdiction languages and game supported languages.
        /// </summary>
        IList<LanguageOption> LanguageOptions { get; }
    }
}
