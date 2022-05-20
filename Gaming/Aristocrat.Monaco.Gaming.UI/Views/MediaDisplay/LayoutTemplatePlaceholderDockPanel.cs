namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using Application.Contracts.Media;
    using System.Windows.Controls;
    using System.Windows.Media;
    using ViewModels;

    /// <summary>
    ///  See LayoutTemplateDockPanel.  This is a variant used for overlays to keep this view in sync
    ///  with the parent LayoutTemplateDockPanel.  MediaPlayerViews will be linked so that they open and close together and the 
    ///  screen size-position is maintained.  The logic to set and maintain sizes are controlled in the <seealso cref="LayoutTemplateViewModel"/>.
    /// </summary>
    public class LayoutTemplatePlaceholderDockPanel : LayoutTemplateDockPanelBase
    {
        /// <summary>
        ///  Initializes the <seealso cref="LayoutTemplatePlaceholderDockPanel"/>.
        /// </summary>
        public LayoutTemplatePlaceholderDockPanel()
        {
            ViewModel = new LayoutTemplateViewModel(ScreenType, DisplayType.Scale, true);
            Background = Brushes.Transparent;
        }

        protected override ContentControl CreateView(IMediaPlayerViewModel player)
        {
            return new MediaPlayerPlaceholderView();
        }
    }
}
