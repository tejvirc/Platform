namespace Aristocrat.Monaco.Sas.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using DiagnosticSasPage = ConfigurationScreen.DiagnosticSasPage;
    /// <summary>
    /// 
    /// </summary>
    public class DiagnosticSasPageLoader : OperatorMenuPageLoader
    {
        /// <summary>
        /// 
        /// </summary>
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DiagnosticSasPageTitle);

        /// <inheritdoc />
        public override CommsProtocol RequiredProtocol => CommsProtocol.SAS;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IOperatorMenuPage CreatePage()
        {
            return new DiagnosticSasPage { DataContext = ViewModel };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new DiagnosticSasPageViewModel();
        }
    }
}
