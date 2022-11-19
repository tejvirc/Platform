namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Newtonsoft.Json;

    /// <summary>
    ///     Implements localization logic for player culture provider.
    /// </summary>
    public class PlayerCultureProvider : CultureProvider, IPlayerCultureProvider
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private CultureInfo _defaultCulture;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureProvider"/> class.
        /// </summary>
        public PlayerCultureProvider()
            : base(ServiceManager.GetInstance().GetService<ILocalization>())
        {
            _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        }
        

        [JsonProperty]
        public CultureInfo PrimaryCulture { get; private set; }

        [JsonProperty]
        public CultureInfo DefaultCulture
        {
            get => _defaultCulture;
            set
            {
                if (Equals(_defaultCulture, value))
                {
                    return;
                }

                _defaultCulture = value;
                RaisePropertyChangedEvent(nameof(DefaultCulture));
            }
        }
        
        /// <inheritdoc />
        public override string ProviderName => CultureFor.Player;

        [JsonProperty]
        public IList<LanguageOption> LanguageOptions { get; } = new List<LanguageOption>();

        /// <inheritdoc />
        protected override string CurrentCulturePropertyName =>
            ApplicationConstants.LocalizationPlayerCurrentCulture;

        /// <inheritdoc />
        protected override void OnConfigure()
        {
            var locales = GetLanguageLocales();

            AddCultures(locales.Select(CultureInfo.GetCultureInfo).ToArray());

            SetCurrentCulture(CurrentCulture ?? PrimaryCulture);

            _eventBus.Subscribe<SetValidationEvent>(this, Handle);
        }

        /// <inheritdoc />
        protected override void OnCultureChanged(CultureInfo oldCulture, CultureInfo newCulture)
        {
            base.OnCultureChanged(oldCulture, newCulture);

            _eventBus.Publish(new PlayerCultureChangedEvent(oldCulture, newCulture));

            SwitchTo();
        }

        /// <inheritdoc />
        protected override void OnCultureAdded(IEnumerable<CultureInfo> cultures)
        {
            if (cultures == null)
            {
                return;
            }

            var cultureList = cultures.ToList();
            foreach (var cultureName in cultureList.Select(c => c.Name))
            {
                var option = LanguageOptions.FirstOrDefault(
                    l => string.Equals(l.Locale, cultureName, StringComparison.InvariantCultureIgnoreCase));
                if (option == null)
                {
                    LanguageOptions.Add(new LanguageOption { IsMandatory = false, Locale = cultureName });
                    RaisePropertyChangedEvent(nameof(LanguageOptions));
                }
            }

            _eventBus.Publish(new PlayerCultureAdded(cultureList));
        }

        /// <inheritdoc />
        protected override void OnCultureRemoved(IEnumerable<CultureInfo> cultures)
        {
            if (cultures == null)
            {
                return;
            }

            var cultureList = cultures.ToList();
            foreach (var culture in cultureList)
            {
                var option = LanguageOptions.FirstOrDefault(
                    l => string.Equals(l.Locale, culture.Name, StringComparison.InvariantCultureIgnoreCase));
                if (option != null)
                {
                    LanguageOptions.Remove(option);
                    RaisePropertyChangedEvent(nameof(LanguageOptions));
                }
            }

            _eventBus.Publish(new PlayerCultureRemoved(cultureList));
        }

        private IEnumerable<string> GetLanguageLocales()
        {
            var playerLocales = (string[])_properties.GetProperty(ApplicationConstants.LocalizationPlayerAvailable, new[] { CultureInfo.CurrentCulture.Name });
            var playerPrimaryLocale = (string)_properties.GetProperty(ApplicationConstants.LocalizationPlayerPrimary, playerLocales.First());

            if (playerLocales.All(x => !string.Equals(x, playerPrimaryLocale, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new LocalizationException($"Configuration error Player primary locale not supported, {playerPrimaryLocale} not in ({string.Join(",", playerLocales)})");
            }

            if (!LanguageOptions.Any(l => l.IsMandatory))
            {
                LanguageOptions.Clear();
                Array.ForEach(playerLocales, l => LanguageOptions.Add(new LanguageOption { Locale = l, IsMandatory = true }));
            }

            PrimaryCulture ??= CultureInfo.GetCultureInfo(playerPrimaryLocale);

            DefaultCulture ??= PrimaryCulture ?? (LanguageOptions.Any() ? new CultureInfo(LanguageOptions.First().Locale) : new CultureInfo(ApplicationConstants.DefaultLanguage));

            return from l in LanguageOptions select l.Locale;
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

        private void Handle(SetValidationEvent evt)
        {
            SetCurrentCulture(
                !string.IsNullOrWhiteSpace(evt.Identity?.LocaleId)
                    ? CultureInfo.GetCultureInfo(evt.Identity.LocaleId)
                    : CurrentCulture ?? PrimaryCulture);
        }
    }
}
