namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    /// <summary>
    /// This class holds language settings value.
    /// </summary>
    public class LanguageOption
    {
        /// <summary>
        ///     Check if the language is mandatory. A mandatory language
        /// can not be disabled by operator. If the language is from jurisdiction
        /// it will be mandatory and enabled by default. Each Jurisdiction has a list
        /// of mandatory languages that game has to support.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        ///     Locale of the language.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        ///     If the language is enabled(selectable by player) by default or operator.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
