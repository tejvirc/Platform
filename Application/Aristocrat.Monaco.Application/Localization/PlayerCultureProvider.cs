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
    public class PlayerCultureProvider : CultureProvider
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureProvider"/> class.
        /// </summary>
        public PlayerCultureProvider()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ILocalization>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureProvider"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/> instance.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/> instance.</param>
        /// <param name="localization"><see cref="ILocalization"/> instance.</param>
        public PlayerCultureProvider(
            IEventBus eventBus,
            IPropertiesManager properties,
            ILocalization localization)
            : base(localization)
        {
            _properties = properties;
            _eventBus = eventBus;
        }

        [JsonProperty]
        private CultureInfo PrimaryCulture { get; set; }

        /// <inheritdoc />
        public override string ProviderName => CultureFor.Player;

        /// <inheritdoc />
        protected override string CurrentCulturePropertyName =>
            ApplicationConstants.LocalizationPlayerCurrentCulture;

        /// <inheritdoc />
        protected override void OnConfigure()
        {
            var locales = (string[])_properties.GetProperty(ApplicationConstants.LocalizationPlayerAvailable, new[] { CultureInfo.CurrentCulture.Name });
            var primaryLocale = (string)_properties.GetProperty(ApplicationConstants.LocalizationPlayerPrimary, locales.First());

            if (locales.All(x => !string.Equals(x, primaryLocale, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new LocalizationException($"Configuration error Player primary locale not supported, {primaryLocale} not in ({string.Join(",", locales)})");
            }

            PrimaryCulture = CultureInfo.GetCultureInfo(primaryLocale);

            AddCultures(locales.Select(CultureInfo.GetCultureInfo).ToArray());

            SetCurrentCulture(PrimaryCulture);

            _eventBus.Subscribe<SetValidationEvent>(this, Handle);
        }

        /// <inheritdoc />
        protected override void OnCultureChanged(CultureInfo oldCulture, CultureInfo newCulture)
        {
            base.OnCultureChanged(oldCulture, newCulture);

            _eventBus.Publish(new PlayerCultureChangedEvent(oldCulture, newCulture, PrimaryCulture?.Equals(newCulture) ?? true));

            SwitchTo();
        }

        /// <inheritdoc />
        protected override void OnCultureAdded(IEnumerable<CultureInfo> cultures)
        {
            _eventBus.Publish(new PlayerCultureAdded(cultures));
        }

        /// <inheritdoc />
        protected override void OnCultureRemoved(IEnumerable<CultureInfo> cultures)
        {
            _eventBus.Publish(new PlayerCultureAdded(cultures));
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
                    : PrimaryCulture);
        }
    }
}
