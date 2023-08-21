namespace Aristocrat.Monaco.Mgam.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     
    /// </summary>
    public class HostTranscriptsPageLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Mgam);

        /// <inheritdoc />
        public override CommsProtocol RequiredProtocol => CommsProtocol.MGAM;

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new HostTranscriptsView { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HostTranscriptsViewModel();
        }
    }
}
