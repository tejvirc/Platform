namespace Aristocrat.Monaco.Sas.UI.ConfigurationScreen
{
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Loaders;
    using System.Collections.ObjectModel;

    /// <summary>Definition of the SasConfigurationWizard class.</summary>
    public sealed class SasConfigurationWizard : IComponentWizard
    {
        /// <summary>Initializes a new instance of the SasConfigurationScreen class.</summary>
        public SasConfigurationWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>
            {
                new SasConfigurationPageLoader(true),
                new SasFeaturesPageLoader(true)
            };
        }

        /// <inheritdoc />
        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}
