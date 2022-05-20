namespace Aristocrat.Monaco.UI.Common.Markup
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Windows.Markup;
    using System.Xaml;

    /// <summary>
    ///     This class allows access to localized string resources from XAML
    /// </summary>
    [MarkupExtensionReturnType(typeof(string))]
    public class LocalizationExtension : MarkupExtension
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationExtension" /> class.
        /// </summary>
        public LocalizationExtension()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationExtension" /> class.
        /// </summary>
        /// <param name="name">Name of resource</param>
        public LocalizationExtension(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets or sets the root name of the resources to lookup
        /// </summary>
        public string Root { get; set; }

        /// <summary>
        ///     Gets or sets the name of the resource to lookup
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the culture of the resource to lookup
        /// </summary>
        public string Culture { get; set; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return null;
            }

            var provider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;

            var view = provider?.RootObject;

            if (view == null)
            {
                return null;
            }

            var assembly = view.GetType().Assembly;

            var resourcesType = GetResourcesType(assembly);

            var baseName = resourcesType?.FullName;

            if (baseName == null)
            {
                return null;
            }

            var resourceManager = new ResourceManager(baseName, assembly);

            var culture = GetCulture(resourcesType);

            string value = null;

            try
            {
                value = resourceManager.GetString(Name, culture);
            }
            catch (MissingManifestResourceException)
            {
                // Ignore this exception and return null
            }

            return value;
        }

        private CultureInfo GetCulture(Type resourcesType)
        {
            if (string.IsNullOrWhiteSpace(Culture))
            {
                var property = resourcesType?.GetProperty("Culture", BindingFlags.Public | BindingFlags.Static);

                return property?.GetValue(null) as CultureInfo;
            }

            return CultureInfo.CreateSpecificCulture(Culture);
        }

        private Type GetResourcesType(Assembly assembly)
        {
            if (!string.IsNullOrWhiteSpace(Root))
            {
                return assembly.GetType(Root, false, true);
            }

            return assembly.GetTypes().FirstOrDefault(x => x.Name == "Resources");
        }
    }
}