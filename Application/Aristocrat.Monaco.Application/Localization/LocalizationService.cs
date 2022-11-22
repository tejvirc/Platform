namespace Aristocrat.Monaco.Application.Localization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Monaco.Localization;
    using Mono.Addins;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///     Localization service that implements <see cref="ILocalization"/> interface.
    /// </summary>
    public sealed class LocalizationService : ILocalization, ILocalizationClient, IService, IDisposable
    {
        // ReSharper disable once PossibleNullReferenceException
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string CultureProvidersExtensionPath = "/Application/Localization/Providers";
        private const string OverridesResourcesDictionary = "Resources";

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly ILocalizationManager _localizer;

        private readonly Dictionary<string, ICultureProvider> _providers = new();
        private readonly object _providersLock = new();

        private LocalizationPropertiesProvider _propertiesProvider;

        private bool _configured;

        private readonly string[] _culturePropertyNames = 
        {
            nameof(ICultureProvider.CurrentCulture),
            nameof(IPlayerCultureProvider.DefaultCulture),
            nameof(IPlayerCultureProvider.LanguageOptions)
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        public LocalizationService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationService"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/> instance.</param>
        /// <param name="properties"><see cref="IPropertiesManager"/> instance.</param>
        public LocalizationService(IEventBus eventBus, IPropertiesManager properties)
        {
            _eventBus = eventBus;
            _properties = properties;

            _localizer = new LocalizationManager(this);
        }

        /// <inheritdoc />
        ~LocalizationService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[]
        {
            typeof(ILocalization),
            typeof(ILocalizerFactory)
        };

        /// <inheritdoc />
        public RegionInfo RegionInfo => new(CurrentCulture.LCID);

        /// <inheritdoc />
        public CultureInfo DefaultCulture { get; private set; }

        /// <inheritdoc />
        public CultureInfo CurrentCulture
        {
            get => _localizer.CurrentCulture;

            set
            {
                if (Equals(_localizer.CurrentCulture, value))
                {
                    return;
                }

                ThrowIfCultureNotSupported(value);

                var oldCulture = _localizer.CurrentCulture;

                _localizer.CurrentCulture = value;

                OnCurrentCultureChanged(oldCulture, _localizer.CurrentCulture);
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<CultureInfo> SupportedCultures => _localizer.AvailableCultures;

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug("Initializing localization service...");

            try
            {
                SubscribeToEvents();

                DefaultCulture = CultureInfo.GetCultureInfo(ApplicationConstants.DefaultLanguage);

                AddCultures(DefaultCulture);

                CurrentCulture = DefaultCulture;

                StartLocalizer();

                WaitForStorageAsync();

                Logger.Debug("Completed initializing localization service.");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize localization service.", ex);
                throw;
            }
        }

        private void WaitForStorageAsync()
        {
            Task.Run(
                    () =>
                    {
                        using (var serviceWaiter = new ServiceWaiter(_eventBus))
                        {
                            serviceWaiter.AddServiceToWaitFor<IPersistentStorageManager>();

                            serviceWaiter.WaitForServices();
                        }

                        _propertiesProvider = new LocalizationPropertiesProvider();

                        Restore();
                    })
                .ContinueWith(
                    task =>
                    {
                        if (!task.IsFaulted)
                        {
                            return;
                        }

                        Logger.Error("Error restoring localization state.", task.Exception);
                        task.Exception?.Handle(_ => true);
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <inheritdoc />
        public ICultureProvider GetProvider(string name)
        {
            if (!TryGetProvider(name, out var provider))
            {
                throw new ArgumentOutOfRangeException(nameof(name));

            }

            return provider;
        }

        /// <inheritdoc />
        public bool TryGetProvider(string name, out ICultureProvider provider)
        {
            lock (_providersLock)
            {
                return _providers.TryGetValue(name, out provider);
            }
        }

        /// <inheritdoc />
        public bool IsCultureSupported(CultureInfo culture)
        {
            return culture != null && _localizer.AvailableCultures.Any(c => c.Equals(culture) || c.Equals(culture.Parent));
        }

        /// <inheritdoc />
        public CultureInfo GetCultureFor(string name)
        {
            return GetProvider(name).CurrentCulture;
        }

        /// <inheritdoc />
        public void LocalizationError(LocalizationErrorArgs args)
        {
            Logger.Error($"Error retrieving localized resource for {args.Key} -- {args.Message}");
        }

        /// <inheritdoc />
        public ILocalizer For(string name)
        {
            return GetProvider(name);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<LocalizationConfigurationEvent>(this, Handle);
        }

        private void Handle(LocalizationConfigurationEvent evt)
        {
            Configure(evt.OverridePaths);
        }

        private void Restore()
        {
            try
            {
                InitFromStorage();

                RegisterProviders();
            }
            finally
            {
                _properties.AddPropertyProvider(_propertiesProvider);
            }
        }

        private void Configure(IEnumerable<string> overridePaths)
        {
            if (_configured)
            {
                return;
            }

            try
            {
                Logger.Debug("Configuring localization service.");

                ConfigureProviders();

                LoadOverrides(overridePaths);

                CommitToStorage();

                Logger.Debug("Completed configuring localization service.");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to configure localization service.", ex);
                throw;
            }
            finally
            {
                _configured = true;
            }
        }

        private void ConfigureProviders()
        {
            foreach (var provider in _providers.Values.ToArray())
            {
                provider.Configure();
            }
        }

        private void StartLocalizer()
        {
            Logger.Debug("Starting localizer...");
            _localizer.Start();
            Logger.Debug("Completed starting localizer.");
        }

        private void RegisterProviders()
        {
            var providers = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(CultureProvidersExtensionPath)
                .Select(x => (ICultureProvider)x.CreateInstance());

            Register(providers.ToArray());
        }

        private void Register(params ICultureProvider[] providers)
        {
            InitFromStorage(providers);

            foreach (var provider in providers)
            {
                if (_providers.ContainsKey(provider.ProviderName))
                {
                    throw new LocalizationException($"Provider already registered, {provider.ProviderName}");
                }

                provider.AddCultures(DefaultCulture);

                provider.Initialize(_localizer, _propertiesProvider);

                lock (_providersLock)
                {
                    _providers.Add(provider.ProviderName, provider);
                }

                provider.PropertyChanged += OnProviderPropertyChanged;

                if (_configured)
                {
                    provider.Configure();
                }
            }
        }

        private void LoadOverrides(IEnumerable<string> overridePaths)
        {
            Logger.Debug("Loading override resources...");

            foreach (var path in overridePaths)
            {
                try
                {
                    Logger.Debug($"Scanning assemblies in {path}...");

                    var assemblyPaths = new List<string>();

                    if (Directory.Exists(path))
                    {
                        assemblyPaths.AddRange(Directory.EnumerateFiles(path, "*.dll"));
                    }
                    else if (File.Exists(path))
                    {
                        assemblyPaths.Add(path);
                    }
                    else
                    {
                        throw new FileNotFoundException($"File or directory not found: {path}");
                    }

                    foreach (var assemblyPath in assemblyPaths)
                    {
                        LoadResources(assemblyPath);
                    }

                    Logger.Debug($"Completed scanning assemblies in {path}.");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed loading override resources at {path}", ex);
                }
            }

            Logger.Debug("Completed loading override resources.");
        }

        private void LoadResources(string assemblyPath)
        {
            Logger.Debug($"Loading resources from {assemblyPath}...");

            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                _localizer.LoadResources(assembly, OverridesResourcesDictionary);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed loading resources from {assemblyPath}...", ex);
            }

            Logger.Debug($"Completed loading resources from {assemblyPath}...");
        }

        private void AddCultures(params CultureInfo[] cultures)
        {
            _localizer.AddCultures(cultures);
        }

        private void OnCurrentCultureChanged(CultureInfo oldCulture, CultureInfo newCulture)
        {
            CommitToStorage();

            _properties.SetProperty(ApplicationConstants.LocalizationCurrentCulture, newCulture.Name);

            _eventBus.Publish(new CurrentCultureChangedEvent(oldCulture, newCulture));
        }

        private void OnProviderPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (_culturePropertyNames.Any(n => n == args.PropertyName))
            {
                // if any of these property is changed, save them
                CommitToStorage();
            }
        }

        private void CommitToStorage()
        {
            if (_propertiesProvider == null)
            {
                return;
            }

            var state =
                new LocalizationState
                {
                    CurrentCulture = CurrentCulture,
                    DefaultCulture = DefaultCulture,
                    Providers = _providers.Values.ToDictionary(
                        x => x.ProviderName,
                        JObject.FromObject)
                };

            var json = JsonConvert.SerializeObject(state, Formatting.None);

            _propertiesProvider.SetProperty(ApplicationConstants.LocalizationState, json);
        }

        private void InitFromStorage()
        {
            try
            {
                var json = (string)_propertiesProvider?.GetProperty(ApplicationConstants.LocalizationState);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                var state = JsonConvert.DeserializeObject<LocalizationState>(json);

                if (state == null)
                {
                    return;
                }

                DefaultCulture = state.DefaultCulture;
                CurrentCulture = state.CurrentCulture;

                _propertiesProvider.AddProperty(ApplicationConstants.LocalizationCurrentCulture, CurrentCulture.Name);
            }
            catch (Exception ex)
            {
                Logger.Error("Error retrieving localization state", ex);
            }
        }

        private void InitFromStorage(IEnumerable<ICultureProvider> providers)
        {
            try
            {
                var json = (string)_propertiesProvider?.GetProperty(ApplicationConstants.LocalizationState);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                var state = JsonConvert.DeserializeObject<LocalizationState>(json);

                if (state == null)
                {
                    return;
                }

                foreach (var provider in providers)
                {
                    if (!state.Providers.TryGetValue(provider.ProviderName, out JObject obj))
                    {
                        continue;
                    }

                    JsonConvert.PopulateObject(obj.ToString(), provider);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error retrieving localization state", ex);
            }
        }

        private void ThrowIfCultureNotSupported(CultureInfo culture)
        {
            if (!IsCultureSupported(culture))
            {
                throw new LocalizationException($"Culture ({culture.Name}) is not supported");
            }
        }
    }
}
