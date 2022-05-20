namespace Aristocrat.Monaco.Application.Localization
{
    using System.Globalization;
    using Contracts;
    using Contracts.Localization;
    using Kernel;

    /// <summary>
    ///     Implements localization logic for operator ticket culture provider.
    /// </summary>
    public class OperatorTicketCultureProvider : CultureProvider
    {
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorTicketCultureProvider"/> class.
        /// </summary>
        public OperatorTicketCultureProvider()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ILocalization>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorTicketCultureProvider"/> class.
        /// </summary>
        /// <param name="properties"><see cref="IPropertiesManager"/> instance.</param>
        /// <param name="localization"></param>
        public OperatorTicketCultureProvider(
            IPropertiesManager properties,
            ILocalization localization)
            : base(localization)
        {
            _properties = properties;
        }

        /// <inheritdoc />
        public override string ProviderName => CultureFor.OperatorTicket;

        /// <inheritdoc />
        protected override string CurrentCulturePropertyName =>
            ApplicationConstants.LocalizationOperatorTicketCurrentCulture;

        /// <inheritdoc />
        protected override void OnConfigure()
        {
            var locale = (string)_properties.GetProperty(ApplicationConstants.LocalizationOperatorTicketLocale, string.Empty);

            if (string.IsNullOrEmpty(locale))
            {
                locale = CultureInfo.CurrentCulture.Name;
                _properties.SetProperty(ApplicationConstants.LocalizationOperatorTicketLocale, locale);
            }

            var selectedCulture = CultureInfo.GetCultureInfo(locale);

            AddCultures(selectedCulture);

            SetCurrentCulture(selectedCulture);
        }
    }
}
