namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using Newtonsoft.Json;

    /// <summary>
    ///     Implements localization logic for operator culture provider.
    /// </summary>
    public class OperatorCultureProvider : CultureProvider
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorCultureProvider"/> class.
        /// </summary>
        public OperatorCultureProvider()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ILocalization>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorCultureProvider"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/> instance.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/> instance.</param>
        /// <param name="localization"></param>
        public OperatorCultureProvider(
            IEventBus eventBus,
            IPropertiesManager properties,
            ILocalization localization)
            : base(localization)
        {
            _eventBus = eventBus;
            _properties = properties;
        }

        /// <summary>
        ///   Gets or sets the default culture for the current OperatorCultureProvider.
        /// </summary>
        [JsonProperty]
        public CultureInfo DefaultCulture { get; set; }

        /// <inheritdoc />
        public override string ProviderName => CultureFor.Operator;

        /// <inheritdoc />
        protected override string CurrentCulturePropertyName =>
            ApplicationConstants.LocalizationOperatorCurrentCulture;

        /// <inheritdoc />
        protected override void OnConfigure()
        {
            var locales = (string[])_properties.GetProperty(ApplicationConstants.LocalizationOperatorAvailable, new [] { CultureInfo.CurrentCulture.Name });
            var defaultLocale = (string)_properties.GetProperty(ApplicationConstants.LocalizationOperatorDefault, locales.First());

            if (locales.All(x => !string.Equals(x, defaultLocale, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new LocalizationException($"Configuration error Operator default locale not supported, {defaultLocale} not in ({string.Join(",", locales)})");
            }

            DefaultCulture = CultureInfo.GetCultureInfo(defaultLocale);

            AddCultures(locales.Select(CultureInfo.GetCultureInfo).ToArray());

            SetCurrentCulture(CultureInfo.GetCultureInfo(defaultLocale));
        }

        /// <inheritdoc />
        protected override void OnCultureChanged(CultureInfo oldCulture, CultureInfo newCulture)
        {
            base.OnCultureChanged(oldCulture, newCulture);

            _eventBus.Publish(new OperatorCultureChangedEvent(oldCulture, newCulture));
        }

        /// <inheritdoc />
        protected override void OnCultureAdded(IEnumerable<CultureInfo> cultures)
        {
            _eventBus.Publish(new OperatorCultureAdded(cultures));
        }

        /// <inheritdoc />
        protected override void OnCultureRemoved(IEnumerable<CultureInfo> cultures)
        {
            _eventBus.Publish(new OperatorCultureRemoved(cultures));
        }
    }
}
