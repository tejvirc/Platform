namespace Aristocrat.Monaco.Mgam.UI
{
    using System.Collections.ObjectModel;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Loaders;

    /// <summary>
    ///     Defines the pages to load for configuration.
    /// </summary>
    public class MgamConfigurationWizard : IComponentWizard
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MgamConfigurationWizard" /> class.
        /// </summary>
        /// <remarks>
        ///     Constructs a new instance initializing the WizardPages collection.
        /// </remarks>
        public MgamConfigurationWizard()
        {
            WizardPages = new Collection<IOperatorMenuPageLoader>();
            if (!NetworkConfigPageLoader.IsInstantiated)
            {
                WizardPages.Add(new NetworkConfigPageLoader(true));
            }
            WizardPages.Add(new HostConfigurationPageLoader(true));
        }

        /// <inheritdoc />
        public Collection<IOperatorMenuPageLoader> WizardPages { get; }
    }
}
