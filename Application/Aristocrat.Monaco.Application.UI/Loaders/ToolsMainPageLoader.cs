namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     Tools Main Page Loader, <see cref="ToolsMainPage"/> and <see cref="ToolsMainPageViewModel"/>.
    /// </summary>
    public class ToolsMainPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ToolsMainPageLabel);

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new ToolsMainPage { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ToolsMainPageViewModel(this);
        }
    }
}
