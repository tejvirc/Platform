//[assembly: System.Windows.Markup.XmlnsDefinition(
//    "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
//    "WPFTutorial.Utils")]

namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// The shared resource dictionary is a specialized resource dictionary
    /// that loads it content only once. If a second instance with the same source
    /// is created, it only merges the resources from the cache.
    /// </summary>
    [CLSCompliant(false)]
    public class SharedResourceDictionary : ResourceDictionary
    {
        /// <summary>
        /// Internal cache of loaded dictionaries 
        /// </summary>
        public static Dictionary<Uri, ResourceDictionary> SharedDictionaries = new Dictionary<Uri, ResourceDictionary>();

        /// <summary>
        /// Local member of the source uri
        /// </summary>
        private Uri _sourceUri;

        /// <summary>
        /// Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get => _sourceUri;
            set
            {
                _sourceUri = value;

                if (!SharedDictionaries.ContainsKey(value))
                {
                    // If the dictionary is not yet loaded, load it by setting
                    // the source of the base class
                    base.Source = value;

                    // add it to the cache
                    SharedDictionaries.Add(value, this);
                }
                else
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add(SharedDictionaries[value]);
                }
            }
        }
    }
}
