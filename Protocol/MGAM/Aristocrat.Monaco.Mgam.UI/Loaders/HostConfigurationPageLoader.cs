namespace Aristocrat.Monaco.Mgam.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     Used to load the <see cref="HostConfigurationView"/> and <see cref="HostConfigurationViewModel"/>.
    /// </summary>
    public class HostConfigurationPageLoader : OperatorMenuPageLoader
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationPageLoader"/> class.
        /// </summary>
        public HostConfigurationPageLoader()
            : this(false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationPageLoader"/> class.
        /// </summary>
        /// <param name="isWizardPage">A value that indicates whether the loader is being created for the configuration wizard.</param>
        public HostConfigurationPageLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Mgam);

        /// <inheritdoc />
        public override CommsProtocol RequiredProtocol => CommsProtocol.MGAM;

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new HostConfigurationView { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HostConfigurationViewModel(IsWizardPage);
        }
    }
}
