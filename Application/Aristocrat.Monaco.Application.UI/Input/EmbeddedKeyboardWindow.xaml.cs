namespace Aristocrat.Monaco.Application.UI.Input
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using log4net;
    using MindFusion.UI.Wpf;

    /// <summary>
    ///     Interaction logic for EmbeddedKeyboardWindow.xaml
    /// </summary>
    public partial class EmbeddedKeyboardWindow
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const string LayoutFileDefault = "MonacoDefaultQwertyLayout.xml";
        private const string LayoutFile = "MonacoQwertyLayout";
        private const string LicenseKey = "AQAAAEIAAAAoAAAAAQAAHRcIvo2WjIuQnI2ei9+rmpyXkZCTkJiWmozT37aRnNGylpGbuYqMlpCR0bSahp2Qno2b0aivuf9/2IyRBCT3ZVFkgacL4X4PMWD3TUdT2ZM59wKnzAHHzp1llWfsybcHgCxIlgWCeQ==";

        public EmbeddedKeyboardWindow()
        {
            InitializeComponent();
            Keyboard.LicenseKey = LicenseKey;
            Keyboard.TemplateLayout = KeyboardLayout.Create(LayoutFileDefault);
        }

        public void SetCulture(CultureInfo culture)
        {
            if (culture == null)
            {
                return;
            }

            // Each language code needs its own layout file without function keys and specific special keys
            // Download layout creator from https://mindfusion.eu/virtual-keyboard-wpf.html
            var xml = $"{LayoutFile}_{culture.IetfLanguageTag}.xml";
            var file = File.Exists(xml) ? xml : LayoutFileDefault;
            Logger.Debug($"Using layout file {file} for language {culture.Name}");
            Keyboard.TemplateLayout = KeyboardLayout.Create(file);
        }
    }
}
