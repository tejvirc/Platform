namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    /// Definition of the PlayerLanguageChangedEvent class
    /// </summary>
    public class PlayerLanguageChangedEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLanguageChangedEvent" /> class.
        /// </summary>
        /// <param name="localeCode">Is this the first prompt.</param>
        public PlayerLanguageChangedEvent(string localeCode)
        {
            LocaleCode = localeCode;
        }

        /// <summary>
        ///  LocaleCode of the Language Change
        /// </summary>
        public string LocaleCode { get; }
    }
}
