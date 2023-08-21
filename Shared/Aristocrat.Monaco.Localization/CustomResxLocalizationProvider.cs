namespace Aristocrat.Monaco.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Loader;
    using System.Windows;
    using Markup;
    using WPFLocalizeExtension.Engine;
    using WPFLocalizeExtension.Providers;

    /// <summary>
    ///     Custom RESX localization provider that implements <see cref="ILocalizationProvider"/> interface.
    /// </summary>
    internal class CustomResxLocalizationProvider : ResxLocalizationProvider
    {
        private static string _resourceDebugKey;

        private const string ResourceFileExtension = ".resources";

        private readonly ILocalizationManagerCallback _manager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CustomResxLocalizationProvider"/> class.
        /// </summary>
        /// <param name="manager"><see cref="LocalizationManager"/> instance.</param>
        public CustomResxLocalizationProvider(ILocalizationManagerCallback manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            IgnoreCase = true;
        }

        /// <summary>
        ///     Adds supported cultures.
        /// </summary>
        /// <param name="cultures">Culture to add.</param>
        public void AddCultures(params CultureInfo[] cultures)
        {
            foreach (var culture in cultures)
            {
                AddCulture(culture);
            }
        }

        public void OnProviderChanged()
        {
            Application.Current.Dispatcher?.InvokeAsync(() =>
            {
                try
                {
                    base.OnProviderChanged(null);
                }
                catch (Exception ex)
                {
                    var exceptionToPropagate = ex;
                    if (ex is AggregateException aggregateException)
                    {
                        exceptionToPropagate = aggregateException.InnerException;
                    }

                    OnProviderError(null, _resourceDebugKey, exceptionToPropagate.Message);
                }
            });
        }

        public CultureInfo GetCultureFor(DependencyObject target)
        {
            var name = Localizer.GetFor(target);

            return GetCultureFor(name);
        }

        public CultureInfo GetCultureFor(string name)
        {
            var culture = LocalizeDictionary.CurrentCulture;

            if (!string.IsNullOrWhiteSpace(name))
            {
                culture = _manager.GetCultureFor(name);
            }

            return culture;
        }

        /// <inheritdoc />
        public override object GetLocalizedObject(string key, DependencyObject target, CultureInfo culture)
        {
            try
            {
                var name = Localizer.GetFor(target);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    culture = _manager.GetCultureFor(name);
                }

                object resource;

                var allowOverrides = Localizer.GetAllowOverrides(target);

                if (allowOverrides)
                {
                    var overrideKeys = _manager.GetOverrideKeys(key);

                    foreach (var overrideKey in overrideKeys)
                    {
                        resource = GetResource(overrideKey, target, culture);

                        if (resource != null)
                        {
                            return resource;
                        }
                    } 
                }

                resource = GetResource(key, target, culture);

                if (resource == null)
                {
                    OnProviderError(target, key, $"Resource not found for key: {key}");
                }

                _resourceDebugKey = key;

                return resource;
            }
            catch (Exception ex)
            {
                OnProviderError(target, key, "Error retrieving the resource for key: {key}." + ex.Message);
                return null;
            }
        }

        internal bool UpdateCultureList(Assembly resourceAssembly, string resourceDictionary, string name)
        {
            return GetResourceManager(resourceAssembly, resourceDictionary, name) != null;
        }

        protected new ResourceManager GetResourceManager(string resourceAssembly, string resourceDictionary)
        {
            var resManagerNameToSearch = $".{resourceDictionary}{ResourceFileExtension}";
            var resManKey = resourceAssembly + resManagerNameToSearch;

            if (TryGetValue(resManKey, out var resManager))
            {
                return resManager;
            }

            var assembly = GetResourceAssembly(resourceAssembly);

            return GetResourceManager(assembly, resourceDictionary, resManKey, resManagerNameToSearch);
        }

        private ResourceManager GetResourceManager(Assembly assembly, string resourceDictionary, string name)
        {
            var resManagerNameToSearch = $".{resourceDictionary}{ResourceFileExtension}";
            var resManKey = name + resManagerNameToSearch;

            return GetResourceManager(assembly, resourceDictionary, resManKey, resManagerNameToSearch);
        }

        private ResourceManager GetResourceManager(Assembly assembly, string resourceDictionary, string resManKey, string resManagerNameToSearch)
        {
            var resManagerType = GetResourceManagerType(assembly, resourceDictionary, resManagerNameToSearch);

            var resManager = resManagerType != null
                ? GetResourceManagerFromType(resManagerType)
                : new ResourceManager(resManagerNameToSearch, assembly);

            if (resManager == null)
            {
                throw new InvalidOperationException(
                    $"No resource manager for dictionary '{resourceDictionary}' in assembly '{assembly}' found");
            }

            Add(resManKey, resManager);

            LoadCultureResources(assembly, resManager);

            return resManager;
        }

        private static Type GetResourceManagerType(Assembly assembly, string resourceDictionary, string resManagerNameToSearch)
        {
            Type resManagerType = null;

            var availableResources = assembly.GetManifestResourceNames();

            IEnumerable<Type> availableTypes;

            try
            {
                availableTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                availableTypes = ex.Types.Where(t => t != null);
            }

            // The proposed approach: http://wpflocalizeextension.codeplex.com/discussions/66098?ProjectName=wpflocalizeextension
            string TryGetNamespace(Type type)
            {
                try
                {
                    return type.Namespace;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            var prefixes = availableTypes.Select(TryGetNamespace)
                .Where(n => n != null)
                .Distinct()
                .ToList();

            var foundResource = availableResources.FirstOrDefault(
                availableResource => availableResource.EndsWith(resManagerNameToSearch) &&
                                     prefixes.Any(p => availableResource.StartsWith(p + ".")));

            if (foundResource != null)
            {
                foundResource = foundResource.Substring(0, foundResource.Length - ResourceFileExtension.Length);

                try
                {
                    resManagerType = assembly.GetType(foundResource);
                }
                catch (Exception)
                {
                    resManagerType = null;
                }

                if (resManagerType == null)
                {
                    var dictTypeName = resourceDictionary.Replace('.', '_');

                    bool MatchesDictTypeName(Type type)
                    {
                        try
                        {
                            return type.Name == dictTypeName;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }

                    resManagerType = availableTypes.FirstOrDefault(MatchesDictTypeName);
                }
            }

            return resManagerType;
        }

        private static Assembly GetResourceAssembly(string resourceAssembly)
        {
            Assembly assembly = null;

            try
            {
                var loadedAssemblies = AssemblyLoadContext.Default.Assemblies;
                foreach (var assemblyInAppDomain in loadedAssemblies)
                {
                    var assemblyName = new AssemblyName(assemblyInAppDomain.FullName);

                    if (assemblyName.Name == resourceAssembly)
                    {
                        assembly = assemblyInAppDomain;
                        break;
                    }
                }

                if (assembly == null)
                {
                    assembly = Assembly.Load(new AssemblyName(resourceAssembly));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The Assembly '{resourceAssembly}' cannot be loaded.", ex);
            }

            return assembly;
        }

        private void LoadCultureResources(Assembly assembly, ResourceManager resManager)
        {
            var cultures = GetCulturesInUse(assembly).ToArray();

            foreach (var c in cultures)
            {
                var rs = resManager.GetResourceSet(c, true, false);
                if (rs != null)
                {
                    AddCulture(c);
                }
            }
        }

        private static ResourceManager GetResourceManagerFromType(IReflect type)
        {
            ResourceManager resManager;

            if (type == null)
            {
                return null;
            }

            try
            {
                resManager = (ResourceManager)type
                    .GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .GetGetMethod(true).Invoke(null, null);
            }
            catch
            {
                resManager = null;
            }

            return resManager;
        }

        private static IEnumerable<CultureInfo> GetCulturesInUse(Assembly assembly)
        {
            var location = assembly.Location;
            var fileName = Path.GetFileNameWithoutExtension(location) + ".resources.dll";

            var directoryName = Path.GetDirectoryName(location);

            if (directoryName == null)
            {
                throw new InvalidOperationException($"Unable to retrieve cultures -- path is invalid: {location}");
            }

            var directory = new DirectoryInfo(directoryName);

            return from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
                   join d in directory.EnumerateDirectories() on c.IetfLanguageTag equals d.Name
                   where d.EnumerateFiles(fileName).Any()
                   select (CultureInfo)c.Clone();
        }

        private object GetResource(string key, DependencyObject target, CultureInfo culture)
        {
            var fqKey = (FQAssemblyDictionaryKey)GetFullyQualifiedResourceKey(key, target);

            if (string.IsNullOrWhiteSpace(fqKey?.Key))
            {
                OnProviderError(target, key, "No key provided.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(fqKey.Assembly))
            {
                OnProviderError(target, key, "No assembly provided.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(fqKey.Dictionary))
            {
                OnProviderError(target, key, "No dictionary provided.");
                return null;
            }

            var resManager = GetResourceManager(fqKey.Assembly, fqKey.Dictionary);

            resManager.IgnoreCase = IgnoreCase;

            return resManager.GetObject(fqKey.Key, culture);
        }
    }
}
