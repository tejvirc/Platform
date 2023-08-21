namespace Aristocrat.Monaco.UI.Common
{
    using System.IO;
    using System.Windows;
    using System.Windows.Markup;
    using Kernel;

    /// <summary>
    ///     Definition of the SkinLoader class.
    /// </summary>
    public static class SkinLoader
    {
        private const string FilePath = "/Assets/Skins";
        private const string Common = "CommonUI.xaml";

        private static readonly ResourceDictionary CommonUiDictionary = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceDictionary" /> class.
        /// </summary>
        /// <param name="filename">The name of the resource dictionary</param>
        /// <param name="includeCommon">Allow NOT to load CommonUI.xaml, for non-lobby/game users.</param>
        /// <returns>A resource dictionary loaded from the file</returns>
        public static ResourceDictionary Load(string filename, bool includeCommon = true)
        {
            var dictionary = LoadDictionary(filename);
            if (includeCommon)
            {
                dictionary.MergedDictionaries.Add(LoadCommon());
            }
            return dictionary;
        }

        private static ResourceDictionary LoadDictionary(string filename)
        {
            var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            var dir = pathMapper.GetDirectory(FilePath);
            var path = Path.Combine(dir.FullName, filename);

            if (File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    // Get the root element, which must be a ResourceDictionary
                    var rd = (ResourceDictionary)XamlReader.Load(fs);

                    return rd;
                }
            }

            return new ResourceDictionary();
        }

        private static ResourceDictionary LoadCommon()
        {
            return CommonUiDictionary ?? LoadDictionary(Common);
        }
    }
}