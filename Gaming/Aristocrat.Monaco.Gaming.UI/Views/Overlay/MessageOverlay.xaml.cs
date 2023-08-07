namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System.Reflection;
    using System.Windows.Forms;
    using log4net;

    /// <summary>
    ///     Interaction logic for MessageOverlay.xaml
    /// </summary>
    public partial class MessageOverlay
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageOverlay" /> class.
        /// </summary>
        public MessageOverlay()
        {
            InitializeComponent();
            //// Landscape
            if (Screen.PrimaryScreen.Bounds.Width > Screen.PrimaryScreen.Bounds.Height)
            {
                OuterGrid.Width = 1920;
                OuterGrid.Height = 1080;
            }
            else
            {
                OuterGrid.Width = 1080;
                OuterGrid.Height = 1080;
            }
            Logger.Debug($"PrimaryScreen.Width = {Screen.PrimaryScreen.Bounds.Width}, PrimaryScreen.Height = {Screen.PrimaryScreen.Bounds.Height}");
            Logger.Debug($"OuterGrid.Width = {OuterGrid.Width}, OuterGrid.Height = {OuterGrid.Height}");
        }
    }
}