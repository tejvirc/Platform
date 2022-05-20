namespace Aristocrat.Monaco.Localization
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    using Markup;
    using WPFLocalizeExtension.Engine;
    using WPFLocalizeExtension.Providers;

    /// <summary>
    ///     Bridges localization services for the Platform.
    /// </summary>
    public class LocalizationManager : ILocalizationManager, ILocalizationManagerCallback
    {
        private const string DefaultResourceDictionary = "Resources";

        private readonly ILocalizationClient _client;
        private readonly CustomResxLocalizationProvider _provider;

        private readonly List<(string Assembly, string Dictionary)> _resources =
            new List<(string Assembly, string Dictionary)>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationManager"/> class.
        /// </summary>
        /// <param name="client">Localization services service.</param>
        public LocalizationManager(ILocalizationClient client)
        {
            _client = client;

            _provider = new CustomResxLocalizationProvider(this);

            _provider.ProviderError += OnProviderError;
        }

        /// <inheritdoc />
        public CultureInfo CurrentCulture
        {
            get => LocalizeDictionary.Instance.Culture;

            set => LocalizeDictionary.Instance.Culture = value;
        }

        /// <inheritdoc />
        public IReadOnlyList<CultureInfo> AvailableCultures =>
            _provider.AvailableCultures.ToList().AsReadOnly();

        /// <inheritdoc />
        public void Start()
        {
            Start(Assembly.GetExecutingAssembly(), DefaultResourceDictionary);
        }

        /// <inheritdoc />
        public void Start(Assembly resourceAssembly, string resourceDictionary)
        {
            var assemblyName = new AssemblyName(resourceAssembly.FullName);

            _provider.FallbackAssembly = assemblyName.Name;
            _provider.FallbackDictionary = resourceDictionary;

            LocalizeDictionary.Instance.DefaultProvider = _provider;
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;

            LocalizationEventManager.Start();

            _provider.UpdateCultureList(resourceAssembly, resourceDictionary, assemblyName.Name);
        }

        /// <inheritdoc />
        public void LoadResources(Assembly resourceAssembly, string resourceDictionary)
        {
            var resourceAssemblyPath = resourceAssembly.Location;

            var directory = Path.GetDirectoryName(resourceAssemblyPath);
            if (directory == null)
            {
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(resourceAssemblyPath);
            var name = $@"{directory.Substring(Path.GetPathRoot(directory).Length)}\{fileName}";

            _provider.UpdateCultureList(resourceAssembly, resourceDictionary, name);

            _resources.Add((name, resourceDictionary));
        }

        /// <inheritdoc />
        public void AddCultures(params CultureInfo[] cultures)
        {
            _provider.AddCultures(cultures);
        }

        /// <summary>
        ///     Gets the localized resource for a specified key.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <typeparam name="TResource">The type of the resource to retrieve.</typeparam>
        /// <param name="name">The name</param>
        /// <param name="allowOverrides">true if overrides are allowed</param>
        /// <returns>The resource form the specified key.</returns>
        public TResource GetObject<TResource>(string name, string key, bool allowOverrides = true)
        {
            var resourceKey = _provider.GetFullyQualifiedResourceKey(key, null);

            var target = new DependencyObject();

            Localizer.SetFor(target, name);
            Localizer.SetAllowOverrides(target, allowOverrides);

            return (TResource)_provider.GetLocalizedObject(resourceKey, target, CurrentCulture);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(string key)
        {
            return GetObject<TResource>(key, CurrentCulture);
        }

        /// <inheritdoc />
        public TResource GetObject<TResource>(string key, CultureInfo culture)
        {
            var resourceKey = _provider.GetFullyQualifiedResourceKey(key, null);

            return (TResource)_provider.GetLocalizedObject(resourceKey, null, culture ?? CurrentCulture);
        }

        /// <inheritdoc />
        public void NotifyCultureChanged(string name, CultureInfo newCulture)
        {
            _provider.OnProviderChanged();
        }

        /// <inheritdoc />
        public string[] GetOverrideKeys(string key)
        {
            return _resources
                .ToArray()
                .Reverse()
                 .Select(x => Regex.Replace(key, @"\S+?:\S+?:(.+)", $"{x.Assembly}:{x.Dictionary}:$1"))
                // .Select(x => Regex.Replace(key, @"[a-zA-Z]:\\[\\\S|*\S]?.*:\S+?:(.+)", $"{x.Assembly}:{x.Dictionary}:$1"))
                .ToArray();
        }

        /// <inheritdoc />
        public CultureInfo GetCultureFor(string name)
        {
            return _client.GetCultureFor(name);
        }

        private void OnProviderError(object sender, ProviderErrorEventArgs args)
        {
            _client.LocalizationError(new LocalizationErrorArgs(args.Key, args.Message));
        }
    }
}
