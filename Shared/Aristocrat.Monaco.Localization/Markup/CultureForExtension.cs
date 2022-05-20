namespace Aristocrat.Monaco.Localization.Markup
{
    using System;
    using System.Windows;
    using System.Windows.Markup;
    using System.Xaml;
    using WPFLocalizeExtension.Engine;

    /// <summary>
    ///     <see cref="MarkupExtension"/> to get the current culture for a provider.
    /// </summary>
    public class CultureForExtension : MarkupExtension
    {
        /// <summary>
        ///     Initializes an instance of the <see cref="CultureForExtension"/> class.
        /// </summary>
        public CultureForExtension()
        {
        }

        /// <summary>
        ///     Initializes an instance of the <see cref="CultureForExtension"/> class.
        /// </summary>
        /// <param name="name"></param>
        public CultureForExtension(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the provider name.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var targetObject = default(DependencyObject);

            if (string.IsNullOrEmpty(Name))
            {
                if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget)
                {
                    targetObject = provideValueTarget.TargetObject as DependencyObject;
                }

                if (targetObject == null && serviceProvider.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootObjectProvider)
                {
                    targetObject = rootObjectProvider.RootObject as DependencyObject;
                }
            }

            if (!(LocalizeDictionary.Instance.DefaultProvider is CustomResxLocalizationProvider provider))
            {
                throw new InvalidOperationException($"Expected the default provider to be {nameof(CustomResxLocalizationProvider)}");
            }

            return !string.IsNullOrEmpty(Name) ? provider.GetCultureFor(Name).Name : provider.GetCultureFor(targetObject).Name;
        }
    }
}
