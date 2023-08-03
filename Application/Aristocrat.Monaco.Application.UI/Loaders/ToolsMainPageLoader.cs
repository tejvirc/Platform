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
        private readonly string _pageNameResourceKey = ResourceKeys.ToolsMainPageLabel;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(_pageNameResourceKey);

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new ToolsMainPage { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ToolsMainPageViewModel(_pageNameResourceKey);
        }
    }
}
