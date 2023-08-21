namespace Aristocrat.Monaco.Gaming.UI.Config
{
    using System.Collections.ObjectModel;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Loaders;

    /// <summary>
    /// Definition of the GamingConfigWizard class.
    /// This class defines which gaming layer pages will be shown by
    /// the Configuration Wizard
    /// </summary>
    // ReSharper disable once UnusedType.Global - used by addins
    public sealed class GamingConfigWizard : IComponentWizard
    {
        /// <summary>Initializes a new instance of the GamingConfigWizard class.</summary>
        public GamingConfigWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>
            {
                new LimitsPageLoader(true)
            };
        }

        /// <inheritdoc />
        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}
