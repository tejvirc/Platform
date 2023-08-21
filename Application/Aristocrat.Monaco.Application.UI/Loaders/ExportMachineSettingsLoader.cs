namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     Export machine settings view loader, <see cref="ExportMachineSettingsPage"/> and <see cref="ExportMachineSettingsViewModel"/>.
    /// </summary>
    public class ExportMachineSettingsLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportMachineSettings);

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new ExportMachineSettingsPage { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ExportMachineSettingsViewModel();
        }
    }
}
