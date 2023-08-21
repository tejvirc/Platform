namespace Aristocrat.Monaco.Application.UI.Input
{
    using MindFusion.UI.Wpf;

    /// <summary>
    ///     Interaction logic for EmbeddedKeyboardWindow.xaml
    /// </summary>
    public partial class EmbeddedKeyboardWindow
    {
        private const string LayoutFile = "MonacoDefaultQwertyLayout.xml";
        private const string LicenseKey = "AQAAAEIAAAAoAAAAAQAAHRcIvo2WjIuQnI2ei9+rmpyXkZCTkJiWmozT37aRnNGylpGbuYqMlpCR0bSahp2Qno2b0aivuf9/2IyRBCT3ZVFkgacL4X4PMWD3TUdT2ZM59wKnzAHHzp1llWfsybcHgCxIlgWCeQ==";

        public EmbeddedKeyboardWindow()
        {
            InitializeComponent();
            Keyboard.LicenseKey = LicenseKey;
            Keyboard.TemplateLayout = KeyboardLayout.Create(LayoutFile);
        }
    }
}
