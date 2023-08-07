namespace Aristocrat.Monaco.G2S.UI
{
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Loaders;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Defines an instance of an IComponentWizard used to configure G2S related settings.
    /// </summary>
    public class G2SConfigurationWizard : IComponentWizard
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SConfigurationWizard" /> class.
        ///     Constructs a new instance initializing the WizardPages collection.
        /// </summary>
        public G2SConfigurationWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>();
            if (!NetworkConfigPageLoader.IsInstantiated)
            {
                WizardPages.Add(new NetworkConfigPageLoader(true));
            }
            WizardPages.Add(new HostConfigurationViewLoader(true));
            WizardPages.Add(new SecurityConfigurationViewLoader(true));
        }
        
        /// <inheritdoc />
        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}
