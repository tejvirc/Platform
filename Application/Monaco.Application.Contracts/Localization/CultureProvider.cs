namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using Monaco.Localization;
    using Newtonsoft.Json;

    /// <summary>
    ///     Culture provider base class.
    /// </summary>
    public abstract class CultureProvider : ICultureProvider, IDisposable
    {
        private readonly ILocalization _localization;

        [JsonProperty(nameof(AvailableCultures))]
        private List<CultureInfo> _availableCultures = new List<CultureInfo>();

        private ILocalizationManager _localizer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CultureProvider"/> class.
        /// </summary>
        /// <param name="localization"></param>
        protected CultureProvider(ILocalization localization)
        {
            _localization = localization;
        }

        /// <inheritdoc />
        ~CultureProvider()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Gets or sets the localizer.
        /// </summary>
        private ILocalizationManager Localizer
        {
            get => _localizer ?? throw new LocalizationException($"Localizer not set for {ProviderName} provider.");

            set => _localizer = value;
        }

        /// <summary>
        ///     Current culture property name.
        /// </summary>
        protected virtual string CurrentCulturePropertyName => $"Localization.{ProviderName}.{nameof(CurrentCulture)}";

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc />
        [JsonProperty]
        public abstract string ProviderName { get; }

        /// <inheritdoc />
        [JsonProperty]
        public CultureInfo CurrentCulture { get; private set; }

        /// <inheritdoc />
        [JsonIgnore]
        public IReadOnlyCollection<CultureInfo> AvailableCultures => _availableCultures;

        /// <summary>
        ///     Perform initialization logic.
        /// </summary>
        protected virtual void OnInitialize()
        {
        }

        /// <summary>
        ///     Perform initialization logic.
        /// </summary>
        protected virtual void OnConfigure()
        {
        }

        /// <inheritdoc />
        public void Initialize(ILocalizationManager localizer, ILocalizationPropertyProvider propertyProvider)
        {
            Localizer = localizer;

            propertyProvider.AddProperty(CurrentCulturePropertyName, CurrentCulture.Name, HandleCurrentCultureChanged);

            OnInitialize();
        }

        /// <inheritdoc />
        public void Configure()
        {
            OnConfigure();
        }

        /// <inheritdoc />
        public CultureInfo[] AddCultures(params CultureInfo[] cultures)
        {
            var culturesAdded = new List<CultureInfo>();

            foreach (var culture in cultures)
            {
                if (!_localization.IsCultureSupported(culture))
                {
                    throw new LocalizationException($"Culture ({culture.Name}) is not supported");
                }

                if (!_availableCultures.Contains(culture))
                {
                    culturesAdded.Add(culture);
                }
            }

            if (culturesAdded.Any())
            {
                _availableCultures.AddRange(culturesAdded);

                OnCollectionChanged(NotifyCollectionChangedAction.Add, culturesAdded.ToArray());

                OnCultureAdded(culturesAdded);
            }

            if (CurrentCulture == null)
            {
                CurrentCulture = _availableCultures.First();
            }

            return culturesAdded.ToArray();
        }

        /// <inheritdoc />
        public CultureInfo[] RemoveCultures(params CultureInfo[] cultures)
        {
            var culturesRemoved = new List<CultureInfo>();

            foreach (var culture in cultures.ToArray())
            {
                if (_availableCultures.Contains(culture))
                {
                    culturesRemoved.Add(culture);
                    _availableCultures.Remove(culture);
                }
            }

            if (culturesRemoved.Any())
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, culturesRemoved.ToArray());

                OnCultureRemoved(culturesRemoved);
            }

            return culturesRemoved.ToArray();
        }

        /// <inheritdoc />
        public virtual bool IsCultureAvailable(CultureInfo culture)
        {
            return _availableCultures.Any(c => c.Equals(culture) || c.Equals(culture.Parent));
        }

        /// <inheritdoc />
        public virtual void SwitchTo()
        {
            _localization.CurrentCulture = CurrentCulture;
        }

        /// <inheritdoc />
        public CultureScope NewScope()
        {
            return new CultureScope(this);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(string key)
        {
            return GetObject<TResource>(CurrentCulture, key, null);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(string key, Action<Exception> exceptionHandler)
        {
            return GetObject<TResource>(CurrentCulture, key, exceptionHandler);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(CultureInfo culture, string key)
        {
            return GetObject<TResource>(culture, key, null);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(CultureInfo culture, string key, Action<Exception> exceptionHandler)
        {
            var resource = default(TResource);

            try
            {
                culture = culture ?? CurrentCulture;

                resource = Localizer.GetObject<TResource>(key, culture);
            }
            catch (Exception ex)
            {
                if (exceptionHandler == null)
                {
                    throw;
                }

                exceptionHandler.Invoke(ex);
            }

            if (resource == null)
            {
                var exception = new LocalizationException($"Resource not found for key: {key}");
                if (exceptionHandler == null)
                {
                    throw exception;
                }

                exceptionHandler.Invoke(exception);
            }

            return resource;
        }

        /// <inheritdoc />
        public string GetString(string key)
        {
            return GetString(CurrentCulture, key, null);
        }

        /// <inheritdoc />
        public string GetString(string key, Action<Exception> exceptionHandler)
        {
            return GetString(CurrentCulture, key, exceptionHandler);
        }

        /// <inheritdoc />
        public string GetString(CultureInfo culture, string key)
        {
            return GetString(culture, key, null);
        }

        /// <inheritdoc />
        public string GetString(CultureInfo culture, string key, Action<Exception> exceptionHandler)
        {
            return GetObject<string>(culture, key, exceptionHandler);
        }

        /// <inheritdoc />
        public string FormatString(string key, params object[] args)
        {
            return FormatString(CurrentCulture, key, args);
        }

        /// <inheritdoc />
        public string FormatString(CultureInfo culture, string key, params object[] args)
        {
            var format = GetObject<string>(culture, key);
            return string.Format(culture, format, args);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Release managed resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether the object is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <summary>
        ///     Sets the current culture.
        /// </summary>
        /// <param name="newCulture">The new culture.</param>
        protected virtual void SetCurrentCulture(CultureInfo newCulture)
        {
            ThrowIfCultureNotSupported(newCulture);

            var oldCulture = CurrentCulture;

            CurrentCulture = newCulture;

            _localizer.NotifyCultureChanged(ProviderName, newCulture);

            OnPropertyChanged(nameof(CurrentCulture));

            OnCultureChanged(oldCulture, newCulture);
        }

        /// <summary>
        ///     Handle culture changed.
        /// </summary>
        /// <param name="oldCulture">Previous culture.</param>
        /// <param name="newCulture">New culture.</param>
        protected virtual void OnCultureChanged(CultureInfo oldCulture, CultureInfo newCulture)
        {
        }

        /// <summary>
        ///     Handle culture(s) added.
        /// </summary>
        /// <param name="cultures">The cultures added.</param>
        protected virtual void OnCultureAdded(IEnumerable<CultureInfo> cultures)
        {
        }

        /// <summary>
        ///     Handle culture(s) removed.
        /// </summary>
        /// <param name="cultures">The cultures removed.</param>
        protected virtual void OnCultureRemoved(IEnumerable<CultureInfo> cultures)
        {
        }

        private void ThrowIfCultureNotSupported(CultureInfo culture)
        {
            if (!IsCultureAvailable(culture))
            {
                throw new LocalizationException($"Culture ({culture.Name}) is not supported for {ProviderName}");
            }
        }

        private void HandleCurrentCultureChanged(object value)
        {
            if (!(value is string locale))
            {
                return;
            }

            SetCurrentCulture(CultureInfo.GetCultureInfo(locale));
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, CultureInfo[] cultures)
        {
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(action, cultures));
        }
    }
}
