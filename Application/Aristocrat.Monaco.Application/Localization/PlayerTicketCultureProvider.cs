namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Contracts;
    using Contracts.Localization;
    using Kernel;

    public class PlayerTicketCultureProvider : CultureProvider
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly ILocalization _localization;

        /// <summary>
        ///     Implements localization logic for player ticket culture provider.
        /// </summary>
        public PlayerTicketCultureProvider()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ILocalization>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerTicketCultureProvider"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/> instance.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/> instance.</param>
        /// <param name="localization"><see cref="ILocalization"/> instance.</param>
        public PlayerTicketCultureProvider(
            IEventBus eventBus,
            IPropertiesManager properties,
            ILocalization localization)
            : base(localization)
        {
            _eventBus = eventBus;
            _properties = properties;
            _localization = localization;
        }

        /// <inheritdoc />
        public override string ProviderName => CultureFor.PlayerTicket;

        /// <inheritdoc />
        protected override string CurrentCulturePropertyName =>
            ApplicationConstants.LocalizationPlayerTicketCurrentCulture;

        /// <inheritdoc />
        protected override void OnConfigure()
        {
            var locale = _properties.GetValue(ApplicationConstants.LocalizationPlayerTicketLocale, string.Empty);

            if (string.IsNullOrEmpty(locale))
            {
                locale = CultureInfo.CurrentCulture.Name;
                _properties.SetProperty(ApplicationConstants.LocalizationPlayerTicketLocale, locale);
            }

            _properties.SetProperty(ApplicationConstants.LocalizationPlayerTicketDefault, locale);

            var supportedLocales = _localization.SupportedCultures
                .Where(culture => !string.IsNullOrEmpty(culture.Name))
                .Select(culture => culture.Name)
                .ToList();

            var playerTicketSelectableLocales = _properties.GetValue(ApplicationConstants.LocalizationPlayerTicketSelectable,
                new []
                {
                    new PlayerTicketSelectionArrayEntry
                    {
                        Locale = locale,
                        CurrencyValueLocale = string.Empty,
                        CurrencyWordsLocale = string.Empty
                    }
                });

            //var playerTicketSelectableLocales = _properties.GetValue(ApplicationConstants.LocalizationPlayerTicketSelectable, new PlayerTicketSelectionArrayEntry[] { });
            foreach (var playerTicketSelectableLocale in playerTicketSelectableLocales)
            {
                if (supportedLocales.All(x => !string.Equals(x, playerTicketSelectableLocale.Locale, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new LocalizationException($"Configuration error PlayerTicket locale {playerTicketSelectableLocale.Locale} not supported");
                }
            }

            var selectedCulture = CultureInfo.GetCultureInfo(locale);

            AddCultures(selectedCulture);

            SetCurrentCulture(selectedCulture);

            _eventBus.Subscribe<PropertyChangedEvent>(this, HandleLocalizationPlayerCurrentCulture, e => e.PropertyName == ApplicationConstants.LocalizationPlayerCurrentCulture);
            _eventBus.Subscribe<PropertyChangedEvent>(this, HandleLocalizationPlayerTicketLocale, e => e.PropertyName == ApplicationConstants.LocalizationPlayerTicketLocale);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }
        }

        private void HandleLocalizationPlayerCurrentCulture(PropertyChangedEvent evt)
        {
            var playerCurrentCulture = _properties.GetValue(ApplicationConstants.LocalizationPlayerCurrentCulture, null as string);
            if (playerCurrentCulture == null)
            {
                return;
            }

            var playerTicketOverride = _properties.GetValue(ApplicationConstants.LocalizationPlayerTicketOverride, true);
            if (playerTicketOverride)
            {
                return;
            }

            SetCurrentCulture(CultureInfo.GetCultureInfo(playerCurrentCulture));
            var playerTicketLocale = _properties.GetValue(ApplicationConstants.LocalizationPlayerTicketLocale, string.Empty);
            if (playerCurrentCulture != playerTicketLocale)
            {
                _properties.SetProperty(ApplicationConstants.LocalizationPlayerTicketLocale, playerCurrentCulture);
            }
        }

        private void HandleLocalizationPlayerTicketLocale(PropertyChangedEvent evt)
        {
            var playerTicketLocale = _properties.GetValue<string>(ApplicationConstants.LocalizationPlayerTicketLocale, null);

            if (playerTicketLocale == null || playerTicketLocale == CurrentCulture.Name)
            {
                return;
            }

            var playerTicketCulture = CultureInfo.GetCultureInfo(playerTicketLocale);

            if (IsCultureAvailable(playerTicketCulture))
            {
                SetCurrentCulture(playerTicketCulture);
            }
        }
    }
}
