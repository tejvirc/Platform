namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using Monaco.UI.Common;

    /// <summary>
    ///     Interaction logic for ReserveOverlay.xaml
    /// </summary>
    public partial class ReserveOverlay
    {
        public ReserveOverlay()
        {
            InitializeComponent();

            Resources.MergedDictionaries.Add(SkinLoader.Load("PlayerMenuPopupUI.xaml"));
        }
    }
}